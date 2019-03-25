using System;
using System.Threading;
using System.Threading.Tasks;
using EventApi.Implementation.DataProviders;
using EventApi.Implementation.Helpers;

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

        protected async Task<long> GetStartIndexAsync(long startTick, long? minIndex = null, long? maxIndex = null, CancellationToken ctn = default(CancellationToken))
        {
            if (startTick < _globalStartTick)
                return 0L;

            var searchResult = await FindNearestAsync(startTick, true, minIndex, maxIndex, ctn);
            ctn.ThrowIfCancellationRequested();

            switch (searchResult.CompareResult)
            {
                case 0:
                    return searchResult.FoundIndex;
                case -1:
                {
                    //nearest on the left side check next stop after nearest for intersection 
                    var leftStopEvent = await _dataProvider.GetEventAtIndexAsync(searchResult.FoundIndex + 1);
                    return leftStopEvent.Ticks < startTick ? searchResult.FoundIndex + 2 : searchResult.FoundIndex; //it can't be last, because of check in start of searching
                }
                default:
                {
                    //nearest on the right side 
                    var rightStopEvent = await _dataProvider.GetEventAtIndexAsync(searchResult.FoundIndex - 1);
                    return rightStopEvent.Ticks > startTick ? searchResult.FoundIndex - 2 : searchResult.FoundIndex; //it can't be first, because of check in start of searching
                }
            }
        }

        protected async Task<long> GetStopIndexAsync(long stopTick, long? minIndex = null, long? maxIndex = null, CancellationToken ctn = default(CancellationToken))
        {
            if (stopTick > _globalStopTick)
                return _globalEventsCount - 1;

            var searchResult = await FindNearestAsync(stopTick, false, minIndex, maxIndex, ctn);
            ctn.ThrowIfCancellationRequested();

            switch (searchResult.CompareResult)
            {
                case 0:
                    return searchResult.FoundIndex;
                case -1:
                {
                    //nearest on the left side check next stop after nearest for intersection 
                    var rightStartEvent = await _dataProvider.GetEventAtIndexAsync(searchResult.FoundIndex + 1);
                    return rightStartEvent.Ticks < stopTick ? searchResult.FoundIndex + 2 : searchResult.FoundIndex; //it can't be last, because of check in start of searching
                }
                default:
                {
                    //nearest on the right side 
                    var leftStartEvent = await _dataProvider.GetEventAtIndexAsync(searchResult.FoundIndex - 1);
                    return leftStartEvent.Ticks > stopTick ? searchResult.FoundIndex - 2 : searchResult.FoundIndex;
                }
            }
        }

        private async Task<SearchResult> FindNearestAsync(long ticks, bool even, long? minIndex = null, long? maxIndex = null, CancellationToken ctn = default(CancellationToken))
        {
            long first = minIndex / 2 ?? 0;
            long last = (maxIndex - 1) / 2 ?? _globalEventsCount / 2 - 1;
            long mid;
            int compareResult;
            do
            {
                ctn.ThrowIfCancellationRequested();

                mid = first + (last - first) / 2;
                var globalIndex = even ? 2 * mid : 2 * mid + 1;
                var item = await _dataProvider.GetEventAtIndexAsync(globalIndex);
                if (item.Ticks == ticks)
                {
                    return new SearchResult(0, globalIndex);
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

            return new SearchResult(compareResult, even ? 2 * mid : 2 * mid + 1);
        }

        public void Dispose()
        {
            _dataProvider?.Dispose();
        }
    }
}
