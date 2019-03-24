using System;
using System.Collections.Generic;
using System.Threading;
using EventApi.Models;

namespace EventsApi.Contracts
{
    public interface IEventApi:IDisposable
    {
        IEnumerable<PayloadEvent> GetEvents(long startTick, long stopTick, CancellationToken token);
    }
}
