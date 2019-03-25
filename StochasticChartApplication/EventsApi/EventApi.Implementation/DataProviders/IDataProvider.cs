using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EventApi.Models;

namespace EventApi.Implementation.DataProviders
{
    public interface IDataProvider:IDisposable
    {
        Task<PayloadEvent> GetEventAtIndexAsync(long index);
        Task<IEnumerable<PayloadEvent>> GetEventsBetweenAsync(long startIndex, long stopIndex);
        long GetGlobalEventsCount();
        long GetGlobalStartTick();
        long GetGlobalStopTick();
    }
}
