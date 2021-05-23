using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleStateMachine
{
    /// <summary>
    /// Raised if an Action method terminates by an exception. Call Reset before invoking Execute.
    /// </summary>
    public class ActionRaisedException : Exception
    {
        internal ActionRaisedException(StateMachineInternalBase stateMachine, string currentStateName, Exception actionException) :
                        base($@"Action terminated by an exception in state ""{currentStateName}"". State machine trace: {stateMachine.EventTrace}", actionException)
        {

        }
    }
}
