using System;
using System.Collections.Generic;
using System.Threading;
using EventApi.Models;

namespace EventsApi.Contracts
{
    public interface IDensityApi : IDisposable
    {
        List<DensityInfo> GetDensityInfo(long start, long stop, long groupInterval, CancellationToken ctnToken);

        #region onlyForTest

        //it will be new methods for api, that will increase performance
        List<DensityInfo> SplitDensityInfo(List<DensityInfo> currentInfo, long start, long stop, long groupInterval, CancellationToken ctnToken);
        List<DensityInfo> GetInfoForRightSide(long lastStopTick, long lastStopIndex, long stopTick, long groupInterval, CancellationToken ctnToken);
        List<DensityInfo> GetInfoForLeftSide(long firstStartTick, long firstStartIndex, long startTick, long groupInterval, CancellationToken ctnToken);

        #endregion

    }
}
