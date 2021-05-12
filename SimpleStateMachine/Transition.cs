using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleStateMachine
{
    /// <summary>
    /// Represents a transition from one state to another, together with the actions to be executed
    /// </summary>
    public class Transition<A>
    {
        /// <summary>
        /// The end-state for the transition
        /// </summary>
        public int NextState;
        /// <summary>
        /// The set of actions to be executed before entering NextState
        /// </summary>
        public A[] Actions;

        public Transition(int nextState, A[] actions)
        {
            NextState = nextState;
            Actions = actions;
        }
    }
}
