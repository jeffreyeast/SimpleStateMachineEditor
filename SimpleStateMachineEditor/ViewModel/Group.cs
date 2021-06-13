using Microsoft.VisualStudio.Shell;
using SimpleStateMachineEditor.ObjectModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Serialization;

namespace SimpleStateMachineEditor.ViewModel
{
    //++
    //      The Group class represents a group of states.
    //--
    public class Group : TransitionHost
    {
        internal Layer Layer => CoNamedObject as Layer;
        internal ObservableCollection<ObjectModel.ITransitionEndpoint> Members => Layer?.Members;
        int ValidationPendingCount = 0;

        internal enum MembershipChangeAction
        {
            Add,
            Remove,
            Change,
            AddTransition,
            RemoveTransition,
        }

        internal struct MembershipChangeArgument
        {
            internal MembershipChangeAction Action;
            internal ObjectModel.ITransitionEndpoint Endpoint;
            internal ObjectModel.ITransition Transition;

            internal MembershipChangeArgument(MembershipChangeAction action, ObjectModel.ITransitionEndpoint endpoint)
            {
                Action = action;
                Endpoint = endpoint;
                Transition = null;
            }

            internal MembershipChangeArgument(MembershipChangeAction action, ObjectModel.ITransition transition)
            {
                Action = action;
                Endpoint = null;
                Transition = transition;
            }
        }

        internal delegate void MembershipChangeHandler(Group sender, MembershipChangeArgument e);
        internal event MembershipChangeHandler MembershipChanged;





        //  Constructor for use by serialization ONLY

        public Group()
        {
        }

        //  Constructor for new object creation through commands

        private Group(ViewModelController controller, string rootName, ViewModel.Layer currentLayer) : base (controller, controller.StateMachine.Groups, rootName, currentLayer)
        {
        }

        //  Constructor for use by Redo

        internal Group(ViewModel.ViewModelController controller, UndoRedo.AddGroupRecord redoRecord) : base(controller, redoRecord)
        {
        }

        private void AddTransitionsForEndpoint(ObjectModel.ITransitionEndpoint endpoint)
        {
            endpoint.TransitionsFrom.CollectionChanged += MemberTransitionsCollectionChangedHandler;
            foreach (ObjectModel.ITransition transition in endpoint.TransitionsFrom)
            {
                if (transition.SourceState != null && transition.DestinationState != null)
                {
                    ObjectModel.LayerPosition layerPosition = transition.DestinationState.LayerPositions.Where(lp => lp.Layer == Layer).FirstOrDefault();
                    if (Members.Contains(transition.SourceState) &&
                        this != transition.DestinationState &&
                        (layerPosition == null || layerPosition.GroupStatus != LayerPosition.GroupStatuses.Explicit))
                    {
                        Transition groupTransition = TransitionsFrom.FirstOrDefault(t => t.SourceState == this && t.DestinationState == transition.DestinationState && t.TriggerEvent == transition.TriggerEvent) as Transition;
                        if (groupTransition == null)
                        {
                            groupTransition = new Transition(Controller, this, transition.DestinationState, transition.TriggerEvent);
                            MembershipChanged?.Invoke(this, new MembershipChangeArgument(MembershipChangeAction.AddTransition, groupTransition));
                        }
                        groupTransition.IsValid = true;
                    }
                }
            }
            endpoint.TransitionsTo.CollectionChanged += MemberTransitionsCollectionChangedHandler;
            foreach (ObjectModel.ITransition transition in endpoint.TransitionsTo)
            {
                if (transition.SourceState != null && transition.DestinationState != null)
                {
                    ObjectModel.LayerPosition layerPosition = transition.SourceState.LayerPositions.Where(lp => lp.Layer == Layer).FirstOrDefault();
                    if (Members.Contains(transition.DestinationState) &&
                        this != transition.SourceState &&
                        (layerPosition == null || layerPosition.GroupStatus != LayerPosition.GroupStatuses.Explicit))
                    {
                        Transition groupTransition = TransitionsTo.FirstOrDefault(t => t.SourceState == transition.SourceState && t.DestinationState == this && t.TriggerEvent == transition.TriggerEvent) as Transition;
                        if (groupTransition == null)
                        {
                            groupTransition = new Transition(Controller, transition.SourceState, this, transition.TriggerEvent);
                            MembershipChanged?.Invoke(this, new MembershipChangeArgument(MembershipChangeAction.AddTransition, groupTransition));
                        }
                        groupTransition.IsValid = true;
                    }
                }
            }
        }

