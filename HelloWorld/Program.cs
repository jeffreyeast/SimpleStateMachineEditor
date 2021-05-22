using System;

namespace HelloWorld
{
    class Program
    {
        static void Main(string[] args)
        {
            HelloWorldActions stateMachine = new HelloWorldActions();
            stateMachine.Execute(HelloWorldStateMachine.EventTypes.SayHelloWorld);
        }
    }
}
