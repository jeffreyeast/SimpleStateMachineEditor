using Microsoft.VisualStudio.OLE.Interop;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
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
        public ObservableCollection<Region> Regions { get; private set; }
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
                if (_generatedClassName != value && IsChangeAllowed)
                {
                    if (Controller?.LoggingIsEnabled ?? false)
                    {
                        Controller?.UndoManager.Add(new UndoRedo.PropertyChangedRecord(Controller, this, "GeneratedClassName", _generatedClassName?.ToString() ?? ""));
                    }
                    _generatedClassName = value;
                    OnPropertyChanged("GeneratedClassName");
                    EndChange();
                }
            }
        }
        string _generatedClassName;

        [Description("Specifies how to handle events that do not match the trigger event for any of the current state's transitions. False raises UnexpectgedEventException, True ignores the event and continues execution.")]
        public bool IgnoreUnmatchedEvents
        {
            get => _ignoreUnmatchedEvents;
            set
            {
                if (_ignoreUnmatchedEvents != value && IsChangeAllowed)
                {
                    if (Controller?.LoggingIsEnabled ?? false)
                    {
                        Controller?.UndoManager.Add(new UndoRedo.PropertyChangedRecord(Controller, this, "IgnoreUnmatchedEvents", _ignoreUnmatchedEvents.ToString()));
                    }
                    _ignoreUnmatchedEvents = value;
                    OnPropertyChanged("IgnoreUnmatchedEvents");
                    EndChange();
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
                if (_returnValue != value && IsChangeAllowed)
                {
                    if (Controller?.LoggingIsEnabled ?? false)
                    {
                        Controller?.UndoManager.Add(new UndoRedo.PropertyChangedRecord(Controller, this, "ReturnValue", _returnValue?.ToString() ?? ""));
                    }
                    _returnValue = value;
                    OnPropertyChanged("ReturnValue");
                    EndChange();
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
                if (_startState != value && IsChangeAllowed)
                {
                    if (Controller?.LoggingIsEnabled ?? false)
                    {
                        Controller?.UndoManager.Add(new UndoRedo.PropertyChangedRecord(Controller, this, "StartState", _startState?.Id.ToString() ?? ((int)-1).ToString()));
                    }
                    _startState = value;
                    if (_startState == null)
                    {
                        _startStateId = -1;
                    }
                    OnPropertyChanged("StartState");
                    EndChange();
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
        int _startStateId = -1;

        [Description("Determines if states must have transitions for all defined event types")]
        public bool RequireCompleteEventCoverage
        {
            get => _requireCompleteEventCoverage;
            set
            {
                if (_requireCompleteEventCoverage != value && IsChangeAllowed)
                {
                    if (Controller?.LoggingIsEnabled ?? false)
                    {
                        Controller?.UndoManager.Add(new UndoRedo.PropertyChangedRecord(Controller, this, "RequireCompleteEventCoverage", _requireCompleteEventCoverage.ToString()));
                    }
                    _requireCompleteEventCoverage = value;
                    OnPropertyChanged("RequireCompleteEventCoverage");
                    EndChange();
                }
            }
        }
        bool _requireCompleteEventCoverage;



        //  Constructor for use by serialization ONLY

        public StateMachine()
        {
            Actions = new ObservableCollection<Action>();
            Actions.CollectionChanged += CollectionChangedHandler;
            EventTypes = new ObservableCollection<EventType>();
            EventTypes.CollectionChanged += CollectionChangedHandler;
            Regions = new ObservableCollection<Region>();
            Regions.CollectionChanged += CollectionChangedHandler;
            States = new ObservableCollection<State>();
            States.CollectionChanged += CollectionChangedHandler;
            Transitions = new ObservableCollection<Transition>();
            Transitions.CollectionChanged += CollectionChangedHandler;
        }

        //  General-use internal constructor

        internal StateMachine(ViewModelController controller) : base(controller)
        {
            Actions = new ObservableCollection<Action>();
            Actions.CollectionChanged += CollectionChangedHandler;
            EventTypes = new ObservableCollection<EventType>();
            EventTypes.CollectionChanged += CollectionChangedHandler;
            Regions = new ObservableCollection<Region>();
            Regions.CollectionChanged += CollectionChangedHandler;
            States = new ObservableCollection<State>();
            States.CollectionChanged += CollectionChangedHandler;
            Transitions = new ObservableCollection<Transition>();
            Transitions.CollectionChanged += CollectionChangedHandler;
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

        private void CollectionChangedHandler(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (ObjectModel.TrackableObject o in e.OldItems)
                    {
                        o.OnRemoving();
                    }
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        public static void Deserialize(ViewModelController controller, TextReader xmlStream)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(StateMachine));
            using (new UndoRedo.DontLogBlock(controller))
            {
                StateMachine stateMachine = serializer.Deserialize(xmlStream) as StateMachine;
                stateMachine.DeserializeCleanup(controller, stateMachine);
                controller.StateMachine = stateMachine;
            }
        }

        //  The object graph is serialized as a tree. This method completes the graph.

        internal override void DeserializeCleanup(ViewModelController controller, ViewModel.StateMachine stateMachine)
        {
            base.DeserializeCleanup(controller, this);

            foreach (Action a in Actions)
            {
                a.DeserializeCleanup(Controller, this);
            }
            foreach (EventType e in EventTypes)
            {
                e.DeserializeCleanup(Controller, this);
            }
            foreach (Region r in Regions)
            {
                r.DeserializeCleanup(controller, this);
            }
            foreach (State s in States)
            {
                s.DeserializeCleanup(controller, this);
            }
            foreach (Transition t in Transitions)
            {
                t.DeserializeCleanup(controller, this);
            }

            _startState = Find(_startStateId) as State;
        }

        internal ObjectModel.TrackableObject Find(int id)
        {
            ObjectModel.TrackableObject trackableObject = null;

            trackableObject = Actions.Where(a => a.Id == id).FirstOrDefault();
            if (trackableObject == null)
            {
                trackableObject = EventTypes.Where(e => e.Id == id).FirstOrDefault();
            }
            if (trackableObject == null)
            {
                trackableObject = Regions.Where(r => r.Id == id).FirstOrDefault();
            }
            if (trackableObject == null)
            {
                trackableObject = States.Where(s => s.Id == id).FirstOrDefault();
            }
            if (trackableObject == null)
            {
                trackableObject = Transitions.Where(t => t.Id == id).FirstOrDefault();
            }
            if (trackableObject == null)
            {
                if (id == Id)
                {
                    trackableObject = this;
                }
            }

            return trackableObject;
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
                    value = StartState?.Id.ToString() ?? ((int)-1).ToString();
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
            XmlSerializer serializer = new XmlSerializer(GetType());
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
                    StartState = (newValue == "-1" ? null : Controller.StateMachine.Find(int.Parse(newValue)) as ViewModel.State);
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
