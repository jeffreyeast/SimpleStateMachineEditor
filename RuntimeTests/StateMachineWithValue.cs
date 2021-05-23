// ------------------------------------------------------------------------------
// <auto-generated>
//     This file was generated by SfsaGenerator
// </auto-generated>
// ------------------------------------------------------------------------------

namespace RuntimeTests
{
    using System;
    using System.Collections.Generic;
    using SimpleStateMachine;

    public abstract class StateMachineWithValue : StateMachineWithReturnValueBase<int>
    {
        public enum EventTypes
        {
            Advance,
            BeginTest,
            CauseActionError,
            CauseError,
            Finish,
        };

        static readonly string[] EventTypeNames = new string[]
        {
            "Advance",
            "BeginTest",
            "CauseActionError",
            "CauseError",
            "Finish",

        };

        static readonly string[] StateNames = new string[]
        {
            "Done",
            "S1",
            "S2",
            "Start",
            "Starting",

        };

        protected override int StartState => Array.IndexOf(StateNames, "Start");

        static readonly StateTypes[] StateClassifications = new StateTypes[]
        {

            StateTypes.Finish,
            StateTypes.Normal,
            StateTypes.Error,
            StateTypes.Normal,
            StateTypes.Normal,
        };

        /// <summary>
        /// Action Routines
        /// 
        /// You must override each of these action routines in your implementation.
        /// </summary>

        protected abstract int RaiseException();
        protected abstract int Return1();


        protected override Transition<Action>[,] Transitions => _transitions;
        Transition<Action>[,] _transitions;

        public StateMachineWithValue() : base (StateClassifications, EventTypeNames, StateNames)
        {
            _transitions = new Transition<Action>[,]
            {
                { // Done(0)
                    new Transition<Action>(0, new Action[] { base.InvalidTransition, }),  // Advance(0)
                    new Transition<Action>(0, new Action[] { base.InvalidTransition, }),  // BeginTest(1)
                    new Transition<Action>(0, new Action[] { base.InvalidTransition, }),  // CauseActionError(2)
                    new Transition<Action>(0, new Action[] { base.InvalidTransition, }),  // CauseError(3)
                    new Transition<Action>(0, new Action[] { base.InvalidTransition, }),  // Finish(4)
                },
                { // S1(1)
                    new Transition<Action>(0, new Action[] { base.InvalidTransition, }),  // Advance(0)
                    new Transition<Action>(0, new Action[] { base.InvalidTransition, }),  // BeginTest(1)
                    new Transition<Action>(0, new Action[] { RaiseException, }),  // CauseActionError(2)
                    new Transition<Action>(2, new Action[] { }),  // CauseError(3)
                    new Transition<Action>(0, new Action[] { Return1, }),  // Finish(4)
                },
                { // S2(2)
                    new Transition<Action>(0, new Action[] { base.InvalidTransition, }),  // Advance(0)
                    new Transition<Action>(0, new Action[] { base.InvalidTransition, }),  // BeginTest(1)
                    new Transition<Action>(0, new Action[] { base.InvalidTransition, }),  // CauseActionError(2)
                    new Transition<Action>(0, new Action[] { base.InvalidTransition, }),  // CauseError(3)
                    new Transition<Action>(0, new Action[] { base.InvalidTransition, }),  // Finish(4)
                },
                { // Start(3)
                    new Transition<Action>(0, new Action[] { base.InvalidTransition, }),  // Advance(0)
                    new Transition<Action>(4, new Action[] { }),  // BeginTest(1)
                    new Transition<Action>(0, new Action[] { base.InvalidTransition, }),  // CauseActionError(2)
                    new Transition<Action>(0, new Action[] { base.InvalidTransition, }),  // CauseError(3)
                    new Transition<Action>(0, new Action[] { base.InvalidTransition, }),  // Finish(4)
                },
                { // Starting(4)
                    new Transition<Action>(1, new Action[] { }),  // Advance(0)
                    new Transition<Action>(0, new Action[] { base.InvalidTransition, }),  // BeginTest(1)
                    new Transition<Action>(0, new Action[] { base.InvalidTransition, }),  // CauseActionError(2)
                    new Transition<Action>(0, new Action[] { base.InvalidTransition, }),  // CauseError(3)
                    new Transition<Action>(0, new Action[] { base.InvalidTransition, }),  // Finish(4)
                },
            };
        }

        /// <summary>
        /// Invoked to execute the state machine.
        /// </summary>
        /// <param name="e">Provides an optional event type to post at normal priority before starting execution</param>
        /// <returns>A value of type R</returns>
        /// <exception cref="System.InvalidOperationException">Thrown if an event is chosen for 
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
        public int Execute(EventTypes? e = null)
        {
            return  base.Execute(e.HasValue ? (int)e.Value : default(int?));
        }

        /// <summary>
        /// Invoked by an action routine to post an internal (high-priority) event.
        /// <param name=eventType>Identifies the event to be posted</param>
        /// <exception cref="ArgumentOutOfRangeException">If the eventType is not valid</exception>
        /// </summary>
        protected void PostHighPriorityEvent(EventTypes eventType)
        {
            PostHighPriorityEvent((int)eventType);
        }

        /// <summary>
        /// Invoked by any code to post an external (lower-priority) event.
        /// <param name=eventType>Identifies the event to be posted</param>
        /// <exception cref="ArgumentOutOfRangeException">If the eventType is not valid</exception>
        /// </summary>
        public void PostNormalPriorityEvent(EventTypes eventType)
        {
            PostNormalPriorityEvent((int)eventType);
        }
    }
}