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
        [XmlAttribute]
        public StateTypes StateType
        {
            get => _stateType;
            set
            {
                if (_stateType != value)
                {
                    using (new ViewModelController.GuiChangeBlock(Controller))
                    {
                        Controller?.LogUndoAction(new UndoRedo.PropertyChangedRecord(Controller, this, "StateType", _stateType.ToString()));
                        _stateType = value;
                        OnPropertyChanged("StateType");
                    }
                }
            }
        }
        StateTypes _stateType;

        [ReadOnly(true)]
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

        internal static State Create(ViewModelController controller, IconControls.OptionsPropertiesPage optionsPage, ViewModel.Layer currentLayer)
        {
            using (new UndoRedo.DontLogBlock(controller))
            {
                return new State(controller, optionsPage.StateRootName, currentLayer);
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
