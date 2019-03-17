using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using EventApi.Models;
using EventsApi.Contracts;
using NLog;

namespace EventApi.Implementation
{
    public class EventApi : IEventApi
    {
        private readonly ILogger _logger;
        private PayloadEvent[] _events;

        public EventApi(ILogger logger)
        {
            _logger = logger;
        }

        public IEnumerable<PayloadEvent> GetEvents(long startTick, long stopTick)
        {
            if (_events == null)
            {
                var message = "Call GetEvents for not preloaded api. Call LoadData first";
                _logger.Error(message);
                throw new Exception(message);
            }

            if (startTick > stopTick)
            {
                var message = "Incorrect request, start should be less than stop";
                _logger.Error(message);
                throw new ArgumentException(message);
            }

            try
            {
                if (_events.Length < 2)
                    return new PayloadEvent[0];

                if (stopTick < _events[0].Ticks)
                    return new PayloadEvent[0];

                if (startTick < _events[_events.Length - 1].Ticks)
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
                Array.Copy(_events, fromIndex, resultArray, 0, count);
                return resultArray;
            }
            catch (Exception e)
            {
               _logger.Error($"Error while GetEvents from EventApi:{e.Message} - {e.StackTrace}");
               throw e;
            }
        }

        public void LoadData(PayloadEvent[] events)
        {
            if (events == null)
            {
                _logger.Error("Call LoadData with null data");
                throw new ArgumentNullException(nameof(events));
            }

            //do it only if necessary. If we trust to provider then can be removed. 
            var result = ValidateEvents(events);
            if (!result)
            {
                var message = "Not valid input data. It should be sorted and presented in pairs";
                _logger.Error(message);
                throw new ArgumentException(message, nameof(events));
            }

            _events = events;
        }

        private bool ValidateEvents(IEnumerable<PayloadEvent> events)
        {
            var currentEventIsStart = true;
            var currentTime = 0L;

            foreach (var simpleEvent in events)
            {
                if ((simpleEvent.EventType == EventType.start) != currentEventIsStart)
                    return false;
                if (simpleEvent.Ticks < currentTime)
                    return false;

                currentEventIsStart = !currentEventIsStart;
                currentTime = simpleEvent.Ticks;
            }

            return currentEventIsStart;
        }
    }
}
