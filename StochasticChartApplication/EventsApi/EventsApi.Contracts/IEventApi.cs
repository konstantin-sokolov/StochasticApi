using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EventApi.Models;

namespace EventsApi.Contracts
{
    public interface IEventApi:IDisposable
    {
        Task<IEnumerable<PayloadEvent>> GetEventsAsync(long startTick, long stopTick, CancellationToken token);
    }
}
