using System;
using System.Collections.Generic;
using EventApi.Models;
using EventsApi.Contracts;

namespace EventApi.Implementation
{
    public class EventApi : IEventApi
    {
        private PayloadEvent[] _events;

        public IEnumerable<PayloadEvent> GetEvents(long startTick, long stopTick)
        {
            if (_events == null)
                throw new Exception("Call GetEvents for not preloaded api. Call LoadData first");

            if (startTick > stopTick)
                throw new ArgumentException("Incorrect request, start should be less than stop");

            if (_events.Length < 2)
                return new PayloadEvent[0];

            if (stopTick< _events[0].Ticks)
                return new PayloadEvent[0];

            if (startTick < _events[_events.Length-1].Ticks)
                return new PayloadEvent[0];

            var fromIndex = -1L;
            var toIndex = -1L;
            //stupid logic should be removed in future
            for (int i = 0; i < _events.Length; i++)
            {
                if (_events[i].Ticks <= startTick)
                    fromIndex = i;

                if (_events[i].Ticks <= stopTick)
                    continue;

                toIndex = i;
                break;
            }

            fromIndex = Math.Max(fromIndex, 0);
            toIndex = Math.Min(toIndex, _events.Length - 1);
            var count = toIndex - fromIndex;
            var resultArray = new PayloadEvent[count];
            Array.Copy(_events, fromIndex, resultArray,0, count);
            return resultArray;
        }

        public void LoadData(PayloadEvent[] events)
        {
            if (events == null)
                throw new ArgumentNullException(nameof(events));

            //do it only if necessary. If we trust to provider then can be removed. 
            var result = ValidateEvents(events);
            if (!result)
                throw new ArgumentException("Not valid input data. It should be sorted and presented in pairs", nameof(events));

            _events = events;
        }

        private bool ValidateEvents(IEnumerable<PayloadEvent> events)
        {
            var currentEventIsStart = true;
            var currentTime = 0L;

            foreach (var simpleEvent in events)
            {
                if ((simpleEvent.EventType == EventType.start) == currentEventIsStart)
                    return false;
                if (simpleEvent.Ticks < currentTime)
                    return false;

                currentEventIsStart = !currentEventIsStart;
                currentTime = simpleEvent.Ticks;
            }

            return !currentEventIsStart;
        }
    }
}
