using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EventApi.Implementation.DataProviders;
using EventApi.Models;
using EventsApi.Contracts;
using NLog;

namespace EventApi.Implementation.Api
{
    public class DensityApi : BaseEventApi, IDensityApi
    {
        private readonly ILogger _logger;

        public DensityApi(ILogger logger, IDataProvider dataProvider) : base(dataProvider)
        {
            _logger = logger;
        }

        private Task<List<DensityInfo>> GetDensityInfoAsync(long startTick, long stopTick, long searchStartIndex, long searchStopIndex, long groupInterval, CancellationToken ctn = default(CancellationToken))
        {
            return Task.Run(() =>
            {
                var result = new List<DensityInfo>();
                var stopWatch = new Stopwatch();
                stopWatch.Start();
                try
                {
                    do
                    {
                        ctn.ThrowIfCancellationRequested();

                        var endTick = Math.Min(startTick + groupInterval, stopTick);

                        var searchStartIndexTask = GetStartIndexAsync(startTick, minIndex: searchStartIndex, maxIndex: searchStopIndex, ctn: ctn);
                        var searchStopIndexTask = GetStopIndexAsync(endTick, minIndex: searchStartIndex, maxIndex: searchStopIndex, ctn: ctn);

                        Task.WaitAll(searchStartIndexTask, searchStopIndexTask);

                        ctn.ThrowIfCancellationRequested();

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
                catch (Exception e)
                {
                    _logger.Error(e);
                    throw;
                }
            }, ctn);
        }

        private async Task<List<DensityInfo>> SplitSingleDensityInfo(DensityInfo info, long groupInterval, CancellationToken ctn = default(CancellationToken))
        {
            if (info.StopIndex == info.StartIndex + 1)
                return new List<DensityInfo> {info};
            if (_globalEventsCount == 0)
                return new List<DensityInfo>(0);

            var startTick = Math.Max(info.Start, _globalStartTick);
            var stopTick = Math.Min(info.Stop, _globalStopTick);

            return await GetDensityInfoAsync(startTick, stopTick, info.StartIndex, info.StopIndex, groupInterval, ctn);
        }

        public async Task<List<DensityInfo>> GetDensityInfoAsync(long startTick, long stopTick, long groupInterval, CancellationToken ctn = default(CancellationToken))
        {
            if (_globalEventsCount == 0)
                return new List<DensityInfo>(0);

            if (stopTick < startTick)
                return new List<DensityInfo>(0);

            startTick = Math.Max(startTick, _globalStartTick);
            stopTick = Math.Min(stopTick, _globalStopTick);

            return await GetDensityInfoAsync(startTick, stopTick, 0, _globalEventsCount - 1, groupInterval, ctn);
        }


        #region only for performance test 

        // it will be the new version api - with better performance

        public Task<List<DensityInfo>> SplitDensityInfoAsync(List<DensityInfo> currentInfo, long start, long stop, long groupInterval, CancellationToken ctn = default(CancellationToken))
        {
            return Task.Run(() =>
            {
                if (currentInfo == null || !currentInfo.Any())
                    return new List<DensityInfo>(0);

                var visibleInfo = currentInfo.Where(t => t.Stop >= start && t.Start <= stop).ToArray();
                if (!visibleInfo.Any())
                    return new List<DensityInfo>(0);

                var taskResults = new List<DensityInfo>[visibleInfo.Length];
                var options = new ParallelOptions {CancellationToken = ctn};

                try
                {
                    Parallel.For(0, visibleInfo.Length, options, async (index) => taskResults[index] = await SplitSingleDensityInfo(visibleInfo[index], groupInterval, ctn));
                }
                catch (OperationCanceledException ex)
                {
                    _logger.Info("DensityApi: SplitDensityInfo was canceled");
                }
                catch (AggregateException ex)
                {
                    var wasCriticalError = false;
                    foreach (var innerException in ex.InnerExceptions)
                    {
                        if (innerException is OperationCanceledException)
                            continue;
                        _logger.Error($"Error while split densityInfo:{ex.Message} - {ex.StackTrace}");
                        wasCriticalError = true;
                    }

                    if (wasCriticalError)
                        throw;
                }

                ctn.ThrowIfCancellationRequested();

                var result = new List<DensityInfo>();
                foreach (var splitDensities in taskResults)
                {
                    foreach (var densityInfos in splitDensities)
                    {
                        if (densityInfos.Stop < start)
                            continue;
                        if (densityInfos.Start > stop)
                            continue;
                        result.Add(densityInfos);
                    }
                }

                return result;
            }, ctn);
        }

        public async Task<List<DensityInfo>> GetInfoForRightSideAsync(long lastStopTick, long lastStopIndex, long stopTick, long groupInterval, CancellationToken ctn = default(CancellationToken))
        {
            if (_globalEventsCount == 0)
                return new List<DensityInfo>(0);

            if (lastStopTick >= stopTick)
                return new List<DensityInfo>(0);

            if (lastStopIndex == _globalEventsCount - 1)
                return new List<DensityInfo>(0);

            var startTick = Math.Max(lastStopTick + 1, _globalStartTick);
            stopTick = Math.Min(stopTick, _globalStopTick);

            return await GetDensityInfoAsync(startTick, stopTick, lastStopIndex + 1, _globalEventsCount - 1, groupInterval, ctn);
        }

        public async Task<List<DensityInfo>> GetInfoForLeftSideAsync(long firstStartTick, long firstStartIndex, long startTick, long groupInterval, CancellationToken ctn = default(CancellationToken))
        {
            if (_globalEventsCount == 0)
                return new List<DensityInfo>(0);

            if (firstStartTick <= startTick)
                return new List<DensityInfo>(0);

            if (firstStartIndex == 0)
                return new List<DensityInfo>();

            startTick = Math.Max(startTick, _globalStartTick);
            var stopTick = Math.Min(firstStartTick - 1, _globalStopTick);

            return await GetDensityInfoAsync(startTick, stopTick, 0, firstStartIndex - 1, groupInterval, ctn);
        }

        #endregion
    }
}
