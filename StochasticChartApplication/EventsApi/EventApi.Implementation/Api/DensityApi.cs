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

        public List<DensityInfo> GetDensityInfo(long startTick, long stopTick, long groupInterval)
        {
            if (_globalEventsCount == 0)
                return new List<DensityInfo>(0);

            if (stopTick < startTick)
                return new List<DensityInfo>(0);

            startTick = Math.Max(startTick, _globalStartTick);
            stopTick = Math.Min(stopTick, _globalStopTick);

            var stopWatch = new Stopwatch();
            stopWatch.Start();

            var result = new List<DensityInfo>();
            long searchStartIndex = 0;
            do
            {
                var endTick = Math.Min(startTick + groupInterval, stopTick);

                var searchStartIndexTask = GetStartIndexAsync(startTick, minIndex: searchStartIndex);
                var searchStopIndexTask = GetStopIndexAsync(endTick, minIndex: searchStartIndex);
                Task.WaitAll(searchStartIndexTask, searchStopIndexTask);

                var startIndex = searchStartIndexTask.Result;
                var stopIndex = searchStopIndexTask.Result;
                if (stopIndex < startIndex)
                {
                    startTick = endTick + 1;
                    searchStartIndex = stopIndex + 1;
                    continue;
                }

                //additional get - think about remove it in future
                var startEvent = _dataProvider.GetEventAtIndex(startIndex);
                var stopEvent = _dataProvider.GetEventAtIndex(stopIndex);
                result.Add(new DensityInfo()
                {
                    EventsCount = stopIndex - startIndex + 1,
                    Start = startEvent.Ticks,
                    Stop = stopEvent.Ticks,
                    StartIndex = startIndex,
                    StopIndex = stopIndex
                });

                startTick = Math.Max(stopEvent.Ticks, endTick) + 1;
                searchStartIndex = stopIndex + 1;
            } while (startTick < stopTick);

            stopWatch.Stop();
            _logger.Info($"DensityApi: GetDensityInfo - got {result.Count} densities in {stopWatch.ElapsedMilliseconds}ms");
            return result;
        }

        private List<DensityInfo> SplitSingleDensityInfo(DensityInfo info, long groupInterval)
        {
            var result = new List<DensityInfo>();
            if (info.StopIndex == info.StartIndex + 1)
                return new List<DensityInfo>{info};
            if (_globalEventsCount == 0)
                return new List<DensityInfo>(0);

            var startTick = Math.Max(info.Start, _globalStartTick);
            var stopTick = Math.Min(info.Stop, _globalStopTick);

            var stopWatch = new Stopwatch();
            stopWatch.Start();

            long searchStartIndex = info.StartIndex;
            do
            {
                var endTick = Math.Min(startTick + groupInterval, stopTick);

                var searchStartIndexTask = GetStartIndexAsync(startTick, minIndex: searchStartIndex, maxIndex: info.StopIndex);
                var searchStopIndexTask = GetStopIndexAsync(endTick, searchStartIndex, info.StopIndex);
                Task.WaitAll(searchStartIndexTask, searchStopIndexTask);

                var startIndex = searchStartIndexTask.Result;
                var stopIndex = searchStopIndexTask.Result;
                if (stopIndex < startIndex)
                {
                    startTick = endTick + 1;
                    searchStartIndex = stopIndex + 1;
                    continue;
                }

                //additional get - think about remove it in future
                var startEvent = _dataProvider.GetEventAtIndex(startIndex);
                var stopEvent = _dataProvider.GetEventAtIndex(stopIndex);
                result.Add(new DensityInfo()
                {
                    EventsCount = stopIndex - startIndex + 1,
                    Start = startEvent.Ticks,
                    Stop = stopEvent.Ticks,
                    StartIndex = startIndex,
                    StopIndex = stopIndex
                });

                startTick = Math.Max(stopEvent.Ticks, endTick) + 1;
                searchStartIndex = stopIndex + 1;
            } while (startTick < stopTick);

            stopWatch.Stop();
            _logger.Info($"DensityApi: GetDensityInfo - got {result.Count} densities in {stopWatch.ElapsedMilliseconds}ms");
            return result;
        }
        public List<DensityInfo> SplitDensityInfo(List<DensityInfo> currentInfo, long start, long stop, long groupInterval)
        {
            List<Task<List<DensityInfo>>> splitTasks = new List<Task<List<DensityInfo>>>();
            for (int i = 0; i < currentInfo.Count; i++)
            {
                if (currentInfo[i].Stop<start)
                    continue;
                if (currentInfo[i].Start>stop)
                    continue;

                var i1 = i;
                splitTasks.Add(Task.Run(() => SplitSingleDensityInfo(currentInfo[i1], groupInterval)));
            }

            Task.WaitAll(splitTasks.ToArray());

            var result = new List<DensityInfo>();
            for (int i = 0; i < splitTasks.Count; i++)
            {
                var splitedDensities = splitTasks[i].Result;
                for (int j = 0; j < splitedDensities.Count; j++)
                {
                    if (splitedDensities[j].Stop < start)
                        continue;
                    if (splitedDensities[j].Start > stop)
                        continue;
                    result.Add(splitedDensities[j]);
                }
            }

            return result;
        }
    }
}
