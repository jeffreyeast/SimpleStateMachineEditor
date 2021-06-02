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
    public class Group : ObjectModel.LayeredPositionableObject, ObjectModel.ITransitionEndpoint
    {
        public ObservableCollection<Transition> TransitionsFrom { get; private set; }
        public ObservableCollection<Transition> TransitionsTo { get; private set; }

        [XmlIgnore]
        internal ObservableCollection<State> Members { get; private set; }





        //  Constructor for use by serialization ONLY

        public Group()
        {
            TransitionsFrom = new ObservableCollection<Transition>();
            TransitionsTo = new ObservableCollection<Transition>();
            Members = new ObservableCollection<State>();
            Members.CollectionChanged += Members_CollectionChanged;
        }

        //  Constructor for new object creation through commands

        private Group(ViewModelController controller, string rootName, ViewModel.Layer currentLayer) : base (controller, controller.StateMachine.States, rootName, currentLayer)
        {
            TransitionsFrom = new ObservableCollection<Transition>();
            TransitionsTo = new ObservableCollection<Transition>();
            Members = new ObservableCollection<State>();
            Members.CollectionChanged += Members_CollectionChanged;
        }

        //  Constructor for use by Redo

        internal Group(ViewModel.ViewModelController controller, UndoRedo.AddGroupRecord redoRecord) : base(controller, redoRecord)
        {
            TransitionsFrom = new ObservableCollection<Transition>();
            TransitionsTo = new ObservableCollection<Transition>();
            Members = new ObservableCollection<State>();
            Members.CollectionChanged += Members_CollectionChanged;
        }

        private void AddTransitionsForState(State state)
        {
            state.TransitionsFrom.CollectionChanged += MemberTransitionsFromCollectionChangedHandler;
            foreach (Transition transition in state.TransitionsFrom)
            {
                if (!Members.Contains(transition.DestinationState))
                {
                    TransitionsFrom.Add(transition);
                }
            }
            state.TransitionsTo.CollectionChanged += MemberTransitionsToCollectionChangedHandler;
            foreach (Transition transition in state.TransitionsTo)
            {
                if (!Members.Contains(transition.SourceState))
                {
                    TransitionsTo.Add(transition);
                }
            }
        }

        private void BuildTransitionLists()
        {
            while (TransitionsFrom.Count > 0)
            {
                TransitionsFrom.RemoveAt(0);
            }
            while (TransitionsTo.Count > 0)
            {
                TransitionsTo.RemoveAt(0);
            }

            foreach (State state in Members)
            {
                AddTransitionsForState(state);
            }
        }

        internal static Group Create(ViewModelController controller, IconControls.OptionsPropertiesPage optionsPage, ViewModel.Layer currentLayer)
        {
            using (new UndoRedo.DontLogBlock(controller))
            {
                return new Group(controller, optionsPage.GroupRootName, currentLayer);
            }
        }

        private void GatherPeers(List<Transition> peerTransitions, Transition transition, IEnumerable<Transition> transitionList)
        {
            foreach (Transition t in transitionList)
            {
                if ((t.SourceState == transition.SourceState && t.DestinationState == transition.DestinationState) ||
                    (t.SourceState == transition.DestinationState && t.DestinationState == transition.SourceState))
                {
                    if (!peerTransitions.Contains(t))
                    {
                        peerTransitions.Add(t);
                    }
                }
            }
        }

        internal override void GetProperty(string propertyName, out string value)
        {
            switch (propertyName)
            {
                default:
                    base.GetProperty(propertyName, out value);
                    break;
            }
        }

        //  This method calculates the relative position of a transition among all the transitions that originate or
        //  terminate at this state, which share a common opposite state.

        public int GetRelativePeerPosition(Transition transition)
        {
            List<Transition> peerTransitions = new List<Transition>();
            GatherPeers(peerTransitions, transition, TransitionsFrom);
            GatherPeers(peerTransitions, transition, TransitionsTo);

            int position = 0;
            foreach (Transition t in peerTransitions)
            {
                if (t.Id < transition.Id)
                {
                    position++;
                }
            }
            return position;
        }

        public bool HasTransitionMatchingTrigger(EventType triggerEvent)
        {
            if (triggerEvent != null)
            {
                foreach (Transition transition in TransitionsFrom)
                {
                    if (transition.TriggerEvent == triggerEvent && Members.Contains(transition.SourceState))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private void Members_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (State state in e.NewItems)
                    {
                        AddTransitionsForState(state);
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (State state in e.OldItems)
                    {
                        RemoveTransitionsForState(state);
                    }
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        private void MemberTransitionsFromCollectionChangedHandler(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (Transition transition in e.NewItems)
                    {
                        if (!Members.Contains(transition.DestinationState))
                        {
                            TransitionsFrom.Add(transition);
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (Transition transition in e.OldItems)
                    {
                        TransitionsFrom.Remove(transition);
                    }
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        private void MemberTransitionsToCollectionChangedHandler(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (Transition transition in e.NewItems)
                    {
                        if (!Members.Contains(transition.SourceState))
                        {
                            TransitionsTo.Add(transition);
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (Transition transition in e.OldItems)
                    {
                        TransitionsTo.Remove(transition);
                    }
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        private void RemoveTransitionsForState(State state)
        {
            foreach (Transition transition in state.TransitionsFrom)
            {
                TransitionsFrom.Remove(transition);
            }
            foreach (Transition transition in state.TransitionsTo)
            {
                TransitionsFrom.Remove(transition);
            }
        }

        internal override void SetProperty(string propertyName, string newValue)
        {
            switch (propertyName)
            {
                default:
                    base.SetProperty(propertyName, newValue);
                    break;
            }
        }
    }
}
