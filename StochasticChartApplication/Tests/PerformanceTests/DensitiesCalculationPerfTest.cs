using System.Collections.Generic;
using System.IO;
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
        private const int RequestedSize = 400;
        private List<DensityInfo> _currentInfo;
        private long _requestedStart;
        private long _requestedStop;

        public DensitiesCalculationPerfTest()
        {
            var mockLogger = new Mock<NLog.ILogger>();
            _logger = mockLogger.Object;
            var factory = new GeneratorFactory();
            var generator = factory.GetGenerator(ProviderType.MemoryMappedFile);
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "MmfGeneratedFiles", "LargeData.bin");
            _provider = generator.GenerateDataProviderAsync(100L * 1000L * 1000L, new object[] {filePath}).Result;
        }

        [IterationSetup]
        public void Setup()
        {
            _densityApi = new DensityApi(_logger, _provider);
            var globalStart = _provider.GetGlobalStartTick();
            var globalStop = _provider.GetGlobalStopTick();
            var newLength = (long) ((globalStop - globalStart) * 0.6);
            var middle = (globalStop - globalStart) / 2;

            _requestedStart = middle - newLength / 2;
            _requestedStop = middle + newLength / 2;
            _currentInfo = _densityApi.GetDensityInfo(globalStart, globalStop, (globalStop - globalStart) / RequestedSize);
        }

        [Benchmark]
        public void GetDensityInfo()
        {
            _densityApi.GetDensityInfo(_requestedStart, _requestedStop, (_requestedStop - _requestedStart) / RequestedSize);
        }

        [Benchmark]
        public void SplitDensityInfo()
        {
            _densityApi.SplitDensityInfo(_currentInfo, _requestedStart, _requestedStop, (_requestedStop - _requestedStart) / RequestedSize);
        }
    }
}
