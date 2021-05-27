using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelloWorld
{
    /// <summary>
    /// The implementation of the HelloWorldStateMachine actions
    /// </summary>
    public class HelloWorldActions : HelloWorld.HelloWorldStateMachine
    {
        protected override void DoSayHello()
        {
            Debug.WriteLine("Hello World!");
        }
    }
}
