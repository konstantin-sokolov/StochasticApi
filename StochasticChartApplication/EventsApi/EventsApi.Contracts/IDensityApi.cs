using System;
using System.Collections.Generic;
using System.Threading;
using EventApi.Models;

namespace EventsApi.Contracts
{
    public interface IDensityApi : IDisposable
    {
        List<DensityInfo> GetDensityInfo(long start, long stop, long groupInterval, CancellationToken ctnToken);

        List<DensityInfo> ScaleInto(List<DensityInfo> visibleInfos, long start, long stop, long groupInterval, CancellationToken ctn = default(CancellationToken));
        List<DensityInfo> ScaleOut(List<DensityInfo> visibleInfos, long start, long stop, long groupInterval, CancellationToken ctn = default(CancellationToken));
        List<DensityInfo> MoveToRight(List<DensityInfo> visibleInfos, long start, long stop, long groupInterval, CancellationToken ctn = default(CancellationToken));
        List<DensityInfo> MoveToLeft(List<DensityInfo> visibleInfos, long start, long stop, long groupInterval, CancellationToken ctn = default(CancellationToken));
    }
}
