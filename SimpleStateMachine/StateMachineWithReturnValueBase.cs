using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleStateMachine
{
    /// <summary>
    /// The base class for Simple State Machines whose Execute method returns a value.
    /// </summary>
    public abstract class StateMachineWithReturnValueBase<R> : StateMachineInternalBase
    {
        public delegate R Action();

        StateTypes[] StateClassifications;
        protected abstract Transition<Action>[,] Transitions { get; }
        int? CurrentEvent;
        bool Executing;


        protected StateMachineWithReturnValueBase(StateTypes[] stateTypes, string[] eventNames, string[] stateNames) : base(eventNames, stateNames)
        {
            StateClassifications = stateTypes;
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
        public R Execute(int? e)
        {
            if (e.HasValue)
            {
                PostNormalPriorityEvent(e.Value);
            }

            R returnValue = default(R);

            if (!Executing)
            {
                try
                {
                    Executing = true;

                    while (StateClassifications[CurrentState] == StateTypes.Normal && (CurrentEvent = GetNextEvent()).HasValue)
                    {
                        EventTrace.Add(new EventTraceInstance(CurrentState, CurrentEvent.Value));
                        Transition<Action> transition = Transitions[CurrentState, CurrentEvent.Value];
                        foreach (Action a in transition.Actions)
                        {
                            returnValue = a();
                        }
                        CurrentState = transition.NextState;
                    }

                    if (StateClassifications[CurrentState] == StateTypes.Error)
                    {
                        throw new EnteredErrorState(StateNames[CurrentState]);
                    }
                }
                finally
                {
                    Executing = false;
                }
            }

            return returnValue;
        }

        /// <summary>
        /// Invoked when none of the transitions for the current state have a trigger event matching the current event type
        /// </summary>
        protected R InvalidTransition()
        {
            throw new UnexpectedEventException(this,
                (CurrentEvent.Value >= EventNames.GetLowerBound(0) && CurrentEvent.Value <= EventNames.GetUpperBound(0) ? EventNames[CurrentEvent.Value] : CurrentEvent.Value.ToString()),
                (CurrentState >= StateNames.GetLowerBound(0) && CurrentState <= StateNames.GetUpperBound(0) ? StateNames[CurrentState] : CurrentState.ToString()));
        }
    }
}
