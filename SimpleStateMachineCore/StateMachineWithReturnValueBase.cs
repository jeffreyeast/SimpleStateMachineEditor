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
        /// <summary>
        /// The base class for Simple State Machines whose Execute method does not return a value
        /// </summary>
        public delegate R Action();

        StateTypes[] StateClassifications;

        /// <summary>
        ///  Provides the state transition graph
        /// </summary>
        protected abstract Transition<Action>[,] Transitions { get; }
        int? CurrentEvent;
        bool Executing;


        protected StateMachineWithReturnValueBase(StateTypes[] stateTypes, string[] eventNames, string[] stateNames) : base(eventNames, stateNames)
        {
            StateClassifications = stateTypes;
            ExecutionState = ExecutionStates.Idle;
        }

        /// <summary>
        /// Invoked to execute the state machine.
        /// </summary>
        /// <param name="e">Provides an optional event type to post at normal priority before starting execution</param>
        /// <returns>A value of type R</returns>
        /// <exception cref="System.InvalidOperationException">Thrown if an event is chosen for 
        /// execution and no transition from the current state maches the event.
        /// </exception>
        /// <exception cref="EnteredErrorStateException">Thrown if execution enters an error state</exception>
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

            switch (ExecutionState)
            {
                case ExecutionStates.Idle:
                    ExecutionState = ExecutionStates.Executing;

                    while (StateClassifications[CurrentState] == StateTypes.Normal && (CurrentEvent = GetNextEvent()).HasValue)
                    {
                        EventTrace.Add(new EventTraceInstance(CurrentState, CurrentEvent.Value));
                        Transition<Action> transition = Transitions[CurrentState, CurrentEvent.Value];
                        foreach (Action a in transition.Actions)
                        {
                            try
                            {
                                returnValue = a();
                            }
                            catch (UnexpectedEventException)
                            {
                                throw;
                            }
                            catch (Exception exc)
                            {
                                ExecutionState = ExecutionStates.Exception;
                                throw new ActionRaisedException(this,
                                    (CurrentState >= StateNames.GetLowerBound(0) && CurrentState <= StateNames.GetUpperBound(0) ? StateNames[CurrentState] : CurrentState.ToString()),
                                    exc);
                            }
                        }
                        CurrentState = transition.NextState;
                    }

                    if (StateClassifications[CurrentState] == StateTypes.Error)
                    {
                        ExecutionState = ExecutionStates.Errored;
                        throw new EnteredErrorStateException(this,
                            (CurrentEvent.Value >= EventNames.GetLowerBound(0) && CurrentEvent.Value <= EventNames.GetUpperBound(0) ? EventNames[CurrentEvent.Value] : CurrentEvent.Value.ToString()),
                            (CurrentState >= StateNames.GetLowerBound(0) && CurrentState <= StateNames.GetUpperBound(0) ? StateNames[CurrentState] : CurrentState.ToString()));
                    }

                    ExecutionState = ExecutionStates.Idle;
                    return returnValue;

                case ExecutionStates.Errored:
                case ExecutionStates.Exception:
                    throw new EnteredErrorStateException(this,
                        (CurrentEvent.Value >= EventNames.GetLowerBound(0) && CurrentEvent.Value <= EventNames.GetUpperBound(0) ? EventNames[CurrentEvent.Value] : CurrentEvent.Value.ToString()),
                        (CurrentState >= StateNames.GetLowerBound(0) && CurrentState <= StateNames.GetUpperBound(0) ? StateNames[CurrentState] : CurrentState.ToString()));

                case ExecutionStates.Executing:
                case ExecutionStates.Finished:
                    return returnValue;

                default:
                    throw new NotImplementedException();
            }
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
