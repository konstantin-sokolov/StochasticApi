using System;
using System.Collections.Generic;
using EventApi.Models;

namespace EventsApi.Contracts
{
    public interface IDensityApi:IDisposable
    {
         List<DensityInfo> GetDensityInfo(long start, long stop, long groupInterval);
         List<DensityInfo> SplitDensityInfo(List<DensityInfo> currentInfo, long start, long stop, long groupInterval);
    }
}
