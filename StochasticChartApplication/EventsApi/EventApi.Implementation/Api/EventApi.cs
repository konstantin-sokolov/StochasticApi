using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using EventApi.Implementation.DataProviders;
using EventApi.Models;
using EventsApi.Contracts;
using NLog;

namespace EventApi.Implementation.Api
{
    public class EventApi : BaseEventApi, IEventApi
    {
        private readonly ILogger _logger;

        public EventApi(ILogger logger, IDataProvider dataProvider) : base(dataProvider)
        {
            _logger = logger;
        }

        public Task<IEnumerable<PayloadEvent>> GetEventsAsync(long startTick, long stopTick, CancellationToken ctn = default(CancellationToken))
        {
            return Task.Run(() =>
            {
                _logger.Info($"EventApi: Requested events between: {startTick}-{stopTick}");
                try
                {
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
                    var searchStartIndexTask = GetStartIndexAsync(startTick, ctn: ctn);
                    var searchStopIndexTask = GetStopIndexAsync(stopTick, ctn: ctn);
                    Task.WaitAll(searchStartIndexTask, searchStopIndexTask);

                    ctn.ThrowIfCancellationRequested();

                    var startIndex = searchStartIndexTask.Result;
                    var stopIndex = searchStopIndexTask.Result;

                    var result = _dataProvider.GetEventsBetween(startIndex, stopIndex);
                    stopWatch.Stop();
                    _logger.Info($"EventApi: Got {stopIndex - startIndex} events in {stopWatch.ElapsedMilliseconds}ms");
                    return result;
                }
                catch (Exception e)
                {
                    _logger.Error(e);
                    throw;
                }
            }, ctn);
        }
    }
}
