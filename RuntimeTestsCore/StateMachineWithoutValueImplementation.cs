using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RuntimeTestsCore
{
    public class StateMachineWithoutValueImplementation : StateMachineWithoutValue
    {
        protected override void RaiseException()
        {
            throw new Exception();
        }
    }
}
