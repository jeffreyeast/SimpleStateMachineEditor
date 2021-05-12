using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleStateMachine
{
    /// <summary>
    /// Provides an efficient representation of a state/event combination
    /// </summary>
    internal struct EventTraceInstance
    {
        public int State;
        public int Event;

        internal EventTraceInstance(int s, int e)
        {
            State = s;
            Event = e;
        }

        public string ToString(string[] eventNames, string[] stateNames)
        {
            return ((Event >= eventNames.GetLowerBound(0) && Event <= eventNames.GetUpperBound(0)) ? eventNames[Event] : Event.ToString()) + "@" +
                ((State >= stateNames.GetLowerBound(0) && State <= stateNames.GetUpperBound(0)) ? stateNames[State] : State.ToString());
        }

        public override string ToString()
        {
            return Event.ToString() + "@" + State.ToString();
        }
    }

    /// <summary>
    /// Implements a circular list of state/event instances. Useful for efficiently tracing
    /// the execution of a state machine.
    /// </summary>
    internal class EventTraceList : IEnumerable<EventTraceInstance>
    {
        class EventTraceListEnumerator : IEnumerator<EventTraceInstance>
        {
            int _current;
            EventTraceList _eventList;

            public EventTraceListEnumerator(EventTraceList eventList)
            {
                _eventList = eventList;
                Reset();
            }

            public EventTraceInstance Current => _eventList._events[_current];

            object System.Collections.IEnumerator.Current => _eventList._events[_current];

            public void Dispose()
            {
                _eventList = null;
            }

            public bool MoveNext()
            {
                _current = (_current + 1) % _eventList._events.Length;
                if (_current == _eventList._next)
                {
                    return false;
                }
                return true;
            }

            public void Reset()
            {
                _current = (_eventList._oldest - 1) % _eventList._events.Length;
            }
        }



        EventTraceInstance[] _events;
        string[] EventNames;
        string[] StateNames;
        int _next;
        int _oldest;

        internal EventTraceList(int depth, string[] eventNames, string[] stateNames)
        {
            _events = new EventTraceInstance[depth];
            EventNames = eventNames;
            StateNames = stateNames;
            Reset();
        }

        internal void Add(EventTraceInstance newTraceInstance)
        {
            _events[_next] = newTraceInstance;
            _next = (_next + 1) % _events.Length;
            if (_oldest == _next)
            {
                _oldest = (_oldest + 1) % _events.Length;
            }
        }

        public IEnumerator<EventTraceInstance> GetEnumerator()
        {
            return new EventTraceListEnumerator(this);
        }

        public void Reset()
        {
            _next = 0;
            _oldest = 0;
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return new EventTraceListEnumerator(this);
        }

        public override string ToString()
        {
            string result = "";
            string separator = "";

            foreach (EventTraceInstance e in this)
            {
                result += separator + e.ToString(EventNames, StateNames);
                separator = "=>";
            }

            return result;
        }
    }
}
