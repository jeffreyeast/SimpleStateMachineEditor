using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RuntimeTests
{
    public class StateMachineWithValueImplementation : StateMachineWithValue
    {
        protected override int RaiseException()
        {
            throw new Exception();
        }

        protected override int Return1()
        {
            return 1;
        }
    }
}
