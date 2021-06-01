﻿using System;
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
    public class Transition : ObjectModel.DocumentedObject
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
                    Controller?.LogUndoAction(new UndoRedo.PropertyChangedRecord(Controller, this, "SourceState", _sourceState?.Id.ToString() ?? ((int)-1).ToString()));
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
                    Controller?.LogUndoAction(new UndoRedo.PropertyChangedRecord(Controller, this, "DestinationState", _destinationState?.Id.ToString() ?? ((int)-1).ToString()));
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
                    Controller?.LogUndoAction(new UndoRedo.PropertyChangedRecord(Controller, this, "TriggerEvent", _triggerEvent?.Id.ToString() ?? ((int)-1).ToString()));
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
            get
            {
                if (_actionIds == null)
                {
                    _actionIds = ActionReferences.Select(m => m.Action.Id).ToList();
                }
                return _actionIds;
            }
            private set
            {
                _actionIds = value;
            }
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
        internal event EventHandler EndpointPositionChanged;



        //  Constructor for use by serialization ONLY

        public Transition()
        {
            DeprecatedActions = new ObservableCollection<string>();
            OldActions = new List<string>();
            ActionReferences = new ObservableCollection<ActionReference>();
            ActionReferences.CollectionChanged += Actions_CollectionChanged;
            ActionIds = new List<int>();
        }

        //  Internal constructor for general use

        internal Transition(ViewModelController controller, ViewModel.State sourceState) : base (controller)
        {
            using (new UndoRedo.DontLogBlock(controller))
            {
                DeprecatedActions = new ObservableCollection<string>();
                OldActions = new List<string>();
                _sourceState = sourceState;
                _sourceState.TransitionsFrom.Add(this);
                _sourceState.PropertyChanged += EndpointPropertyChangedHandler;
                ActionReferences = new ObservableCollection<ActionReference>();
                ActionReferences.CollectionChanged += Actions_CollectionChanged;
                ActionIds = null;
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
                OldActions = new List<string>();
                ActionReferences = new ObservableCollection<ActionReference>();
                ActionReferences.CollectionChanged += Actions_CollectionChanged;
                for (int slot = redoRecord.ActionIds.GetLowerBound(0); slot <= redoRecord.ActionIds.GetUpperBound(0); slot++)
                {
                    ActionReference actionReference = new  ActionReference(Controller, this, Controller.StateMachine.Find(redoRecord.ActionIds[slot]) as Action);
                    ActionReferences.Add(actionReference);
                }
                ActionIds = null;
            }
        }

        private void Actions_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            ActionIds = null;

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
            if (IsChangeAllowed)
            {
                Controller?.LogUndoAction(new UndoRedo.ListValuedPropertyChangedRecord(Controller, this, "ActionReferences", OldActions));
                EndChange();
            }

            OldActions = ActionReferences.Select(a => a.Id.ToString()).ToList<string>();
#endif
        }

        private void ActionIsBeingRemovedHandler(ObjectModel.IRemovableObject actionReference)
        {
            actionReference.Removing -= ActionIsBeingRemovedHandler;
            ActionReferences.Remove(actionReference as ActionReference);
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

                for (int slot = 0; slot < ActionIds.Count; slot++)
                {
                    ActionReference actionReference = new ActionReference(Controller, this, stateMachine.Find(ActionIds[slot]) as Action);
                    ActionReferences.Add(actionReference);
                }

                LoadDeprecatedActions(controller, stateMachine);
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

        protected override void OnRemoving()
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
            foreach (ActionReference actionReference in ActionReferences)
            {
                actionReference.Removing -= ActionIsBeingRemovedHandler;
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

#if false
        internal override void SetProperty(string propertyName, IList<string> newValue)
        {
            switch (propertyName)
            {
                case "ActionReferences":
                    using (new UndoRedo.AtomicBlock(Controller, "Set property ActionReferences"))
                    {
                        while (ActionReferences.Count > 0)
                        {
                            //  We don't use the Clear method because it results in a Reset notification, which doesn't provide the OldItems list on the CollectionChanged event

                            ActionReferences.RemoveAt(0);
                        }
                        for (int slot = 0; slot < newValue.Count(); slot++)
                        {
                            Action action = Controller.StateMachine.Find(int.Parse(newValue[slot])) as Action;
                            ActionReferences.Add(new ActionReference(Controller, this, action));
                        }
                    }
                    break;
                default:
                    base.SetProperty(propertyName, newValue);
                    break;
            }
        }
#endif
        public override string ToString()
        {
            return $@"{(SourceState?.Name ?? "<null>")}<({(TriggerEvent?.Name ?? "<null>")})>{(DestinationState?.Name ?? "<null>")}";
        }
    }
}
