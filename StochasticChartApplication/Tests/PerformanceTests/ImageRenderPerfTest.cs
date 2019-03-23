using System;
using System.Linq;
using BenchmarkDotNet.Attributes;
using StochasticUi.ViewModel;
using StochasticUi.ViewModel.Renders;

namespace PerformanceTests
{
    [ClrJob(baseline: true)]
    [RPlotExporter, RankColumn]
    public class ImageRenderPerfTest
    {
        private double[] data;
        private readonly Random _random = new Random(42);
        [Params(1000, 2000)]
        public int N;

        [GlobalSetup]
        public void Setup()
        {
            data = Enumerable.Range(1, N).Select(t => _random.NextDouble()).ToArray();
        }

        [Benchmark]
        public void RenderData() => StatisticRender.RenderData(data);
    }
}
