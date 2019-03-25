using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using EventApi.Implementation.Api;
using EventApi.Implementation.DataProviders;
using EventApi.Models;
using EventsApi.Contracts;
using Generators;
using Moq;

namespace PerformanceTests
{
    [SimpleJob(RunStrategy.ColdStart, 5)]
    [RPlotExporter, RankColumn]
    public class DensitiesCalculationPerfTest
    {
        private IDensityApi _densityApi;
        private IDataProvider _provider;
        private NLog.ILogger _logger;
        private const int RequestedSize = 800;
        private List<DensityInfo> _currentInfo;
        private long _requestedStart;
        private long _requestedStop;
        private long _currentGroupInterval;

        public DensitiesCalculationPerfTest()
        {
            var mockLogger = new Mock<NLog.ILogger>();
            _logger = mockLogger.Object;

            //you can use once generated file but till you hasn't it you should generate it everyTime
            //var entitySize = Marshal.SizeOf(typeof(PayloadEvent));
            //_provider = new MMFDataProvider(@"C:\Repos\StochasticChartApplication\Tests\PerformanceTests\bin\Release\MmfGeneratedFiles\LargeData.bin", entitySize);

            var factory = new GeneratorFactory(_logger);
            var generator = factory.GetGenerator(ProviderType.MemoryMappedFile);
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "MmfGeneratedFiles", "LargeData.bin");
            _provider = generator.GenerateDataProviderAsync(100L * 1000L * 1000L, new object[] { filePath }).Result;
        }

        [IterationSetup]
        public async Task Setup()
        {
            _densityApi = new DensityApi(_logger, _provider);
            var globalStart = _provider.GetGlobalStartTick();
            var globalStop = _provider.GetGlobalStopTick();

            var newLength = (long) ((globalStop - globalStart) * 0.6);
            var middle = (globalStop - globalStart) / 2;

            _requestedStart = middle - newLength / 4;
            _requestedStop = middle + newLength / 4;
            _currentGroupInterval = newLength / RequestedSize;
            _currentInfo = await _densityApi.GetDensityInfoAsync(middle - newLength / 2, middle + newLength / 2, _currentGroupInterval,CancellationToken.None);
        }

        [Benchmark]
        public async Task GetDensityInfo()
        {
            await _densityApi.GetDensityInfoAsync(_requestedStart, _requestedStop, (_requestedStop - _requestedStart) / RequestedSize, CancellationToken.None);
        }

        [Benchmark]
        public async Task SplitDensityInfo()
        {
            await _densityApi.SplitDensityInfoAsync(_currentInfo, _requestedStart, _requestedStop, (_requestedStop - _requestedStart) / RequestedSize, CancellationToken.None);
        }
        [Benchmark]
        public async Task GetLeftAndRightInfo()
        {
            var firstEvent = _currentInfo[0];
            var lastEvent = _currentInfo[_currentInfo.Count - 1];
            var step = (lastEvent.Stop - firstEvent.Start) / 20;
            var rightInfo = await _densityApi.GetInfoForRightSideAsync(lastEvent.Stop, lastEvent.StopIndex, lastEvent.Stop + step, _currentGroupInterval, CancellationToken.None);
            Trace.WriteLine(rightInfo.Count);
            var leftInfo = await _densityApi.GetInfoForLeftSideAsync(firstEvent.Start,firstEvent.StartIndex, firstEvent.Start - step, _currentGroupInterval, CancellationToken.None);
            Trace.WriteLine(leftInfo.Count);
        }
    }
}
