using Microsoft.VisualStudio.TextTemplating.VSHost;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleStateMachineEditor.Generators
{
    internal class SfsaGenerator : BaseCodeGeneratorWithSite
    {
        public const string Name = nameof(SfsaGenerator);
        public const string Description = "Generates CSharp source code for given state machine definitions.";


        // For reasons known only to Visual Studio, it won't use this generator if it has an explicit constructor.

#if false
        internal SfsaGenerator()
        {

        }
#endif

        private List<ObjectModel.INamedObject> BuildSortedNamedObjectList(IEnumerable<ObjectModel.INamedObject> namedObjects)
        {
            List<ObjectModel.INamedObject> namedObjectList = namedObjects.ToList<ObjectModel.INamedObject>();
            namedObjectList.Sort(new Comparison<ObjectModel.INamedObject>((ObjectModel.INamedObject x, ObjectModel.INamedObject y) =>
            {
                if (x.Name == null || y.Name == null)
                {
                    return x.Id.CompareTo(y.Id);
                }
                else
                {
                    return x.Name.CompareTo(y.Name);
                }
            }));
            return namedObjectList;
        }

        public override string GetDefaultExtension()
        {
            return ".cs";
        }

        private string GenerateActions(ViewModel.StateMachine stateMachine, IEnumerable<ViewModel.Transition> transitions)
        {
            string result = "";
            foreach (var action in stateMachine.Actions.OrderBy(a => a.Name))
            {
                if (!string.IsNullOrWhiteSpace(action.Description))
                {
                    result += $@"        ///<summary>{action.Description}</summary>
";
                }

                result += $@"        protected abstract {(stateMachine.ReturnValue == "void" ? "void" : $@"{stateMachine.ReturnValue}")} {action.Name}();
";
            }

            return result;
        }

        protected override byte[] GenerateCode(string inputFileName, string inputFileContent)
        {
            string modulePreamble = $@"// ------------------------------------------------------------------------------
// <auto-generated>
//     This file was generated by {Name}
// </auto-generated>
// ------------------------------------------------------------------------------
";
            string generatedCodeString;

            //  Parse the state machine definition

            try
            {
                ViewModel.ViewModelController model = new ViewModel.ViewModelController(inputFileContent);
                ValidateStateMachine(model);
                generatedCodeString = GenerateStateMachineClass(model.StateMachine);
            }
            catch
            {
                string errorString = $@"State machine definition ({inputFileName}) is not properly formed";
                generatedCodeString = $@"#error {errorString}";
                GeneratorErrorCallback(false, 1, errorString, 0, 0);
            }

            return Encoding.UTF8.GetBytes(modulePreamble + generatedCodeString);
        }

        private string GenerateEnumDeclaration(IEnumerable<ObjectModel.INamedObject> objects)
        {
            string result = "";

            foreach (var o in objects)
            {
                if (!string.IsNullOrWhiteSpace(o.Description))
                {
                    result += $@"            ///<summary>{o.Description}</summary>
";
                }
                result += $@"            {(o.Name == ViewModel.EventType.WildcardEventTypeName ? "Wildcard" : o.Name)},
";
            }

            return result;
        }

        private string GenerateNameStrings(IEnumerable<ObjectModel.INamedObject> objects)
        {
            string result = "";
            foreach (var o in objects.OrderBy(o => o.Name))
            {
                 result += $@"            ""{o.Name}"",
";
            }
            return result;
        }

        protected string GenerateStateMachineClass(ViewModel.StateMachine stateMachine)
        {
            string returnValueDescription = stateMachine.ReturnValue == "void" ? "" : $@"        /// <returns>A value of type {stateMachine.ReturnValue}</returns>
";
            List<ObjectModel.INamedObject> sortedEventTypes = BuildSortedNamedObjectList(stateMachine.EventTypes);
            List<ObjectModel.INamedObject> sortedStates = BuildSortedNamedObjectList(stateMachine.States);
            string eventTypes = GenerateEnumDeclaration(sortedEventTypes);
            string eventTypeNames = GenerateNameStrings(sortedEventTypes);
            string stateNames = GenerateNameStrings(sortedStates);
            string stateClassifications = GenerateStateTypes(sortedStates);
            string actions = GenerateActions(stateMachine, stateMachine.Transitions.Where(t => t.TransitionType == ViewModel.Transition.TransitionTypes.Normal));
            string transitions = GenerateTransitions(stateMachine, sortedEventTypes, sortedStates);



            string template = $@"
namespace {FileNamespace}
{{
    using System;
    using System.Collections.Generic;
    using SimpleStateMachine;

    {(!string.IsNullOrWhiteSpace(stateMachine.Description) ? $@"///<summary>{stateMachine.Description}</summary>" : "")}
    public abstract class {stateMachine.GeneratedClassName} : {(stateMachine.ReturnValue == "void" ? "StateMachineWithoutReturnValueBase" : ($@"StateMachineWithReturnValueBase<{stateMachine.ReturnValue}>"))}
    {{
        public enum EventTypes
        {{
{eventTypes}        }};

        static readonly string[] EventTypeNames = new string[]
        {{
{eventTypeNames}        }};

        static readonly string[] StateNames = new string[]
        {{
{stateNames}        }};

        protected override int StartState => Array.IndexOf(StateNames, ""{stateMachine.StartState?.Name}"");

        static readonly StateTypes[] StateClassifications = new StateTypes[]
        {{
{stateClassifications}        }};

        /// <summary>
        /// Action Routines
        /// 
        /// You must override each of these action routines in your implementation.
        /// </summary>

{actions}

        protected override Transition<Action>[,] Transitions => _transitions;
        Transition<Action>[,] _transitions;

        public {stateMachine.GeneratedClassName}() : base (StateClassifications, EventTypeNames, StateNames)
        {{
            _transitions = new Transition<Action>[,]
            {{
{transitions}            }};
        }}

        /// <summary>
        /// Invoked to execute the state machine.
        /// </summary>
        /// <param name=""e"">Provides an optional event type to post at normal priority before starting execution</param>
{(stateMachine.ReturnValue == "void" ? "" : "        /// <returns>A value of type R</returns>")}
        /// <exception cref=""System.InvalidOperationException"">Thrown if an event is chosen for 
        /// execution and no transition from the current state maches the event.
        /// </exception>
        /// <remarks>
        /// The state machine runs until one of the following conditions is met:
        /// - There are no remaining events to process
        /// - A stop or error state is entered
        /// - An event is encountered and no transition matches the event type
        /// - An action raises an exception
        /// For each state, the next event to be processed is chosen from the head of the
        /// internal event queue, and if no event is found, then the external event queue.
        /// </remarks>
        public {stateMachine.ReturnValue} Execute(EventTypes? e = null)
        {{
            {(stateMachine.ReturnValue == "void" ? "" : "return ")} base.Execute(e.HasValue ? (int)e.Value : default(int?));
        }}

        /// <summary>
        /// Invoked by an action routine to post an internal (high-priority) event.
        /// <param name=eventType>Identifies the event to be posted</param>
        /// <exception cref=""ArgumentOutOfRangeException"">If the eventType is not valid</exception>
        /// </summary>
        protected void PostHighPriorityEvent(EventTypes eventType)
        {{
            PostHighPriorityEvent((int)eventType);
        }}

        /// <summary>
        /// Invoked by any code to post an external (lower-priority) event.
        /// <param name=eventType>Identifies the event to be posted</param>
        /// <exception cref=""ArgumentOutOfRangeException"">If the eventType is not valid</exception>
        /// </summary>
        public void PostNormalPriorityEvent(EventTypes eventType)
        {{
            PostNormalPriorityEvent((int)eventType);
        }}
    }}
}}";

            return template;
        }

        private string GenerateStateTypes(List<ObjectModel.INamedObject> sortedStates)
        {
            string classifications = "";

            foreach (ViewModel.State state in sortedStates)
            {
                classifications += $@"            StateTypes.{state.StateType},
";
            }

            return classifications;
        }

        private string GenerateTransition(ViewModel.Transition transition, List<ObjectModel.INamedObject> sortedStates)
        {
            string transitionString = $@"
                    new Transition<Action>({sortedStates.IndexOf(transition.DestinationState)}, new Action[] {{ ";
            foreach (ViewModel.ActionReference actionReference in transition.ActionReferences)
            {
                transitionString += actionReference.Action.Name + ", ";
            }
            transitionString += $@"}}),";
            return transitionString;
        }

        private string GenerateTransitions(ViewModel.StateMachine stateMachine, List<ObjectModel.INamedObject> sortedEventTypes, List<ObjectModel.INamedObject> sortedStates)
        {
            string transitionList = "";
            string invalidTransitionString = $@"
                    new Transition<Action>(0, new Action[] {{ {(stateMachine.IgnoreUnmatchedEvents ? "" : "base.InvalidTransition, ")}}}),";

            foreach (ViewModel.State state in sortedStates)
            {
                //  Check if this state has a transition with a wild-card trigger. If so, use that for all events that don't have explicit transitions.

                ViewModel.Transition defaultTransition = state.TransitionsFrom.Where(t => t.TriggerEvent?.Name == ViewModel.EventType.WildcardEventTypeName && t.TransitionType == ViewModel.Transition.TransitionTypes.Normal).OfType<ViewModel.Transition>().FirstOrDefault();
                string defaultTransitionString = defaultTransition == null ? invalidTransitionString : GenerateTransition(defaultTransition, sortedStates);

                //  Capture the transitions, order them by trigger event. Exclude any wildcard and transtions without a trigger.

                ViewModel.Transition[] transitions = state.TransitionsFrom.Where(t => t.TriggerEvent != null && t.TriggerEvent.Name != ViewModel.EventType.WildcardEventTypeName && t.TransitionType == ViewModel.Transition.TransitionTypes.Normal).OrderBy(t => t.TriggerEvent.Name).OfType<ViewModel.Transition>().ToArray<ViewModel.Transition>();

                //  Now we can generate the transitions for this state

                transitionList += $@"                {{ // {state.Name}({sortedStates.IndexOf(state)})";

                int currentEventSlot = 0;
                for (int transitionSlot = 0; transitionSlot <= transitions.GetUpperBound(0); transitionSlot++)
                {
                    ViewModel.Transition transition = transitions[transitionSlot];

                    while (sortedEventTypes[currentEventSlot] != transition.TriggerEvent)
                    {
                        //  Generate the default transition for this event

                        transitionList += $@"{defaultTransitionString}  // {sortedEventTypes[currentEventSlot]}({currentEventSlot})";
                        currentEventSlot++;
                    }

                    //  Generate the transition

                    transitionList += GenerateTransition(transition, sortedStates) + $@"  // {sortedEventTypes[currentEventSlot]}({currentEventSlot})";
                    currentEventSlot++;
                }

                //  Pad any remaining events with invalid transitions

                while (currentEventSlot < sortedEventTypes.Count)
                {
                    //  Generate an invalid transition for this event

                    transitionList += $@"{defaultTransitionString}  // {sortedEventTypes[currentEventSlot]}({currentEventSlot})";
                    currentEventSlot++;
                }
                transitionList += $@"
                }},
";
            }

            return transitionList;
        }

        private void ValidateStateMachine(ViewModel.ViewModelController model)
        {
            model.StateMachine.ApplyDefaults(InputFilePath);

            if (model.StateMachine.EventTypes.Count == 0)
            {
                GeneratorErrorCallback(false, 1, "No event types are defined", 0, 0);
            }
            if (model.StateMachine.States.Count == 0)
            {
                GeneratorErrorCallback(false, 1, "No states are defined", 0, 0);
            }
            if (model.StateMachine.StartState == null)
            {
                GeneratorErrorCallback(false, 1, "No starting state is specified", 0, 0);
            }
            foreach (var eventType in model.StateMachine.EventTypes)
            {
                if (string.IsNullOrWhiteSpace(eventType.Name))
                {
                    GeneratorErrorCallback(false, 1, "At least one event type is unnamed", 0, 0);
                    break;
                }
            }

            Hashtable states = new Hashtable();
            foreach (var state in model.StateMachine.States)
            {
                if (string.IsNullOrWhiteSpace(state.Name))
                {
                    GeneratorErrorCallback(false, 1, "At least one state is unnamed", 0, 0);
                    break;
                }
                if (states.ContainsKey(state.Name))
                {
                    GeneratorErrorCallback(false, 1, $@"More than one  state is named {state.Name}", 0, 0);
                }
                else
                {
                    states.Add(state.Name, state);
                }
            }

            if (model.StateMachine.RequireCompleteEventCoverage)
            {
                foreach (var state in model.StateMachine.States)
                {
                    if (state.TransitionsFrom.Where(t => t.TransitionType == ViewModel.Transition.TransitionTypes.Normal).Count() != model.StateMachine.EventTypes.Count && 
                        state.TransitionsFrom.Where(t => t.TriggerEvent?.Name == ViewModel.EventType.WildcardEventTypeName && t.TransitionType == ViewModel.Transition.TransitionTypes.Normal).Count() == 0)
                    {
                        GeneratorErrorCallback(false, 1, $@"State {state.Name} is missing or more more transitions (some event types are missing as trigger events)", 0, 0);
                    }
                }
            }
            foreach (var state in model.StateMachine.States)
            {
                ValidateStateTransitions(state);
            }
        }

        private void ValidateStateTransitions(ViewModel.State state)
        {
            HashSet<ViewModel.EventType> triggerEvents = new HashSet<ViewModel.EventType>();

            foreach (var transition in state.TransitionsFrom.Where(t => t.TransitionType == ViewModel.Transition.TransitionTypes.Normal))
            {
                if (transition.DestinationState == null)
                {
                    GeneratorErrorCallback(false, 1, $@"A transition from state {state.Name} has no ending state", 0, 0);
                }
                if (transition.TriggerEvent == null)
                {
                    GeneratorErrorCallback(false, 1, $@"A transition from state {state.Name} to state {transition.DestinationState?.Name} has no trigger event assigned", 0, 0);
                }
                else if (triggerEvents.Contains(transition.TriggerEvent))
                {
                    GeneratorErrorCallback(false, 1, $@"At least two transitions from state {state.Name} have the same trigger event: {transition.TriggerEvent.Name}", 0, 0);
                }
                else
                {
                    triggerEvents.Add(transition.TriggerEvent);
                }
            }

            foreach (var transition in state.TransitionsTo.Where(t => t.TransitionType == ViewModel.Transition.TransitionTypes.Normal))
            {
                if (transition.SourceState == null)
                {
                    GeneratorErrorCallback(false, 1, $@"A transition ending at state {state.Name} has no beginning state", 0, 0);
                }
            }
        }
    }
}
