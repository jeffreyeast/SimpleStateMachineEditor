using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace SimpleStateMachine
{
    /// <summary>
    /// The base class for all Simple State Machines
    /// </summary>
    public abstract class StateMachineInternalBase
    {
        /// <summary>
        /// Describes the usage of the state
        /// </summary>
        public enum StateTypes
        {
            /// <summary>
            /// States which are in the usual execution flow
            /// </summary>
            Normal,
            /// <summary>
            /// Terminal state, execution will cease once the state machine enters this state
            /// </summary>
            Finish,
            /// <summary>
            /// States which indicate the machine is in an error state. Execution ceases once entered.
            /// </summary>
            Error,
        }


        internal string[] EventNames;
        internal string[] StateNames;
        LinkedList<int> NormalPriorityEventQueue = new LinkedList<int>();
        LinkedList<int> HighPriorityEventQueue = new LinkedList<int>();
        protected int CurrentState;
        protected abstract int StartState { get; }
        internal EventTraceList EventTrace;

        const int EventTraceDepth = 10;



        protected StateMachineInternalBase(string[] eventNames, string[]stateNames)
        {
            EventNames = eventNames;
            StateNames = stateNames;
            CurrentState = StartState;
            EventTrace = new EventTraceList(EventTraceDepth, EventNames, StateNames);
        }

        protected int? GetNextEvent()
        {
            LinkedListNode<int> node = HighPriorityEventQueue.First;
            if (node == null)
            {
                node = NormalPriorityEventQueue.First;
                if (node != null)
                {
                    NormalPriorityEventQueue.RemoveFirst();
                    return node.Value;
                }
            }
            else
            {
                HighPriorityEventQueue.RemoveFirst();
                return node.Value;
            }

            return null;
        }

        /// <summary>
        /// Invoked by an action routine to post an internal (high-priority) event.
        /// <param name="eventType">Identifies the event to be posted</param>
        /// <exception cref="ArgumentOutOfRangeException">If the eventType is not valid</exception>
        /// </summary>
        protected void PostHighPriorityEvent(int eventType)
        {
            if (eventType < EventNames.GetLowerBound(0) || eventType > EventNames.GetUpperBound(0))
            {
                throw new ArgumentOutOfRangeException("eventType");
            }
            HighPriorityEventQueue.AddLast(eventType);
        }

        /// <summary>
        /// Invoked by any code to post an external (lower-priority) event.
        /// <param name=eventType>Identifies the event to be posted</param>
        /// <exception cref="ArgumentOutOfRangeException">If the eventType is not valid</exception>
        /// </summary>
        protected void PostNormalPriorityEvent(int eventType)
        {
            if (eventType < EventNames.GetLowerBound(0) || eventType > EventNames.GetUpperBound(0))
            {
                throw new ArgumentOutOfRangeException("eventType");
            }
            NormalPriorityEventQueue.AddLast(eventType);
        }

        /// <summary>
        /// Invoked to reset the state machine.
        /// </summary>
        public void Reset()
        {
            CurrentState = StartState;
            NormalPriorityEventQueue.Clear();
            HighPriorityEventQueue.Clear();
            EventTrace.Reset();
        }

        /// <summary>
        /// Provides a programmer-friendly representation of the most recently executed state machine transitions
        /// <returns>A string representing the trace</returns>
        /// </summary>
        public string Trace
        {
            get
            {
                string result = EventTrace.ToString();
                return result + (result.Length == 0 ? "" : "=>") + ((CurrentState >= StateNames.GetLowerBound(0) && CurrentState <= StateNames.GetUpperBound(0)) ? StateNames[CurrentState] : CurrentState.ToString());
            }
        }

    }
}
