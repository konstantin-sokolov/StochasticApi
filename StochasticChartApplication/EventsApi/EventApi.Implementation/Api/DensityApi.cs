using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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

        private List<DensityInfo> GetDensityInfo(long startTick, long stopTick, long searchStartIndex, long searchStopIndex, long groupInterval, CancellationToken ctn = default(CancellationToken))
        {
            var result = new List<DensityInfo>();
            var stopWatch = new Stopwatch();
            stopWatch.Start();

            do
            {
                if (ctn.IsCancellationRequested)
                    return null;

                var endTick = Math.Min(startTick + groupInterval, stopTick);

                var searchStartIndexTask = GetStartIndexAsync(startTick, minIndex: searchStartIndex, maxIndex: searchStopIndex, ctn: ctn);
                var searchStopIndexTask = GetStopIndexAsync(endTick, minIndex: searchStartIndex, maxIndex: searchStopIndex, ctn: ctn);
                try
                {
                    Task.WaitAll(searchStartIndexTask, searchStopIndexTask);
                }
                catch (AggregateException e)
                {
                    foreach (var innerException in e.InnerExceptions)
                    {
                        if (innerException is OperationCanceledException)
                            continue;
                        _logger.Error(innerException.Message, innerException.StackTrace);
                    }
                }


                if (ctn.IsCancellationRequested)
                    return null;

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

        #region split

        private List<DensityInfo> SplitSingleDensityInfo(DensityInfo info, long groupInterval, CancellationToken ctn = default(CancellationToken))
        {
            if (info.StopIndex == info.StartIndex + 1)
                return new List<DensityInfo> {info};
            if (_globalEventsCount == 0)
                return new List<DensityInfo>(0);

            var startTick = Math.Max(info.Start, _globalStartTick);
            var stopTick = Math.Min(info.Stop, _globalStopTick);

            return GetDensityInfo(startTick, stopTick, info.StartIndex, info.StopIndex, groupInterval, ctn);
        }

        private List<DensityInfo> SplitDensityInfo(IEnumerable<DensityInfo> currentInfo, long start, long stop, long groupInterval, CancellationToken ctn = default(CancellationToken))
        {
            var splitTasks = new List<Task<List<DensityInfo>>>();
            foreach (var densityInfo in currentInfo)
            {
                if (densityInfo.Stop < start)
                    continue;
                if (densityInfo.Start > stop)
                    continue;

                splitTasks.Add(Task.Run(() => SplitSingleDensityInfo(densityInfo, groupInterval, ctn), ctn));
            }

            try
            {
                Task.WaitAll(splitTasks.ToArray());
            }
            catch (AggregateException e)
            {
                foreach (var innerException in e.InnerExceptions)
                {
                    if (innerException is OperationCanceledException)
                        continue;
                    _logger.Error(innerException.Message, innerException.StackTrace);
                }
            }

            if (ctn.IsCancellationRequested)
                return null;

            var result = new List<DensityInfo>();
            foreach (var task in splitTasks)
            {
                var splitedDensities = task.Result;
                foreach (var densityInfo in splitedDensities)
                {
                    if (densityInfo.Stop < start)
                        continue;
                    if (densityInfo.Start > stop)
                        continue;
                    result.Add(densityInfo);
                }
            }

            return result;
        }

        #endregion split

        private List<DensityInfo> GetInfoForRightSide(long lastStopTick, long lastStopIndex, long stopTick, long groupInterval, CancellationToken ctn = default(CancellationToken))
        {
            if (_globalEventsCount == 0)
                return new List<DensityInfo>(0);

            if (lastStopTick >= stopTick)
                return new List<DensityInfo>(0);

            if (lastStopIndex == _globalEventsCount - 1)
                return new List<DensityInfo>();

            var startTick = Math.Max(lastStopTick + 1, _globalStartTick);
            stopTick = Math.Min(stopTick, _globalStopTick);

            return GetDensityInfo(startTick, stopTick, lastStopIndex + 1, _globalEventsCount - 1, groupInterval, ctn);
        }

        private List<DensityInfo> GetInfoForLeftSide(long firstStartTick, long firstStartIndex, long startTick, long groupInterval, CancellationToken ctn = default(CancellationToken))
        {
            if (_globalEventsCount == 0)
                return new List<DensityInfo>(0);

            if (firstStartTick <= startTick)
                return new List<DensityInfo>(0);

            if (firstStartIndex == 0)
                return new List<DensityInfo>();

            startTick = Math.Max(startTick, _globalStartTick);
            var stopTick = Math.Min(firstStartTick - 1, _globalStopTick);

            return GetDensityInfo(startTick, stopTick, 0, firstStartIndex - 1, groupInterval, ctn);
        }

        private List<DensityInfo> UnionDensity(List<DensityInfo> visibleInfos, long groupInterval, CancellationToken ctn)
        {
            if (visibleInfos.Count == 0)
                return visibleInfos;

            var result = new List<DensityInfo>();
            DensityInfo currentDensityInfo = visibleInfos[0].Clone();
            for (int i = 1; i < visibleInfos.Count; i++)
            {
                if (currentDensityInfo.Start + groupInterval < visibleInfos[i].Start)
                {
                    result.Add(currentDensityInfo);
                    currentDensityInfo = visibleInfos[i].Clone();
                }
                else
                {
                    currentDensityInfo.StopIndex = visibleInfos[i].StopIndex;
                    currentDensityInfo.Stop = visibleInfos[i].Stop;
                    currentDensityInfo.EventsCount += visibleInfos[i].EventsCount;
                }
            }

            result.Add(currentDensityInfo);

            return result;
        }


        public List<DensityInfo> GetDensityInfo(long startTick, long stopTick, long groupInterval, CancellationToken ctn = default(CancellationToken))
        {
            if (_globalEventsCount == 0)
                return new List<DensityInfo>(0);

            if (stopTick < startTick)
                return new List<DensityInfo>(0);

            startTick = Math.Max(startTick, _globalStartTick);
            stopTick = Math.Min(stopTick, _globalStopTick);

            return GetDensityInfo(startTick, stopTick, 0, _globalEventsCount - 1, groupInterval, ctn);
        }
       
        #region chartApi

        public List<DensityInfo> ScaleInto(List<DensityInfo> visibleInfos, long start, long stop, long groupInterval, CancellationToken ctn = default(CancellationToken))
        {
            if (visibleInfos.Count == 0)
                return visibleInfos;
            return SplitDensityInfo(visibleInfos, start, stop, groupInterval, ctn);
        }

        public List<DensityInfo> ScaleOut(List<DensityInfo> visibleInfos, long start, long stop, long groupInterval, CancellationToken ctn = default(CancellationToken))
        {
            if (visibleInfos.Count == 0)
                return GetDensityInfo(start, stop, groupInterval, ctn);

            var leftTask = Task.Run(() =>
            {
                var lastEvent = visibleInfos[visibleInfos.Count - 1];
                return GetInfoForRightSide(lastEvent.Stop, lastEvent.StopIndex, stop, groupInterval, ctn);
            }, ctn);
            var rightTask = Task.Run(() =>
            {
                var firstEvent = visibleInfos[0];
                return GetInfoForLeftSide(firstEvent.Start, firstEvent.StartIndex, start, groupInterval, ctn);
            }, ctn);

            var middleTask = Task.Run(() => UnionDensity(visibleInfos, groupInterval, ctn), ctn);

            try
            {
                Task.WaitAll(leftTask,middleTask,rightTask);
            }
            catch (AggregateException e)
            {
                foreach (var innerException in e.InnerExceptions)
                {
                    if (innerException is OperationCanceledException)
                        continue;
                    _logger.Error(innerException.Message, innerException.StackTrace);
                }
            }

            if (ctn.IsCancellationRequested)
                return null;

            var result = new List<DensityInfo>(rightTask.Result.Count + leftTask.Result.Count + middleTask.Result.Count);
            result.AddRange(leftTask.Result);
            result.AddRange(middleTask.Result);
            result.AddRange(rightTask.Result);
            return result;
        }

        public List<DensityInfo> MoveToRight(List<DensityInfo> visibleInfos, long start, long stop, long groupInterval, CancellationToken ctn = default(CancellationToken))
        {
            if (visibleInfos.Count == 0)
                return GetDensityInfo(start, stop, groupInterval, ctn);

            var lastEvent = visibleInfos[visibleInfos.Count - 1];
            var appendFromRight = GetInfoForRightSide(lastEvent.Stop, lastEvent.StopIndex, stop, groupInterval, ctn);
            var result = new List<DensityInfo>(visibleInfos.Count + appendFromRight.Count);
            result.AddRange(visibleInfos);
            result.AddRange(appendFromRight);
            return result;
        }

        public List<DensityInfo> MoveToLeft(List<DensityInfo> visibleInfos, long start, long stop, long groupInterval, CancellationToken ctn = default(CancellationToken))
        {
            if (visibleInfos.Count == 0)
                return GetDensityInfo(start, stop, groupInterval, ctn);

            var firstEvent = visibleInfos[0];
            var appendFromLeft = GetInfoForLeftSide(firstEvent.Start, firstEvent.StartIndex, start, groupInterval, ctn);
            var result = new List<DensityInfo>(visibleInfos.Count + appendFromLeft.Count);
            result.AddRange(appendFromLeft);
            result.AddRange(visibleInfos);
            return result;
        }

        #endregion chartApi
    }
}