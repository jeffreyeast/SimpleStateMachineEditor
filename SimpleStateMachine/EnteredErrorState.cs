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
    public class EnteredErrorState : Exception
    {
        public EnteredErrorState(string stateName) : base ($@"State name: {stateName}")
        {

        }
    }
}
