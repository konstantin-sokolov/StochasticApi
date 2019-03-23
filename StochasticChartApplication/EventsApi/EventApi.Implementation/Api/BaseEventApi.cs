using System;
using System.Threading.Tasks;
using EventApi.Implementation.DataProviders;
using EventApi.Models;

namespace EventApi.Implementation.Api
{
    public abstract class BaseEventApi :IDisposable
    {
        protected readonly IDataProvider _dataProvider;
        protected readonly long _globalStartTick;
        protected readonly long _globalStopTick;
        protected readonly long _globalEventsCount;

        protected BaseEventApi(IDataProvider dataProvider)
        {
            _dataProvider = dataProvider;
            _globalEventsCount = _dataProvider.GetGlobalEventsCount();
            _globalStartTick = _dataProvider.GetGlobalStartTick();
            _globalStopTick = _dataProvider.GetGlobalStopTick();
        }

        protected Task<long> GetStartIndexAsync(long startTick)
        {
            if (startTick < _globalStartTick)
                return Task.FromResult(0L);

            return Task.Run(() =>
            {
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
                        return rightStopEvent.Ticks > startTick ? nearestIndex - 2 : nearestIndex; //it can't be first, because of check in start of searching
                    }
                }
            });
        }
        protected Task<long> GetStopIndexAsync(long stopTick)
        {
            if (stopTick > _globalStopTick)
                return Task.FromResult(_globalEventsCount - 1);

            return Task.Run(() =>
            {

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
            });
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

        protected PayloadEvent GetEventAtIndex(long index)
        {
            return _dataProvider.GetEventAtIndex(index);
        }

        public void Dispose()
        {
            _dataProvider?.Dispose();
        }
    }
}
