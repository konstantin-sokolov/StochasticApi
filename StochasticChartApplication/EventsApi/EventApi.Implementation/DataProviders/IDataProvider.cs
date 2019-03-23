using System;
using System.Collections.Generic;
using EventApi.Models;

namespace EventApi.Implementation.DataProviders
{
    public interface IDataProvider:IDisposable
    {
        PayloadEvent GetEventAtIndex(long index);
        IEnumerable<PayloadEvent> GetEventsBetween(long startIndex, long stopIndex);
        long GetGlobalEventsCount();
        long GetGlobalStartTick();
        long GetGlobalStopTick();
    }
}
