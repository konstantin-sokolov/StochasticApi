using System.Collections.Generic;
using EventApi.Models;

namespace EventApi.Implementation.DataProviders
{
    public interface IDataProvider
    {
        PayloadEvent GetEventAtIndex(long index);
        PayloadEvent[] GetEventsBetween(long startIndex, long stopIndex);
        long GetGlobalEventsCount();
        long GetGlobalStartTick();
        long GetGlobalStopTick();
        void Init();
    }
}
