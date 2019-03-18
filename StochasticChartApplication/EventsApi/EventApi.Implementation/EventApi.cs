using System.Diagnostics;
using EventApi.Implementation.DataProviders;
using EventApi.Models;
using EventsApi.Contracts;
using NLog;

namespace EventApi.Implementation
{
    public class EventApi : IEventApi
    {
        private readonly ILogger _logger;
        private readonly IDataProvider _dataProvider;
        private readonly long _globalStartTick;
        private readonly long _globalStopTick;
        private readonly long _globalEventsCount;

        public EventApi(ILogger logger, IDataProvider dataProvider)
        {
            _logger = logger;
            _dataProvider = dataProvider;
            _globalEventsCount = _dataProvider.GetGlobalEventsCount();
            _globalStartTick = _dataProvider.GetGlobalStartTick();
            _globalStopTick = _dataProvider.GetGlobalStopTick();
        }

        public PayloadEvent[] GetEvents(long startTick, long stopTick)
        {
            _logger.Info($"EventApi: Requested events between: {startTick}-{stopTick}");
            if (_globalEventsCount == 0)
            {
                _logger.Info("EventApi: _globalEventsCount == 0 - return empty results");
                return new PayloadEvent[0];
            }
            if (startTick > _globalStopTick)
            {
                _logger.Info("EventApi: startTick > _globalStopTick - return empty results");
                return new PayloadEvent[0];
            }

            if (stopTick < _globalStartTick)
            {
                _logger.Info("EventApi: stopTick < _globalStartTick - return empty results");
                return new PayloadEvent[0];
            }

            var stopWatch = new Stopwatch();
            stopWatch.Start();
            var startIndex = GetStartIndex(startTick);
            var stopIndex = GetStopIndex(stopTick);
            var result = _dataProvider.GetEventsBetween(startIndex, stopIndex);
            stopWatch.Stop();
            _logger.Info($"EventApi: Got {result.Length} events in {stopWatch.ElapsedMilliseconds}ms");
            return result;
        }

        private long GetStartIndex(long startTick)
        {
            if (startTick < _globalStartTick)
                return 0;

            var compareResult = FindNearest(startTick, true, out var nearestIndex);
            switch (compareResult)
            {
                case 0:
                    return nearestIndex;
                case -1:
                {
                    //nearest on the left side check next stop after nearest for intersection 
                    var leftStopEvent = _dataProvider.GetEventAtIndex(nearestIndex + 1);
                    return leftStopEvent.Ticks < startTick ? nearestIndex + 2 : nearestIndex; //it can't be last, because of check in start of searching
                }
                default:
                {
                    //nearest on the right side 
                    var rightStopEvent = _dataProvider.GetEventAtIndex(nearestIndex - 1);
                    return rightStopEvent.Ticks > startTick ? nearestIndex - 2 : nearestIndex;//it can't be first, because of check in start of searching
                    }
            }
        }

        private long GetStopIndex(long stopTick)
        {
            if (stopTick > _globalStopTick)
                return _globalEventsCount-1;

            var compareResult = FindNearest(stopTick, false, out var nearestIndex);
            switch (compareResult)
            {
                case 0:
                    return nearestIndex;
                case -1:
                { 
                    //nearest on the left side check next stop after nearest for intersection 
                    var rightStartEvent = _dataProvider.GetEventAtIndex(nearestIndex + 1);
                    return rightStartEvent.Ticks < stopTick ? nearestIndex + 2 : nearestIndex; //it can't be last, because of check in start of searching
                }
                default:
                {
                    //nearest on the right side 
                    var leftStartEvent = _dataProvider.GetEventAtIndex(nearestIndex - 1);
                    return leftStartEvent.Ticks > stopTick ? nearestIndex - 2 : nearestIndex;
                }
            }

        }

        private int FindNearest(long ticks, bool even, out long index)
        {
            long first = 0;
            long last = _globalEventsCount / 2 - 1;
            long mid;
            int compareResult;
            do
            {
                mid = first + (last - first) / 2;
                var globalIndex = even ? 2 * mid : 2 * mid + 1;
                var item = GetEventAtIndex(globalIndex);
                if (item.Ticks == ticks)
                {
                    index = globalIndex;
                    return 0;
                }

                if (item.Ticks < ticks)
                {
                    first = mid + 1;
                    compareResult = -1;
                }
                else
                {
                    last = mid - 1;
                    compareResult = 1;
                }
            } while (first <= last);
            index = even ? 2 * mid : 2 * mid + 1;
            return compareResult;
        }

        private PayloadEvent GetEventAtIndex(long index)
        {
            return _dataProvider.GetEventAtIndex(index);
        }
    }
}
