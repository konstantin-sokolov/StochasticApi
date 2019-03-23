using System.IO;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Loggers;
using EventApi.Implementation.Api;
using EventApi.Implementation.DataProviders;
using EventsApi.Contracts;
using Generators;
using Moq;
using StochasticUi.ViewModel;

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

        private long _globalStart;
        private long _globalStop;
        

        public DensitiesCalculationPerfTest()
        {
            var mockLogger = new Mock<NLog.ILogger>();
            _logger = mockLogger.Object;
            var factory = new GeneratorFactory();
            var generator = factory.GetGenerator(ProviderType.MemoryMappedFile);
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "MmfGeneratedFiles", "LargeData.bin");
            _provider = generator.GenerateDataProviderAsync(100L * 1000L * 1000L, new object[] { filePath }).Result;
        }
        [IterationSetup]
        public void Setup()
        {
            _densityApi = new DensityApi(_logger, _provider);
            _globalStart = _provider.GetGlobalStartTick();
            _globalStop = _provider.GetGlobalStopTick();
        }

        [Benchmark]
        public void GetDensityInfo()
        {
            _densityApi.GetDensityInfo(_globalStart, _globalStop, (_globalStop - _globalStart) / RequestedSize);
        }
    }
}
