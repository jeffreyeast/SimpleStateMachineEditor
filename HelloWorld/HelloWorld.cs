// ------------------------------------------------------------------------------
// <auto-generated>
//     This file was generated by SfsaGenerator
// </auto-generated>
// ------------------------------------------------------------------------------

namespace HelloWorld
{
    using System;
    using System.Collections.Generic;
    using SimpleStateMachine;

    ///<summary>Sample state machine</summary>
    public abstract class HelloWorldStateMachine : StateMachineWithoutReturnValueBase
    {
        public enum EventTypes
        {
            SayHelloWorld,
        };

        static readonly string[] EventTypeNames = new string[]
        {
            "SayHelloWorld",
        };

        static readonly string[] StateNames = new string[]
        {
            "Done",
            "Start",
        };

        protected override int StartState => Array.IndexOf(StateNames, "Start");

        static readonly StateTypes[] StateClassifications = new StateTypes[]
        {
            StateTypes.Normal,
            StateTypes.Normal,
        };

        /// <summary>
        /// Action Routines
        /// 
        /// You must override each of these action routines in your implementation.
        /// </summary>

        protected abstract void DoSayHello();


        protected override Transition<Action>[,] Transitions => _transitions;
        Transition<Action>[,] _transitions;

        public HelloWorldStateMachine() : base (StateClassifications, EventTypeNames, StateNames)
        {
            _transitions = new Transition<Action>[,]
            {
                { // Done(0)
                    new Transition<Action>(0, new Action[] { base.InvalidTransition, }),  // SayHelloWorld(0)
                },
                { // Start(1)
                    new Transition<Action>(0, new Action[] { DoSayHello, }),  // SayHelloWorld(0)
                },
            };
        }

        /// <summary>
        /// Invoked to execute the state machine.
        /// </summary>
        /// <param name="e">Provides an optional event type to post at normal priority before starting execution</param>

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
        public void Execute(EventTypes? e = null)
        {
             base.Execute(e.HasValue ? (int)e.Value : default(int?));
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