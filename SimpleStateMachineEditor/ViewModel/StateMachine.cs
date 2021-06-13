using Microsoft.VisualStudio.OLE.Interop;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SimpleStateMachineEditor.ViewModel
{
    //++
    //  The StateMachine class represents the entire finite-state-automaton. 
    //--
    public class StateMachine : ObjectModel.NamedObject
    {
        [Browsable(false)]
        public ObservableCollection<Action> Actions { get; private set; }
        [Browsable(false)]
        public ObservableCollection<EventType> EventTypes { get; private set; }
        [Browsable(false)]
        public ObservableCollection<Group> Groups { get; private set; }
        [Browsable(false)]
        public ObservableCollection<Layer> Layers { get; private set; }
        [Browsable(false)]
        public ObservableCollection<State> States { get; private set; }
        [Browsable(false)]
        public ObservableCollection<Transition> Transitions { get; private set; }

        [DisplayName("ClassName")]
        [Description("The name of the class created to implement the state machine")]
        public string GeneratedClassName
        {
            get => _generatedClassName;
            set
            {
                if (_generatedClassName != value)
                {
                    using (new ViewModelController.GuiChangeBlock(Controller))
                    {
                        Controller?.LogUndoAction(new UndoRedo.PropertyChangedRecord(Controller, this, "GeneratedClassName", _generatedClassName?.ToString() ?? ""));
                        _generatedClassName = value;
                        OnPropertyChanged("GeneratedClassName");
                    }
                }
            }
        }
        string _generatedClassName;

        [Description("Specifies how to handle events that do not match the trigger event for any of the current state's transitions. False raises UnexpectedEventException, True ignores the event and continues execution.")]
        public bool IgnoreUnmatchedEvents
        {
            get => _ignoreUnmatchedEvents;
            set
            {
                if (_ignoreUnmatchedEvents != value)
                {
                    using (new ViewModelController.GuiChangeBlock(Controller))
                    {
                        Controller?.LogUndoAction(new UndoRedo.PropertyChangedRecord(Controller, this, "IgnoreUnmatchedEvents", _ignoreUnmatchedEvents.ToString()));
                        _ignoreUnmatchedEvents = value;
                        OnPropertyChanged("IgnoreUnmatchedEvents");
                    }
                }
            }
        }
        bool _ignoreUnmatchedEvents;

        [Description("The type of the value returned by execution of the state machine")]
        public string ReturnValue
        {
            get => _returnValue;
            set
            {
                if (_returnValue != value)
                {
                    using (new ViewModelController.GuiChangeBlock(Controller))
                    {
                        Controller?.LogUndoAction(new UndoRedo.PropertyChangedRecord(Controller, this, "ReturnValue", _returnValue?.ToString() ?? ""));
                        _returnValue = value;
                        OnPropertyChanged("ReturnValue");
                    }
                }
            }
        }
        string _returnValue;

        [XmlIgnore]
        [Description("The first state at which execution will begin")]
        public State StartState
        {
            get => _startState;
            set
            {
                if (_startState != value)
                {
                    using (new ViewModelController.GuiChangeBlock(Controller))
                    {
                        if (_startState != null)
                        {
                            _startState.IsStartState = false;
                        }
                        Controller?.LogUndoAction(new UndoRedo.PropertyChangedRecord(Controller, this, "StartState", (_startState?.Id ?? ObjectModel.TrackableObject.NullId).ToString()));
                        _startState = value;
                        if (_startState == null)
                        {
                            _startStateId = ObjectModel.TrackableObject.NullId;
                        }
                        else
                        {
                            _startState.IsStartState = true;
                        }
                        OnPropertyChanged("StartState");
                    }
                }
            }
        }
        State _startState;
        [Browsable(false)]
        [XmlAttribute]
        public int StartStateId
        {
            get => StartState?.Id ?? _startStateId;
            set => _startStateId = value;
        }
        int _startStateId = ObjectModel.TrackableObject.NullId;

        [Description("Determines if states must have transitions for all defined event types")]
        public bool RequireCompleteEventCoverage
        {
            get => _requireCompleteEventCoverage;
            set
            {
                if (_requireCompleteEventCoverage != value)
                {
                    using (new ViewModelController.GuiChangeBlock(Controller))
                    {
                        Controller?.LogUndoAction(new UndoRedo.PropertyChangedRecord(Controller, this, "RequireCompleteEventCoverage", _requireCompleteEventCoverage.ToString()));
                        _requireCompleteEventCoverage = value;
                        OnPropertyChanged("RequireCompleteEventCoverage");
                    }
                }
            }
        }
        bool _requireCompleteEventCoverage;



        //  Constructor for use by serialization ONLY

        public StateMachine()
        {
            Actions = new ObservableCollection<Action>();
            Actions.CollectionChanged += ObservableCollectionOfRemovableObjectsChangedHandler;
            EventTypes = new ObservableCollection<EventType>();
            EventTypes.CollectionChanged += ObservableCollectionOfRemovableObjectsChangedHandler;
            Groups = new ObservableCollection<Group>();
            Groups.CollectionChanged += ObservableCollectionOfRemovableObjectsChangedHandler;
            Layers = new ObservableCollection<Layer>();
            Layers.CollectionChanged += ObservableCollectionOfRemovableObjectsChangedHandler;
            States = new ObservableCollection<State>();
            States.CollectionChanged += ObservableCollectionOfRemovableObjectsChangedHandler;
            Transitions = new ObservableCollection<Transition>();
            Transitions.CollectionChanged += ObservableCollectionOfRemovableObjectsChangedHandler;
        }

        //  General-use internal constructor

        internal StateMachine(ViewModelController controller) : base(controller)
        {
            Actions = new ObservableCollection<Action>();
            Actions.CollectionChanged += ObservableCollectionOfRemovableObjectsChangedHandler;
            EventTypes = new ObservableCollection<EventType>();
            EventTypes.CollectionChanged += ObservableCollectionOfRemovableObjectsChangedHandler;
            Groups = new ObservableCollection<Group>();
            Groups.CollectionChanged += ObservableCollectionOfRemovableObjectsChangedHandler;
            Layers = new ObservableCollection<Layer>();
            Layers.CollectionChanged += ObservableCollectionOfRemovableObjectsChangedHandler;
            States = new ObservableCollection<State>();
            States.CollectionChanged += ObservableCollectionOfRemovableObjectsChangedHandler;
            Transitions = new ObservableCollection<Transition>();
            Transitions.CollectionChanged += ObservableCollectionOfRemovableObjectsChangedHandler;
        }

        public void ApplyDefaults(string filePath)
        {
            if (string.IsNullOrWhiteSpace(GeneratedClassName) && !string.IsNullOrWhiteSpace(Path.GetFileNameWithoutExtension(filePath)))
            {
                _generatedClassName = Path.GetFileNameWithoutExtension(filePath);
            }
            if (string.IsNullOrWhiteSpace(ReturnValue))
            {
                _returnValue = "void";
            }
        }

        public static void Deserialize(ViewModelController controller, TextReader xmlStream)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(StateMachine));
            using (new UndoRedo.DontLogBlock(controller))
            {
                controller.StateMachine = null;
                controller.AllFindableObjects = new Dictionary<int, ObjectModel.TrackableObject>();
                controller.AllFindableObjects.Add(ObjectModel.TrackableObject.NullId, null);

                StateMachine stateMachine = serializer.Deserialize(xmlStream) as StateMachine;

                for (ObjectModel.TrackableObject.DeserializeCleanupPhases phase = DeserializeCleanupPhases.First; phase <= DeserializeCleanupPhases.Last; phase++)
                {
                    stateMachine.DeserializeCleanup(phase, controller, stateMachine);
                }

                controller.StateMachine = stateMachine;
            }
        }

        //  The object graph is serialized as a tree. This method completes the graph.

        internal override void DeserializeCleanup(DeserializeCleanupPhases phase, ViewModelController controller, ViewModel.StateMachine stateMachine)
        {

            base.DeserializeCleanup(phase, controller, this);

            foreach (Action a in Actions)
            {
                a.DeserializeCleanup(phase, Controller, this);
            }
            foreach (EventType e in EventTypes)
            {
                e.DeserializeCleanup(phase, Controller, this);
            }
            foreach (Group g in Groups)
            {
                g.DeserializeCleanup(phase, Controller, this);
            }
            foreach (Layer l in Layers)
            {
                l.DeserializeCleanup(phase, controller, this);
            }
            foreach (State s in States)
            {
                s.DeserializeCleanup(phase, controller, this);
            }
            foreach (Transition t in Transitions)
            {
                t.DeserializeCleanup(phase, controller, this);
            }

            if (phase == DeserializeCleanupPhases.ObjectResolution)
            {
                _startState = Find(_startStateId) as State;
                if (_startState != null)
                {
                    _startState.IsStartState = true;
                }
            }
        }

        internal override void GetProperty(string propertyName, out string value)
        {
            switch (propertyName)
            {
                case "GeneratedClassName":
                    value = GeneratedClassName;
                    break;
                case "IgnoreUnmatchedEvents":
                    value = IgnoreUnmatchedEvents.ToString();
                    break;
                case "ReturnValue":
                    value = ReturnValue;
                    break;
                case "StartState":
                    value = (StartState?.Id ?? ObjectModel.TrackableObject.NullId).ToString();
                    break;
                case "RequireCompleteEventCoverage":
                    value = RequireCompleteEventCoverage.ToString();
                    break;
                default:
                    base.GetProperty(propertyName, out value);
                    break;
            }
        }

        public void Serialize(TextWriter xmlStream)
        {
            XmlAttributes attributes = new XmlAttributes();
            attributes.XmlIgnore = true;

            XmlElementAttribute legacyLeftTopPosition = new XmlElementAttribute("LegacyLeftTopPosition", typeof(System.Windows.Point));
            attributes.XmlElements.Add(legacyLeftTopPosition);
            XmlElementAttribute legacyPosition = new XmlElementAttribute("LegacyPosition", typeof(System.Windows.Point));
            attributes.XmlElements.Add(legacyPosition);

            XmlAttributeOverrides overrides = new XmlAttributeOverrides();
            overrides.Add(typeof(ObjectModel.LayeredPositionableObject), "LegacyLeftTopPosition", attributes);
            overrides.Add(typeof(ObjectModel.LayerPosition), "LegacyPosition", attributes);

            XmlSerializer serializer = new XmlSerializer(GetType(), overrides);
            serializer.Serialize(xmlStream, this);
        }

        internal override void SetProperty(string propertyName, string newValue)
        {
            switch (propertyName)
            {
                case "GeneratedClassName":
                    GeneratedClassName = newValue;
                    break;
                case "IgnoreUnmatchedEvents":
                    IgnoreUnmatchedEvents = bool.Parse(newValue);
                    break;
                case "ReturnValue":
                    ReturnValue = newValue;
                    break;
                case "StartState":
                    StartState = (newValue == ObjectModel.TrackableObject.NullId.ToString() ? null : Find(int.Parse(newValue)) as ViewModel.State);
                    break;
                case "RequireCompleteEventCoverage":
                    RequireCompleteEventCoverage = bool.Parse(newValue);
                    break;
                default:
                    base.SetProperty(propertyName, newValue);
                    break;
            }
        }
    }
}
