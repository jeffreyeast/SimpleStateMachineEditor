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
    //      The State class represents a state.
    //--
    public class State : ObjectModel.LayeredPositionableObject, ObjectModel.ITransitionEndpoint
    {
        public enum StateTypes
        {
            Normal,
            Finish,
            Error,
        }
        public StateTypes StateType
        {
            get => _stateType;
            set
            {
                if (_stateType != value && IsChangeAllowed)
                {
                    Controller?.LogUndoAction(new UndoRedo.PropertyChangedRecord(Controller, this, "StateType", _stateType.ToString()));
                    _stateType = value;
                    OnPropertyChanged("StateType");
                    EndChange();
                }
            }
        }
        StateTypes _stateType;

        [XmlIgnore]
        public bool IsStartState
        {
            get => _isStartState;
            set
            {
                if (_isStartState != value)
                {
                    _isStartState = value;
                    OnPropertyChanged("IsStartState");
                }
            }
        }
        bool _isStartState;

        [XmlIgnore]
        [Browsable(false)]
        public ObservableCollection<Transition> TransitionsFrom { get; private set; }
        [XmlIgnore]
        [Browsable(false)]
        public ObservableCollection<Transition> TransitionsTo { get; private set; }


        [XmlIgnore]
        [Description("The group to which the state belongs")]
        public Group AssociatedGroup
        {
            get => _associatedGroup;
            set
            {
                if (_associatedGroup != value && IsChangeAllowed)
                {
                    if (_associatedGroup != null)
                    {
                        _associatedGroup.Removing -= AssociatedGroupWasRemovedHandler;
                    }
                    Controller?.LogUndoAction(new UndoRedo.PropertyChangedRecord(Controller, this, "AssociatedGroup", _associatedGroup?.Id.ToString() ?? ((int)-1).ToString()));
                    _associatedGroup = value;
                    if (_associatedGroup != null)
                    {
                        _associatedGroup.Removing += AssociatedGroupWasRemovedHandler;
                    }
                    OnPropertyChanged("AssociatedGroup");
                    OnPropertyChanged("IsGrouped");
                    EndChange();
                }
            }
        }
        Group _associatedGroup;

        [Browsable(false)]
        [XmlAttribute]
        public int AssociatedGroupId
        {
            get => AssociatedGroup?.Id ?? _associatedGroupId;
            set => _associatedGroupId = value;
        }
        int _associatedGroupId = -1;

        public bool IsGrouped => AssociatedGroup != null;




        //  Constructor for use by serialization ONLY

        public State()
        {
            TransitionsFrom = new ObservableCollection<Transition>();
            TransitionsFrom.CollectionChanged += TransitionsChangedHandler;
            TransitionsTo = new ObservableCollection<Transition>();
            TransitionsTo.CollectionChanged += TransitionsChangedHandler;
        }


        //  Constructor for new object creation through commands

        private State(ViewModelController controller, string rootName, ViewModel.Layer currentLayer) : base (controller, controller.StateMachine.States, rootName, currentLayer)
        {
            using (new UndoRedo.DontLogBlock(controller))
            {
                TransitionsFrom = new ObservableCollection<Transition>();
                TransitionsFrom.CollectionChanged += TransitionsChangedHandler;
                TransitionsTo = new ObservableCollection<Transition>();
                TransitionsTo.CollectionChanged += TransitionsChangedHandler;
                StateType = StateTypes.Normal;
            }
        }

        //  Constructor for use by Redo

        internal State(ViewModel.ViewModelController controller, UndoRedo.AddStateRecord redoRecord) : base(controller, redoRecord)
        {
            using (new UndoRedo.DontLogBlock(controller))
            {
                TransitionsFrom = new ObservableCollection<Transition>();
                TransitionsFrom.CollectionChanged += TransitionsChangedHandler;
                TransitionsTo = new ObservableCollection<Transition>();
                TransitionsTo.CollectionChanged += TransitionsChangedHandler;
                StateType = redoRecord.StateType;
            }
        }

        private void AssociatedGroupWasRemovedHandler(IRemovableObject item)
        {
            AssociatedGroup = null;
        }

        internal static State Create(ViewModelController controller, IconControls.OptionsPropertiesPage optionsPage, ViewModel.Layer currentLayer)
        {
            using (new UndoRedo.DontLogBlock(controller))
            {
                return new State(controller, optionsPage.StateRootName, currentLayer);
            }
        }

        internal override void DeserializeCleanup(ViewModelController controller, StateMachine stateMachine)
        {
            base.DeserializeCleanup(controller, stateMachine);
            AssociatedGroup = stateMachine.Find(_associatedGroupId) as ViewModel.Group;
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
                case "StateType":
                    value = StateType.ToString();
                    break;
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
            GatherPeers(peerTransitions, transition, TransitionsTo);
            GatherPeers(peerTransitions, transition, TransitionsFrom);

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
                    if (transition.TriggerEvent == triggerEvent)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        internal override void SetProperty(string propertyName, string newValue)
        {
            switch (propertyName)
            {
                case "StateType":
                    StateType = (StateTypes)Enum.Parse(typeof(StateTypes), newValue);
                    break;
                default:
                    base.SetProperty(propertyName, newValue);
                    break;
            }
        }

        private void TransitionsChangedHandler(object sender, NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged("Transitions");
        }
    }
}
