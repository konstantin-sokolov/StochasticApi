using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using EventApi.Models;
using StochasticUi.ViewModel.Renders;

namespace PerformanceTests
{
    [ClrJob(baseline: true)]
    [RPlotExporter, RankColumn]
    public class ImageRenderPerfTest
    {
        private DensityInfo[] data;
        private long _currentStart;
        private long _currentLength;
        private readonly Random _random = new Random(42);

        [Params(1000, 2000)] public int N;

        [GlobalSetup]
        public void Setup()
        {
            data = Enumerable.Range(1, N).Select(t => new DensityInfo
            {
                EventsCount = _random.Next(125),
                Start = t * 10000 + _random.Next(1000),
                Stop = (t + 1) * 10000 - _random.Next(5000)
            }).ToArray();
            _currentStart = data[0].Start;
            _currentLength = data[data.Length - 1].Stop - _currentStart;
        }

        [Benchmark]
        public async Task RenderData() => await ChartRender.RenderDataAsync(data, _currentStart, _currentLength, CancellationToken.None);
    }
}
