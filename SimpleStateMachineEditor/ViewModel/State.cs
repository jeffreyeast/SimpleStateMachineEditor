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
    public class State : TransitionHost
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
                if (_stateType != value && IsChangeAllowed())
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
        [Description("The group to which the state belongs")]
        public override Group AssociatedGroup
        {
            get => _associatedGroup;
            set
            {
                if (_associatedGroup != value && IsChangeAllowed())
                {
                    if (_associatedGroup != null)
                    {
                        _associatedGroup.Removing -= AssociatedGroupWasRemovedHandler;
                    }
                    Controller?.LogUndoAction(new UndoRedo.PropertyChangedRecord(Controller, this, "AssociatedGroup", (_associatedGroup?.Id ?? TrackableObject.NullId).ToString()));
                    _associatedGroup = value;
                    if (_associatedGroup != null)
                    {
                        _associatedGroup.Removing += AssociatedGroupWasRemovedHandler;
                    }
                    OnPropertyChanged("AssociatedGroup");
                    Controller?.StateMachine.OnPropertyChanged(this, "AssociatedGroup");
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
        int _associatedGroupId = ObjectModel.TrackableObject.NullId;

        public override bool IsGrouped => AssociatedGroup != null;




        //  Constructor for use by serialization ONLY

        public State()
        {
            TransitionsFrom.CollectionChanged += TransitionsChangedHandler;
            TransitionsTo.CollectionChanged += TransitionsChangedHandler;
        }


        //  Constructor for new object creation through commands

        private State(ViewModelController controller, string rootName, ViewModel.Layer currentLayer) : base (controller, controller.StateMachine.States, rootName, currentLayer)
        {
            using (new UndoRedo.DontLogBlock(controller))
            {
                TransitionsFrom.CollectionChanged += TransitionsChangedHandler;
                TransitionsTo.CollectionChanged += TransitionsChangedHandler;
                StateType = StateTypes.Normal;
            }
        }

        //  Constructor for use by Redo

        internal State(ViewModel.ViewModelController controller, UndoRedo.AddStateRecord redoRecord) : base(controller, redoRecord)
        {
            using (new UndoRedo.DontLogBlock(controller))
            {
                TransitionsFrom.CollectionChanged += TransitionsChangedHandler;
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

        internal override void DeserializeCleanup(DeserializeCleanupPhases phase, ViewModelController controller, StateMachine stateMachine)
        {
            base.DeserializeCleanup(phase, controller, stateMachine);
            if (phase == DeserializeCleanupPhases.ObjectResolution)
            {
                AssociatedGroup = Find(_associatedGroupId) as ViewModel.Group;
            }
        }

        internal override void GetProperty(string propertyName, out string value)
        {
            switch (propertyName)
            {
                case "AssociatedGroup":
                    value = AssociatedGroupId.ToString();
                    break;
                case "StateType":
                    value = StateType.ToString();
                    break;
                default:
                    base.GetProperty(propertyName, out value);
                    break;
            }
        }

        internal override void SetProperty(string propertyName, string newValue)
        {
            switch (propertyName)
            {
                case "AssociatedGroup":
                    AssociatedGroup = Find(int.Parse(newValue)) as ViewModel.Group;
                    break;
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
