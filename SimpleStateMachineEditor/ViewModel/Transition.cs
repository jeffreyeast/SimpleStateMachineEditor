using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
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
    public class Transition : ObjectModel.DocumentedObject, ObjectModel.ITransition
    {
        //  The design pattern for references to other view model objects is to use a reference to the object at runtime, but to serialize only the objects ID.
        //  This is done to enable the serializer to generate a tree, even through the actual data structure is a graph.  At deserialization
        //  time, we go back and set up the object references, using the IDs to identify the object.

        [XmlIgnore]
        [Description("The state sourcing the event that will invoke the transition")]
        public ObjectModel.ITransitionEndpoint SourceState 
        {
            get => _sourceState;
            set
            {
                if (_sourceState != value)
                {
                    using (new ViewModelController.GuiChangeBlock(Controller))
                    {
                        ObjectModel.ITransitionEndpoint oldSourceState = _sourceState;
                        if (oldSourceState != null)
                        {
                            oldSourceState.PropertyChanged -= EndpointPropertyChangedHandler;
                        }
                        Controller?.LogUndoAction(new UndoRedo.PropertyChangedRecord(Controller, this, "SourceState", (oldSourceState?.Id ?? ObjectModel.TrackableObject.NullId).ToString()));
                        _sourceState = value;
                        if (oldSourceState != null)
                        {
                            oldSourceState.TransitionsFrom.Remove(this);
                        }
                        if (_sourceState == null)
                        {
                            SourceStateId = ObjectModel.TrackableObject.NullId;
                        }
                        else
                        {
                            _sourceState.PropertyChanged += EndpointPropertyChangedHandler;
                            _sourceState.TransitionsFrom.Add(this);
                        }
                        OnEndpointChanged();
                        OnPropertyChanged("SourceState");
                    }
                }
            }
        }
        ObjectModel.ITransitionEndpoint _sourceState;
        [XmlAttribute]
        [Browsable(false)]
        public int SourceStateId
        {
            get => SourceState?.Id ?? _sourceStateId;
            set => _sourceStateId = value;
        }
        int _sourceStateId = ObjectModel.TrackableObject.NullId;

        [XmlIgnore]
        [Description("The state which will be transitioned to")]
        public ObjectModel.ITransitionEndpoint DestinationState 
        {
            get => _destinationState;
            set
            {
                if (_destinationState != value)
                {
                    using (new ViewModelController.GuiChangeBlock(Controller))
                    {
                        ObjectModel.ITransitionEndpoint oldDestinationState = _destinationState;
                        if (oldDestinationState != null)
                        {
                            oldDestinationState.PropertyChanged -= EndpointPropertyChangedHandler;
                        }
                        Controller?.LogUndoAction(new UndoRedo.PropertyChangedRecord(Controller, this, "DestinationState", (oldDestinationState?.Id ?? ObjectModel.TrackableObject.NullId).ToString()));
                        _destinationState = value;
                        if (oldDestinationState != null)
                        {
                            oldDestinationState.TransitionsTo.Remove(this);
                        }
                        if (_destinationState == null)
                        {
                            DestinationStateId = ObjectModel.TrackableObject.NullId;
                        }
                        else
                        {
                            _destinationState.PropertyChanged += EndpointPropertyChangedHandler;
                            _destinationState.TransitionsTo.Add(this);
                        }
                        OnEndpointChanged();
                        OnPropertyChanged("DestinationState");
                    }
                }
            }
        }
        ObjectModel.ITransitionEndpoint _destinationState;
        [Browsable(false)]
        [XmlAttribute]
        public int DestinationStateId
        {
            get => DestinationState?.Id ?? _destinationStateId;
            set => _destinationStateId = value;
        }
        int _destinationStateId = ObjectModel.TrackableObject.NullId;

        [XmlIgnore]
        [Description("The event which will trigger the transition")]
        public EventType TriggerEvent 
        {
            get => _triggerEvent;
            set
            {
                if (_triggerEvent != value)
                {
                    using (new ViewModelController.GuiChangeBlock(Controller))
                    {
                        Controller?.LogUndoAction(new UndoRedo.PropertyChangedRecord(Controller, this, "TriggerEvent", (_triggerEvent?.Id ?? ObjectModel.TrackableObject.NullId).ToString()));
                        _triggerEvent = value;
                        if (_triggerEvent == null)
                        {
                            TriggerEventId = ObjectModel.TrackableObject.NullId;
                        }
                        OnPropertyChanged("TriggerEvent");
                    }
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
        int _triggerEventId = ObjectModel.TrackableObject.NullId;

        public enum TransitionTypes
        {
            Normal,
            Group,
        }

        [XmlAttribute]
        [Browsable(false)]
        public TransitionTypes TransitionType { get;  set; }


        //  Previously, actions were simply string names of abstract methods. Now they're represented by the
        //  Action object.
        [Browsable(false)]
        [XmlArray(ElementName = "Actions")]
        public ObservableCollection<string> DeprecatedActions { get; private set; }

        [Browsable(false)]
        [XmlIgnore]
        public ObservableCollection<ActionReference> ActionReferences { get; private set; }

        [Browsable(false)]
        public List<int> ActionIds
        {
            get => ActionReferences?.Select(m => m.Action.Id).ToList() ?? _actionIds;
            private set => _actionIds = value;
        }
        List<int> _actionIds;

        List<string> OldActions
        {
            get => _oldActions;
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException();
                }
                if (_oldActions != value)
                {
                    _oldActions = value;
                }
            }
        }
        List<string> _oldActions;

        [XmlIgnore]
        [Browsable(false)]
        public bool WasActionFound
        {
            get => _wasActionFound;
            set
            {
                if (_wasActionFound != value)
                {
                    _wasActionFound = value;
                    OnPropertyChanged("WasActionFound");
                }
            }
        }
        bool _wasActionFound;

        [XmlIgnore]
        [Browsable(false)]
        public bool WasTriggerFound
        {
            get => _wasTriggerFound;
            set
            {
                if (_wasTriggerFound != value)
                {
                    _wasTriggerFound = value;
                    OnPropertyChanged("WasTriggerFound");
                }
            }
        }
        bool _wasTriggerFound;



        //  Events posted for use by the icon

        internal event EventHandler EndpointChanged;
        public event EventHandler EndpointPositionChanged;



        //  Constructor for use by serialization ONLY

        public Transition()
        {
            DeprecatedActions = new ObservableCollection<string>();
            OldActions = new List<string>();
            ActionIds = new List<int>();
            TransitionType = TransitionTypes.Normal;
        }

        //  Internal constructor for general use

        internal Transition(ViewModelController controller, ViewModel.State sourceState) : base (controller)
        {
            using (new UndoRedo.DontLogBlock(controller))
            {
                //  The order of these operations is significant -- we first set up the object, then perform the membership operations

                DeprecatedActions = new ObservableCollection<string>();
                OldActions = new List<string>();
                _sourceState = sourceState;
                _sourceState.PropertyChanged += EndpointPropertyChangedHandler;
                ActionReferences = new ObservableCollection<ActionReference>();
                ActionReferences.CollectionChanged += Actions_CollectionChanged;
                ActionIds = null;
                TransitionType = TransitionTypes.Normal;

                _sourceState.TransitionsFrom.Add(this);
            }
        }

        internal Transition(ViewModelController controller, ObjectModel.ITransitionEndpoint source, ObjectModel.ITransitionEndpoint destination, ViewModel.EventType triggerEvent) : base(controller)
        {
            using (new UndoRedo.DontLogBlock(controller))
            {
                //  The order of these operations is significant -- we first set up the object, then perform the membership operations

                _sourceState = source;
                _sourceState.PropertyChanged += EndpointPropertyChangedHandler;
                _destinationState = destination;
                _destinationState.PropertyChanged += EndpointPropertyChangedHandler;
                TriggerEvent = triggerEvent;
                TransitionType = TransitionTypes.Group;

                _sourceState.TransitionsFrom.Add(this);
                _destinationState.TransitionsTo.Add(this);
            }
        }

        //  Constructor for use by Redo

        internal Transition(ViewModelController controller, UndoRedo.AddTransitionRecord redoRecord) : base(controller, redoRecord)
        {
            using (new UndoRedo.DontLogBlock(controller))
            {
                //  The order of these operations is significant -- we first set up the object, then perform the membership operations

                _sourceState = redoRecord.SourceStateId == ObjectModel.TrackableObject.NullId ? null : Find(redoRecord.SourceStateId) as State;
                if (_sourceState != null)
                {
                    _sourceState.PropertyChanged += EndpointPropertyChangedHandler;
                }
                _destinationState = redoRecord.DestinationStateId == ObjectModel.TrackableObject.NullId ? null : Find(redoRecord.DestinationStateId) as State;
                if (_destinationState != null)
                {
                    _destinationState.PropertyChanged += EndpointPropertyChangedHandler;
                }
                TriggerEvent = redoRecord.TriggerEventId == ObjectModel.TrackableObject.NullId ? null : Find(redoRecord.TriggerEventId) as EventType;
                OldActions = new List<string>();
                ActionReferences = new ObservableCollection<ActionReference>();
                ActionReferences.CollectionChanged += Actions_CollectionChanged;
                for (int slot = redoRecord.ActionIds.GetLowerBound(0); slot <= redoRecord.ActionIds.GetUpperBound(0); slot++)
                {
                    ActionReference actionReference = new  ActionReference(Controller, this, Find(redoRecord.ActionIds[slot]) as Action);
                    ActionReferences.Add(actionReference);
                }
                ActionIds = null;
                TransitionType = redoRecord.TransitionType;

                if (_sourceState != null)
                {
                    _sourceState.TransitionsFrom.Add(this);
                }
                if (_destinationState != null)
                {
                    _destinationState.TransitionsTo.Add(this);
                }
            }
        }

        private void Actions_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    foreach (ActionReference actionReference in e.NewItems)
                    {
                        actionReference.Removing += ActionIsBeingRemovedHandler;
                    }
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    foreach (ActionReference actionReference in e.OldItems)
                    {
                        actionReference.Removing -= ActionIsBeingRemovedHandler;
                    }
                    break;
                default:
                    throw new NotImplementedException();
            }

            //  TODO TODO TODO
            //
            //  We should have synchronized with the underlying text buffer before we made the change.
#if false
            if ((Controller?.CanGuiChangeBegin() ?? true))
            {
                Controller?.LogUndoAction(new UndoRedo.ListValuedPropertyChangedRecord(Controller, this, "ActionReferences", OldActions));
                Controller?.NoteGuiChangeEnd();
            }

            OldActions = ActionReferences.Select(a => a.Id.ToString()).ToList<string>();
#endif
        }

        private void ActionIsBeingRemovedHandler(ObjectModel.IRemovableObject actionReference)
        {
            actionReference.Removing -= ActionIsBeingRemovedHandler;
            ActionReferences.Remove(actionReference as ActionReference);
        }

        internal override void DeserializeCleanup(DeserializeCleanupPhases phase, ViewModelController controller, StateMachine stateMachine)
        {
            base.DeserializeCleanup(phase, controller, stateMachine);

            if (phase == DeserializeCleanupPhases.ObjectResolution)
            {
                using (new UndoRedo.DontLogBlock(controller))
                {
                    _destinationState = Find(_destinationStateId) as ViewModel.State;
                    _destinationState.PropertyChanged += EndpointPropertyChangedHandler;
                    _destinationState.TransitionsTo.Add(this);

                    _sourceState = Find(_sourceStateId) as ViewModel.State;
                    _sourceState.PropertyChanged += EndpointPropertyChangedHandler;
                    _sourceState.TransitionsFrom.Add(this);

                    _triggerEvent = Find(_triggerEventId) as ViewModel.EventType;

                    ActionReferences = new ObservableCollection<ActionReference>();
                    ActionReferences.CollectionChanged += Actions_CollectionChanged;
                    for (int slot = 0; slot < _actionIds.Count; slot++)
                    {
                        ActionReference actionReference = new ActionReference(Controller, this, Find(_actionIds[slot]) as Action);
                        ActionReferences.Add(actionReference);
                    }

                    LoadDeprecatedActions(controller, stateMachine);
                }
            }
        }

        private void EndpointPropertyChangedHandler(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "LeftTopPosition":
                case "Transitions":
                    EndpointPositionChanged?.Invoke(this, new EventArgs());
                    break;

                default:
                    break;
            }
        }

        internal override void GetProperty(string propertyName, out IEnumerable<string> value)
        {
            switch (propertyName)
            {
                case "ActionReferencess":
                    value = ActionReferences.Select(a => a.Action.Id.ToString()).ToArray<string>();
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
                    value = (DestinationState?.Id ?? (int)ObjectModel.TrackableObject.NullId).ToString();
                    break;
                case "SourceState":
                    value = (SourceState?.Id ?? ObjectModel.TrackableObject.NullId).ToString();
                    break;
                case "TriggerEvent":
                    value = (TriggerEvent?.Id ?? ObjectModel.TrackableObject.NullId).ToString();
                    break;
                default:
                    base.GetProperty(propertyName, out value);
                    break;
            }
        }

        /// <summary>
        /// Previously, actions were saved in the XML serialization as an array of strings, and no semantics were
        /// associated with them, except they were the name of abstract methods.
        /// This has been replaced by the ViewModel.Action class.  Here, we create Action objects for each old-style
        /// string action name. Then we remove the old-style names so they won't be saved by the serializer.
        /// </summary>
        /// <param name="controller"></param>
        /// <param name="stateMachine"></param>
        private void LoadDeprecatedActions(ViewModelController controller, StateMachine stateMachine)
        {
            for (int slot = 0; slot < DeprecatedActions.Count; slot++)
            {
                string actionName = DeprecatedActions[slot];
                Action action = stateMachine.Actions.Where(a => a.Name == actionName).FirstOrDefault();
                if (action == null)
                {
                    action = new Action(controller, actionName);
                    stateMachine.Actions.Add(action);
                }
                ActionReferences.Add(new ActionReference(Controller, this, action));
            }
            DeprecatedActions.Clear();
        }

        private void OnEndpointChanged()
        {
            EndpointChanged?.Invoke(this, new EventArgs());
        }

        protected override void OnRemoving()
        {
            using (new UndoRedo.DontLogBlock(Controller))
            {
                if (_destinationState != null)
                {
                    _destinationState.PropertyChanged -= EndpointPropertyChangedHandler;
                    _destinationState.TransitionsTo.Remove(this);
                }
                if (_sourceState != null)
                {
                    _sourceState.PropertyChanged -= EndpointPropertyChangedHandler;
                    _sourceState.TransitionsFrom.Remove(this);
                }
                if (ActionReferences != null)
                {
                    foreach (ActionReference actionReference in ActionReferences)
                    {
                        actionReference.Removing -= ActionIsBeingRemovedHandler;
                        actionReference.Remove();
                    }
                }
            }
            base.OnRemoving();
        }

        internal override void ResetSearch()
        {
            WasActionFound = false;
            WasTriggerFound = false;
            base.ResetSearch();
        }

        internal override uint Search(string searchString)
        {
            uint count =  base.Search(searchString);

            WasActionFound = ActionReferences.Where(a => a.Action.Name.Contains(searchString)).Any();
            WasTriggerFound = TriggerEvent != null && !string.IsNullOrWhiteSpace(TriggerEvent.Name) && TriggerEvent.Name.Contains(searchString);

            return count + (uint)(WasActionFound ? 1 : 0) + (uint)(WasTriggerFound ? 1 : 0);
        }

        internal override void SetProperty(string propertyName, string newValue)
        {
            switch (propertyName)
            {
                case "DestinationState":
                    DestinationState = newValue == ObjectModel.TrackableObject.NullId.ToString() ? null : Find(int.Parse(newValue)) as ViewModel.State;
                    break;
                case "SourceState":
                    SourceState = newValue == ObjectModel.TrackableObject.NullId.ToString() ? null : Find(int.Parse(newValue)) as ViewModel.State;
                    break;
                case "TriggerEvent":
                    TriggerEvent = newValue == ObjectModel.TrackableObject.NullId.ToString() ? null : Find(int.Parse(newValue)) as ViewModel.EventType;
                    break;
                default:
                    base.SetProperty(propertyName, newValue);
                    break;
            }
        }

        public override string ToString()
        {
            return $@"{(SourceState?.Name ?? "<null>")}<({(TriggerEvent?.Name ?? "<null>")})>{(DestinationState?.Name ?? "<null>")}";
        }
    }
}
