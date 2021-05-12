using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleStateMachine
{
    /// <summary>
    /// Raised when an event does not match any of the trigger events for the current state
    /// </summary>
    public class UnexpectedEventException : Exception
    {
        public UnexpectedEventException(StateMachineInternalBase stateMachine, string eventName, string currentStateName) :
            base($@"Unexpected event ""{eventName}"" encountered in state ""{currentStateName}"". State machine trace: {stateMachine.EventTrace}")
        {
        }
    }
}