        internal static Group Create(ViewModelController controller, IconControls.OptionsPropertiesPage optionsPage, ViewModel.Layer currentLayer)
        {
            using (new UndoRedo.DontLogBlock(controller))
            {
                return new Group(controller, optionsPage.GroupRootName, currentLayer);
            }
        }

        private void DeleteInvalidTransitions(ObservableCollection<ObjectModel.ITransition> transitionList)
        {
            IEnumerable<ObjectModel.ITransition> invalidTransitions = transitionList.Where(t => t.TransitionType == ViewModel.Transition.TransitionTypes.Group && !t.IsValid).ToArray();
            foreach (Transition groupTransition in invalidTransitions)
            {
                groupTransition.Remove();
                MembershipChanged?.Invoke(this, new MembershipChangeArgument(MembershipChangeAction.RemoveTransition, groupTransition));
            }
        }

        private void Members_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    if (e.NewItems.Count == 1 && e.NewItems[0] is ObjectModel.ITransitionEndpoint endpoint)
                    {
                        MonitorEndpoint(endpoint);
                        System.Threading.Interlocked.Increment(ref ValidationPendingCount);
                        ThreadHelper.Generic.BeginInvoke(new System.Action(() =>
                        {
                            if (System.Threading.Interlocked.Exchange(ref ValidationPendingCount, 0) > 0)
                            {
                                ValidateTransitions();
                            }
                            MembershipChanged?.Invoke(this, new MembershipChangeArgument(MembershipChangeAction.Add, endpoint));
                        }));
                        break;
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                case NotifyCollectionChangedAction.Remove:
                    if (e.OldItems.Count == 1 && e.OldItems[0] is ObjectModel.ITransitionEndpoint endpoint1)
                    {
                        UnmonitorEndpoint(endpoint1);
                        System.Threading.Interlocked.Increment(ref ValidationPendingCount);
                        ThreadHelper.Generic.BeginInvoke(new System.Action(() =>
                        {
                            if (System.Threading.Interlocked.Exchange(ref ValidationPendingCount, 0) > 0)
                            {
                                ValidateTransitions();
                            }
                            MembershipChanged?.Invoke(this, new MembershipChangeArgument(MembershipChangeAction.Remove, endpoint1));
                        }));
                        break;
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                default:
                    throw new NotImplementedException();
            }
        }

        private void MemberPropertyChangedHandler(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "GroupStatus":
                    if (sender is ObjectModel.ITransitionEndpoint endpoint)
                    {
                        ValidateTransitions();
                        MembershipChanged?.Invoke(this, new MembershipChangeArgument(MembershipChangeAction.Change, endpoint));
                    }
                    break;
                default:
                    break;
            }
        }

