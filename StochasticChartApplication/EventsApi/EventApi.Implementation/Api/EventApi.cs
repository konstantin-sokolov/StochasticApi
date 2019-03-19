using System.Collections.Generic;
using System.Diagnostics;
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

        public IEnumerable<PayloadEvent> GetEvents(long startTick, long stopTick)
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
            _logger.Info($"EventApi: Got {stopIndex - startIndex} events in {stopWatch.ElapsedMilliseconds}ms");
            return result;
        }
    }
}
