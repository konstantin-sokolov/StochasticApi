using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using EventApi.Implementation.DataProviders;
using EventApi.Models;
using EventsApi.Contracts;
using NLog;

namespace EventApi.Implementation.Api
{
    public class DensityApi : BaseEventApi,IDensityApi
    {
        private readonly ILogger _logger;

        public DensityApi(ILogger logger, IDataProvider dataProvider):base(dataProvider)
        {
            _logger = logger;
        }

        public IEnumerable<DensityInfo> GetDensityInfo(long startTick, long stopTick, long groupInterval)
        {
            if (_globalEventsCount == 0)
                return new DensityInfo[0];

            if (stopTick < startTick)
                return new DensityInfo[0];
            startTick = Math.Max(startTick, _globalStartTick);
            stopTick = Math.Min(stopTick, _globalStopTick);

            var stopWatch = new Stopwatch();
            stopWatch.Start();

            var result = new List<DensityInfo>();

            do
            {
                var endTick = Math.Min(startTick + groupInterval, stopTick);

                var searchStartIndexTask = GetStartIndexAsync(startTick);
                var searchStopIndexTask = GetStopIndexAsync(endTick);
                Task.WaitAll(searchStartIndexTask, searchStopIndexTask);

                var startIndex = searchStartIndexTask.Result;
                var stopIndex = searchStopIndexTask.Result;
                if (stopIndex < startIndex)
                {
                    startTick = endTick + 1;
                    continue;
                }

                //additional get - think about remove it in future
                var startEvent = _dataProvider.GetEventAtIndex(startIndex);
                var stopEvent = _dataProvider.GetEventAtIndex(stopIndex);
                result.Add(new DensityInfo()
                {
                    EventsCount = stopIndex - startIndex + 1,
                    Start = startEvent.Ticks,
                    Stop = stopEvent.Ticks
                });

                startTick = Math.Max(stopEvent.Ticks, endTick) + 1;
            } while (startTick < stopTick);

            stopWatch.Stop();
            _logger.Info($"DensityApi: GetDensityInfo - got {result.Count} densities in {stopWatch.ElapsedMilliseconds}ms");
            return result;
        }
    }
}
