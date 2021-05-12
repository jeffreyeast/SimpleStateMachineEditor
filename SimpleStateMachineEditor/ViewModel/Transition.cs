using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SimpleStateMachineEditor.ViewModel
{
    //++
    //  The Transition class represents a transition from one state to another, based on a triggering event. A set of
    //  actions is executed as part of the transition.
    //--
    public class Transition : ObjectModel.TrackableObject
    {
        //  The design pattern for references to other view model objects is to use a reference to the object at runtime, but to serialize only the objects ID.
        //  This is done to enable the serializer to generate a tree, even through the actual data structure is a graph.  At deserialization
        //  time, we go back and set up the object references, using the IDs to identify the object.

        [XmlIgnore]
        [Description("The state sourcing the event that will invoke the transition")]
        public State SourceState 
        {
            get => _sourceState;
            set
            {
                if (_sourceState != value && IsChangeAllowed)
                {
                    if (_sourceState != null)
                    {
                        _sourceState.PropertyChanged -= EndpointPropertyChangedHandler;
                        _sourceState.TransitionsFrom.Remove(this);
                    }
                    if (Controller?.LoggingIsEnabled ?? false)
                    {
                        Controller?.UndoManager.Add(new UndoRedo.PropertyChangedRecord(Controller, this, "SourceState", _sourceState?.Id.ToString() ?? ((int)-1).ToString()));
                    }
                    _sourceState = value;
                    if (_sourceState != null)
                    {
                        _sourceState.PropertyChanged += EndpointPropertyChangedHandler;
                        _sourceState.TransitionsFrom.Add(this);
                    }
                    OnEndpointChanged();
                    OnPropertyChanged("SourceState");
                    EndChange();
                }
            }
        }
        State _sourceState;
        [XmlAttribute]
        [Browsable(false)]
        public int SourceStateId
        {
            get => SourceState?.Id ?? _sourceStateId;
            set => _sourceStateId = value;
        }
        int _sourceStateId = -1;

        [XmlIgnore]
        [Description("The state which will be transitioned to")]
        public State DestinationState 
        {
            get => _destinationState;
            set
            {
                if (_destinationState != value && IsChangeAllowed)
                {
                    if (_destinationState != null)
                    {
                        _destinationState.PropertyChanged -= EndpointPropertyChangedHandler;
                        _destinationState.TransitionsTo.Remove(this);
                    }
                    if (Controller?.LoggingIsEnabled ?? false)
                    {
                        Controller?.UndoManager.Add(new UndoRedo.PropertyChangedRecord(Controller, this, "DestinationState", _destinationState?.Id.ToString() ?? ((int)-1).ToString()));
                    }
                    _destinationState = value;
                    if (_destinationState != null)
                    {
                        _destinationState.PropertyChanged += EndpointPropertyChangedHandler;
                        _destinationState.TransitionsTo.Add(this);
                    }
                    OnEndpointChanged();
                    OnPropertyChanged("DestinationState");
                    EndChange();
                }
            }
        }
        State _destinationState;
        [Browsable(false)]
        [XmlAttribute]
        public int DestinationStateId
        {
            get => DestinationState?.Id ?? _destinationStateId;
            set => _destinationStateId = value;
        }
        int _destinationStateId = -1;

        [XmlIgnore]
        [Description("The event which will trigger the transition")]
        public EventType TriggerEvent 
        {
            get => _triggerEvent;
            set
            {
                if (_triggerEvent != value && IsChangeAllowed)
                {
                    if (Controller?.LoggingIsEnabled ?? false)
                    {
                        Controller?.UndoManager.Add(new UndoRedo.PropertyChangedRecord(Controller, this, "TriggerEvent", _triggerEvent?.Id.ToString() ?? ((int)-1).ToString()));
                    }
                    _triggerEvent = value;
                    OnPropertyChanged("TriggerEvent");
                    EndChange();
                }
            }
        }
        EventType _triggerEvent;
        [Browsable(false)]
        [XmlAttribute]
        public int TriggerEventId
        {
            get => TriggerEvent?.Id ?? _triggerEventId;
            set => _triggerEventId = value;
        }
        int _triggerEventId = -1;

        public ObservableCollection<string> Actions { get; private set; }
        List<string> OldActions;

        [Browsable(false)]
        public string ActionNames
        {
            get
            {
                string actionNames = "";
                string separator = "";

                foreach (string actionName in Actions)
                {
                    actionNames += separator + actionName;
                    separator = ",";
                }

                return actionNames;
            }
        }

        //  Events posted for use by the icon

        internal event EventHandler EndpointChanged;
        internal event EventHandler EndpointPositionChanged;



        //  Constructor for use by serialization ONLY

        public Transition()
        {
            Actions = new ObservableCollection<string>();
            OldActions = new List<string>();
            Actions.CollectionChanged += ActionNameCollectionChangedHandler;
        }

        //  Internal constructor for general use

        internal Transition(ViewModelController controller, ViewModel.State sourceState) : base (controller)
        {
            using (new UndoRedo.DontLogBlock(controller))
            {
                Actions = new ObservableCollection<string>();
                Actions.CollectionChanged += ActionNameCollectionChangedHandler;
                OldActions = new List<string>();
                _sourceState = sourceState;
                _sourceState.TransitionsFrom.Add(this);
                _sourceState.PropertyChanged += EndpointPropertyChangedHandler;
            }
        }

        //  Constructor for use by Redo

        internal Transition(ViewModelController controller, UndoRedo.AddTransitionRecord redoRecord) : base(controller, redoRecord)
        {
            using (new UndoRedo.DontLogBlock(controller))
            {
                SourceState = redoRecord.SourceStateId == -1 ? null : Controller.StateMachine.Find(redoRecord.SourceStateId) as State;
                DestinationState = redoRecord.DestinationStateId == -1 ? null : Controller.StateMachine.Find(redoRecord.DestinationStateId) as State;
                TriggerEvent = redoRecord.TriggerEventId == -1 ? null : Controller.StateMachine.Find(redoRecord.TriggerEventId) as EventType;
                Actions = new ObservableCollection<string>(redoRecord.Actions);
                Actions.CollectionChanged += ActionNameCollectionChangedHandler;
                OldActions = new List<string>();
            }
        }

        private void ActionNameCollectionChangedHandler(object sender, NotifyCollectionChangedEventArgs e)
        {
            //  TODO TODO TODO
            //
            //  We should have synchronized with the underlying text buffer before we made the change.

            if (IsChangeAllowed)
            {
                if (Controller?.LoggingIsEnabled ?? false)
                {
                    Controller?.UndoManager.Add(new UndoRedo.ListValuedPropertyChangedRecord(Controller, this, "Actions", OldActions));
                }
                OldActions = new List<string>(Actions);
                EndChange();
            }
            OnPropertyChanged("ActionNames");
        }

        internal override void DeserializeCleanup(ViewModelController controller, ViewModel.StateMachine stateMachine)
        {
            base.DeserializeCleanup(controller, stateMachine);

            using (new UndoRedo.DontLogBlock(controller))
            {
                _destinationState = stateMachine.Find(_destinationStateId) as ViewModel.State;
                _destinationState.PropertyChanged += EndpointPropertyChangedHandler;
                _destinationState.TransitionsTo.Add(this);

                _sourceState = stateMachine.Find(_sourceStateId) as ViewModel.State;
                _sourceState.PropertyChanged += EndpointPropertyChangedHandler;
                _sourceState.TransitionsFrom.Add(this);

                _triggerEvent = stateMachine.Find(_triggerEventId) as ViewModel.EventType;
            }
        }

        private void EndpointPropertyChangedHandler(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "LeftTopPosition" || e.PropertyName == "Transitions")
            {
                EndpointPositionChanged?.Invoke(this, new EventArgs());
            }
        }

        internal override void GetProperty(string propertyName, out IEnumerable<string> value)
        {
            switch (propertyName)
            {
                case "Actions":
                    value = Actions.ToArray<string>();
                    break;
                default:
                    base.GetProperty(propertyName, out value);
                    break;
            }
        }

        internal override void GetProperty(string propertyName, out string value)
        {
            switch (propertyName)
            {
                case "DestinationState":
                    value = DestinationState?.Id.ToString() ?? ((int)-1).ToString();
                    break;
                case "SourceState":
                    value = SourceState?.Id.ToString() ?? ((int)-1).ToString();
                    break;
                case "TriggerEvent":
                    value = TriggerEvent?.Id.ToString() ?? ((int)-1).ToString();
                    break;
                default:
                    base.GetProperty(propertyName, out value);
                    break;
            }
        }

        private void OnEndpointChanged()
        {
            EndpointChanged?.Invoke(this, new EventArgs());
        }

        private void SetActionNamesFromTextString(string actionNames)
        {
            Actions.Clear();
            int start = 0;
            while (start < actionNames.Length)
            {
                int delimiterPosition = actionNames.IndexOf(',', start);
                if (delimiterPosition == -1)
                {
                    delimiterPosition = actionNames.Length;
                }
                Actions.Add(actionNames.Substring(start, delimiterPosition - start));
                start = delimiterPosition + 1;
            }
        }

        internal override void OnRemoving()
        {
            if (DestinationState != null)
            {
                _destinationState.PropertyChanged -= EndpointPropertyChangedHandler;
                _destinationState.TransitionsTo.Remove(this);
            }
            if (SourceState != null)
            {
                _sourceState.PropertyChanged -= EndpointPropertyChangedHandler;
                _sourceState.TransitionsFrom.Remove(this);
            }
            base.OnRemoving();
        }

        internal override void SetProperty(string propertyName, string newValue)
        {
            switch (propertyName)
            {
                case "DestinationState":
                    DestinationState = newValue == "-1" ? null : Controller.StateMachine.Find(int.Parse(newValue)) as ViewModel.State;
                    break;
                case "SourceState":
                    SourceState = newValue == "-1" ? null : Controller.StateMachine.Find(int.Parse(newValue)) as ViewModel.State;
                    break;
                case "TriggerEvent":
                    TriggerEvent = newValue == "-1" ? null : Controller.StateMachine.Find(int.Parse(newValue)) as ViewModel.EventType;
                    break;
                default:
                    base.SetProperty(propertyName, newValue);
                    break;
            }
        }

        internal override void SetProperty(string propertyName, IEnumerable<string> newValue)
        {
            switch (propertyName)
            {
                case "Actions":
                    using (new UndoRedo.AtomicBlock(Controller, "Set property Actions"))
                    {
                        Actions.Clear();
                        foreach (string v in newValue)
                        {
                            Actions.Add(v);
                        }
                    }
                    break;
                default:
                    base.SetProperty(propertyName, newValue);
                    break;
            }
        }
    }
}
