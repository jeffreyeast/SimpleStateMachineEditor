using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleStateMachine
{
    /// <summary>
    /// Raised if the current state machine state is an error state
    /// </summary>
    public class EnteredErrorStateException : Exception
    {
        public EnteredErrorStateException(StateMachineInternalBase stateMachine, string eventName, string currentStateName) : 
            base($@"Entered error state ""{currentStateName}"". State machine trace: {stateMachine.EventTrace}")
        {

        }
    }
}
