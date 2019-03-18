using EventApi.Models;

namespace EventsApi.Contracts
{
    public interface IEventApi
    {
        PayloadEvent[] GetEvents(long startTick, long stopTick);
       // void LoadData(PayloadEvent[] events);
    }
}
