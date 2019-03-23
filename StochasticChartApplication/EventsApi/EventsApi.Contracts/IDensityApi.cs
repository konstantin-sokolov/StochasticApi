using System;
using System.Collections.Generic;
using EventApi.Models;

namespace EventsApi.Contracts
{
    public interface IDensityApi:IDisposable
    {
         IEnumerable<DensityInfo> GetDensityInfo(long start, long stop, long groupInterval);
    }
}
