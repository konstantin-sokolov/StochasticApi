﻿using System;
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

        private List<DensityInfo> GetDensityInfo(long startTick,long stopTick, long searchStartIndex,long searchStopIndex, long groupInterval)
        {
            var result = new List<DensityInfo>();
            var stopWatch = new Stopwatch();
            stopWatch.Start();

            do
            {
                var endTick = Math.Min(startTick + groupInterval, stopTick);

                var searchStartIndexTask = GetStartIndexAsync(startTick, minIndex: searchStartIndex, maxIndex:searchStopIndex);
                var searchStopIndexTask = GetStopIndexAsync(endTick, minIndex: searchStartIndex, maxIndex: searchStopIndex);
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
        public List<DensityInfo> GetDensityInfo(long startTick, long stopTick, long groupInterval)
        {
            if (_globalEventsCount == 0)
                return new List<DensityInfo>(0);

            if (stopTick < startTick)
                return new List<DensityInfo>(0);

            startTick = Math.Max(startTick, _globalStartTick);
            stopTick = Math.Min(stopTick, _globalStopTick);

            return GetDensityInfo(startTick, stopTick, 0, _globalEventsCount - 1, groupInterval);
        }

        private List<DensityInfo> SplitSingleDensityInfo(DensityInfo info, long groupInterval)
        {
            if (info.StopIndex == info.StartIndex + 1)
                return new List<DensityInfo>{info};
            if (_globalEventsCount == 0)
                return new List<DensityInfo>(0);

            var startTick = Math.Max(info.Start, _globalStartTick);
            var stopTick = Math.Min(info.Stop, _globalStopTick);

            return GetDensityInfo(startTick, stopTick, info.StartIndex, info.StopIndex, groupInterval);
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

        public List<DensityInfo> GetInfoForRightSide(long lastStopTick, long lastStopIndex, long stopTick, long groupInterval)
        {
            if (_globalEventsCount == 0)
                return new List<DensityInfo>(0);

            if (lastStopTick >= stopTick)
                return new List<DensityInfo>(0);

            if (lastStopIndex == _globalEventsCount -1)
                return new List<DensityInfo>();

            var startTick = Math.Max(lastStopTick + 1, _globalStartTick);
            stopTick = Math.Min(stopTick, _globalStopTick);

            return GetDensityInfo(startTick, stopTick, lastStopIndex + 1, _globalEventsCount - 1, groupInterval);
        }

        public List<DensityInfo> GetInfoForLeftSide(long firstStartTick, long firstStartIndex, long startTick, long groupInterval)
        {
            if (_globalEventsCount == 0)
                return new List<DensityInfo>(0);

            if (firstStartTick <= startTick)
                return new List<DensityInfo>(0);

            if (firstStartIndex == 0)
                return new List<DensityInfo>();

            startTick = Math.Max(startTick, _globalStartTick);
            var stopTick = Math.Min(firstStartTick - 1, _globalStopTick);

            return GetDensityInfo(startTick, stopTick, 0, firstStartIndex - 1, groupInterval);
        }

    }
}