        private void MemberTransitionsCollectionChangedHandler(object sender, NotifyCollectionChangedEventArgs e)
        {
            System.Threading.Interlocked.Increment(ref ValidationPendingCount);
            ThreadHelper.Generic.BeginInvoke(new System.Action(() =>
            {
                if (System.Threading.Interlocked.Exchange(ref ValidationPendingCount, 0) > 0)
                {
                    ValidateTransitions();
                }
                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        if (e.NewItems.Count == 1 && e.NewItems[0] is ObjectModel.ITransition transition)
                        {
                            if (transition.DestinationState != null && transition.SourceState != null)
                            {
                                transition.PropertyChanged += TransitionPropertyChangedHandler;
                                MembershipChanged?.Invoke(this, new MembershipChangeArgument(MembershipChangeAction.AddTransition, transition));
                            }
                            break;
                        }
                        else
                        {
                            throw new NotImplementedException();
                        }
                    case NotifyCollectionChangedAction.Remove:
                        if (e.OldItems.Count == 1 && e.OldItems[0] is ObjectModel.ITransition transition1)
                        {
                            if (transition1.DestinationState != null && transition1.SourceState != null)
                            {
                                transition1.PropertyChanged -= TransitionPropertyChangedHandler;
                                MembershipChanged?.Invoke(this, new MembershipChangeArgument(MembershipChangeAction.RemoveTransition, transition1));
                            }
                            break;
                        }
                        else
                        {
                            throw new NotImplementedException();
                        }
                    default:
                        throw new NotImplementedException();
                }
            }));
        }

        private void MonitorEndpoint(ObjectModel.ITransitionEndpoint endpoint)
        {
            endpoint.PropertyChanged += MemberPropertyChangedHandler;
            endpoint.TransitionsFrom.CollectionChanged += MemberTransitionsCollectionChangedHandler;
            endpoint.TransitionsTo.CollectionChanged += MemberTransitionsCollectionChangedHandler;
            foreach (ObjectModel.ITransition transition in endpoint.TransitionsFrom)
            {
                transition.PropertyChanged += TransitionPropertyChangedHandler;
            }
        }

        protected override void OnCoNamedObjectChange(NamedObject preValue, NamedObject postValue)
        {
            if (preValue != null)
            {
                (preValue as Layer).Members.CollectionChanged -= Members_CollectionChanged;
                foreach (ViewModel.TransitionHost endpoint in (preValue as Layer).Members)
                {
                    UnmonitorEndpoint(endpoint);
                }
            }
            if (postValue != null)
            {
                (postValue as Layer).Members.CollectionChanged += Members_CollectionChanged;
                foreach (ViewModel.TransitionHost endpoint in (postValue as Layer).Members)
                {
                    MonitorEndpoint(endpoint);
                }
            }
            base.OnCoNamedObjectChange(preValue, postValue);
        }

        private void TransitionPropertyChangedHandler(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "TriggerEvent":
                    ValidateTransitions();
                    break;
                default:
                    break;
            }
        }

        private void UnmonitorEndpoint(ObjectModel.ITransitionEndpoint endpoint)
        {
            endpoint.PropertyChanged -= MemberPropertyChangedHandler;
            endpoint.TransitionsFrom.CollectionChanged -= MemberTransitionsCollectionChangedHandler;
            endpoint.TransitionsTo.CollectionChanged -= MemberTransitionsCollectionChangedHandler;
            foreach (ObjectModel.ITransition transition in endpoint.TransitionsFrom)
            {
                transition.PropertyChanged -= TransitionPropertyChangedHandler;
            }
        }

        private void ValidateTransitions()
        {
            using (new UndoRedo.DontLogBlock(Controller))
            {
                //  First, mark all the transitions for potential removal

                foreach (Transition groupTransition in TransitionsFrom.Where(t => t.TransitionType == Transition.TransitionTypes.Group))
                {
                    groupTransition.IsValid = false;
                }
                foreach (Transition groupTransition in TransitionsTo.Where(t => t.TransitionType == Transition.TransitionTypes.Group))
                {
                    groupTransition.IsValid = false;
                }

                //  Now figure what transitions we actually need

                if (Members != null)
                {
                    foreach (ObjectModel.ITransitionEndpoint endpoint in Members.Where(m => m.LayerPositions.Any(lp => lp.Layer == Layer && lp.GroupStatus == LayerPosition.GroupStatuses.Explicit)))
                    {
                        AddTransitionsForEndpoint(endpoint);
                    }
                }

                //  And finally, get rid of those not needed

                DeleteInvalidTransitions(TransitionsFrom);
                DeleteInvalidTransitions(TransitionsTo);
            }
            System.Threading.Interlocked.Exchange(ref ValidationPendingCount, 0);
        }
    }
}
