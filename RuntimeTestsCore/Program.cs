using SimpleStateMachine;
using System;

namespace RuntimeTestsCore
{
    class Program
    {
        static void Main(string[] args)
        {
            bool failed = false;

            failed = TestNormalFlow();
            failed = TestNormalFlowWithResult();
            failed = TestEventAfterFinish() || failed;
            failed = TestErrorFlow() || failed;
            failed = TestEventAfterError() || failed;
            failed = TestActionException() || failed;
            failed = TestEventAfterActionException() || failed;
            failed = TestInvalidTransition() || failed;

            if (failed)
            {
                Console.WriteLine("Tests failed");
            }
            else
            {
                Console.WriteLine("Tests passed");
            }
        }

        static bool TestNormalFlow()
        {
            try
            {
                StateMachineWithoutValueImplementation sm = new StateMachineWithoutValueImplementation();

                sm.Execute(StateMachineWithoutValue.EventTypes.BeginTest);
                sm.Execute(StateMachineWithoutValue.EventTypes.Advance);
                sm.Execute(StateMachineWithoutValue.EventTypes.Finish);
                return false;
            }
            catch (Exception e)
            {
                Console.WriteLine("TestNormalFlow raised " + e.Message);
                return true;
            }
        }

        static bool TestNormalFlowWithResult()
        {
            try
            {
                StateMachineWithValueImplementation sm = new StateMachineWithValueImplementation();

                sm.Execute(StateMachineWithValue.EventTypes.BeginTest);
                sm.Execute(StateMachineWithValue.EventTypes.Advance);
                int returnValue = sm.Execute(StateMachineWithValue.EventTypes.Finish);
                if (returnValue != 1)
                {
                    Console.WriteLine("TestNormalFlowWithResult returned " + returnValue.ToString());
                    return true;
                }
                return false;
            }
            catch (Exception e)
            {
                Console.WriteLine("TestNormalFlowWithResult raised " + e.Message);
                return true;
            }
        }

        static bool TestEventAfterFinish()
        {
            try
            {
                StateMachineWithoutValueImplementation sm = new StateMachineWithoutValueImplementation();

                sm.Execute(StateMachineWithoutValue.EventTypes.BeginTest);
                sm.Execute(StateMachineWithoutValue.EventTypes.Advance);
                sm.Execute(StateMachineWithoutValue.EventTypes.Finish);
                sm.Execute(StateMachineWithoutValue.EventTypes.Finish);
                return false;
            }
            catch (Exception e)
            {
                Console.WriteLine("TestEventAfterFinish raised " + e.Message);
                return true;
            }
        }

        static bool TestErrorFlow()
        {
            try
            {
                StateMachineWithoutValueImplementation sm = new StateMachineWithoutValueImplementation();

                sm.Execute(StateMachineWithoutValue.EventTypes.BeginTest);
                sm.Execute(StateMachineWithoutValue.EventTypes.Advance);
                try
                {
                    sm.Execute(StateMachineWithoutValue.EventTypes.CauseError);
                    Console.WriteLine("TestErrorFlow failed");
                    return true;
                }
                catch (EnteredErrorStateException)
                {
                    //  This was supposed to happen
                }
                return false;
            }
            catch (Exception e)
            {
                Console.WriteLine("TestErrorFlow raised " + e.Message);
                return true;
            }
        }

        static bool TestEventAfterError()
        {
            try
            {
                StateMachineWithoutValueImplementation sm = new StateMachineWithoutValueImplementation();

                sm.Execute(StateMachineWithoutValue.EventTypes.BeginTest);
                sm.Execute(StateMachineWithoutValue.EventTypes.Advance);
                try
                {
                    sm.Execute(StateMachineWithoutValue.EventTypes.CauseError);
                    Console.WriteLine("TestEventAfterError failed");
                    return true;
                }
                catch (EnteredErrorStateException)
                {
                    try
                    {
                        sm.Execute(StateMachineWithoutValue.EventTypes.Finish);
                        Console.WriteLine("TestEventAfterError failed");
                        return true;
                    }
                    catch (EnteredErrorStateException)
                    {
                    }
                }
                return false;
            }
            catch (Exception e)
            {
                Console.WriteLine("TestEventAfterError raised " + e.Message);
                return true;
            }
        }

        static bool TestActionException()
        {
            try
            {
                StateMachineWithoutValueImplementation sm = new StateMachineWithoutValueImplementation();

                sm.Execute(StateMachineWithoutValue.EventTypes.BeginTest);
                sm.Execute(StateMachineWithoutValue.EventTypes.Advance);
                try
                {
                    sm.Execute(StateMachineWithoutValue.EventTypes.CauseActionError);
                    Console.WriteLine("TestActionException failed");
                    return true;
                }
                catch (ActionRaisedException)
                {
                    //  This was supposed to happen
                }
                return false;
            }
            catch (Exception e)
            {
                Console.WriteLine("TestActionException raised " + e.Message);
                return true;
            }
        }

        static bool TestEventAfterActionException()
        {
            try
            {
                StateMachineWithoutValueImplementation sm = new StateMachineWithoutValueImplementation();

                sm.Execute(StateMachineWithoutValue.EventTypes.BeginTest);
                sm.Execute(StateMachineWithoutValue.EventTypes.Advance);
                try
                {
                    sm.Execute(StateMachineWithoutValue.EventTypes.CauseActionError);
                    Console.WriteLine("TestErrorFlow failed");
                    return true;
                }
                catch (ActionRaisedException)
                {
                    try
                    {
                        sm.Execute(StateMachineWithoutValue.EventTypes.Finish);
                        Console.WriteLine("TestEventAfterActionException failed");
                        return true;
                    }
                    catch (EnteredErrorStateException)
                    {
                    }
                }
                return false;
            }
            catch (Exception e)
            {
                Console.WriteLine("TestEventAfterActionException raised " + e.Message);
                return true;
            }
        }

        static bool TestInvalidTransition()
        {
            try
            {
                StateMachineWithoutValueImplementation sm = new StateMachineWithoutValueImplementation();

                sm.Execute(StateMachineWithoutValue.EventTypes.BeginTest);
                sm.Execute(StateMachineWithoutValue.EventTypes.Advance);
                try
                {
                    sm.Execute(StateMachineWithoutValue.EventTypes.Advance);
                }
                catch (UnexpectedEventException)
                {
                    return false;
                }
                Console.WriteLine("TestInvalidTransition failed");
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("TestInvalidTransition raised " + e.Message);
                return true;
            }
        }
    }
}
