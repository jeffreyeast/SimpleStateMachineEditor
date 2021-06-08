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
        public override Group AssociatedGroup { get => null; set => throw new NotImplementedException(); }
        public override bool IsGrouped => false;



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
            endpoint.TransitionsFrom.CollectionChanged += MemberTransitionsFromCollectionChangedHandler;
            foreach (ObjectModel.ITransition transition in endpoint.TransitionsFrom)
            {
                if (!(CoNamedObject as Layer).Members.Contains(transition.DestinationState) &&
                    (CoNamedObject as Layer).Members.Contains(transition.SourceState))
                {
                    GroupTransition groupTransition = TransitionsFrom.Where(t => t.SourceState == this && t.DestinationState == transition.DestinationState && t.TriggerEvent == transition.TriggerEvent).FirstOrDefault() as GroupTransition;
                    if (groupTransition == null)
                    {
                        TransitionsFrom.Add(new GroupTransition(Controller, this, transition.DestinationState, transition.TriggerEvent));
                    }
                }
            }
            endpoint.TransitionsTo.CollectionChanged += MemberTransitionsToCollectionChangedHandler;
            foreach (ObjectModel.ITransition transition in endpoint.TransitionsTo)
            {
                if (!(CoNamedObject as Layer).Members.Contains(transition.SourceState) &&
                    (CoNamedObject as Layer).Members.Contains(transition.DestinationState))
                {
                    GroupTransition groupTransition = TransitionsFrom.Where(t => t.SourceState == transition.SourceState && t.DestinationState == this && t.TriggerEvent == transition.TriggerEvent).FirstOrDefault() as GroupTransition;
                    if (groupTransition == null)
                    {
                        TransitionsTo.Add(new GroupTransition(Controller, transition.SourceState, this, transition.TriggerEvent));
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

        private void Members_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            ValidateTransitions();
        }

        private void MemberTransitionsFromCollectionChangedHandler(object sender, NotifyCollectionChangedEventArgs e)
        {
            ValidateTransitions();
        }

        private void MemberTransitionsToCollectionChangedHandler(object sender, NotifyCollectionChangedEventArgs e)
        {
            ValidateTransitions();
        }

        protected override void OnCoNamedObjectChange(NamedObject preValue, NamedObject postValue)
        {
            if (preValue != null)
            {
                (preValue as Layer).Members.CollectionChanged -= Members_CollectionChanged;
                foreach (ViewModel.State member in (preValue as Layer).Members)
                {
                    member.AssociatedGroup = null;
                }
            }
            if (postValue != null)
            {
                (postValue as Layer).Members.CollectionChanged += Members_CollectionChanged;
                foreach (ViewModel.State member in (postValue as Layer).Members)
                {
                    member.AssociatedGroup = null;
                }
            }
            ValidateTransitions();
            base.OnCoNamedObjectChange(preValue, postValue);
        }

        private void ValidateTransitions()
        {

        }
    }
}
