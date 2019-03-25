using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EventApi.Models;

namespace EventsApi.Contracts
{
    public interface IDensityApi : IDisposable
    {
        Task<List<DensityInfo>> GetDensityInfoAsync(long start, long stop, long groupInterval, CancellationToken ctnToken);

        #region onlyForTest

        //it will be new methods for api, that will increase performance
        Task<List<DensityInfo>> SplitDensityInfoAsync(List<DensityInfo> currentInfo, long start, long stop, long groupInterval, CancellationToken ctnToken);
        Task<List<DensityInfo>> GetInfoForRightSideAsync(long lastStopTick, long lastStopIndex, long stopTick, long groupInterval, CancellationToken ctnToken);
        Task<List<DensityInfo>> GetInfoForLeftSideAsync(long firstStartTick, long firstStartIndex, long startTick, long groupInterval, CancellationToken ctnToken);

        #endregion

    }
}
