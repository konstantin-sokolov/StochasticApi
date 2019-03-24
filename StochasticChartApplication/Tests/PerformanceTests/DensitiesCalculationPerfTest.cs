using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
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
        private long _currentGroupInterval;

        public DensitiesCalculationPerfTest()
        {
            var mockLogger = new Mock<NLog.ILogger>();
            _logger = mockLogger.Object;
            var entitySize = Marshal.SizeOf(typeof(PayloadEvent));
            _provider = new MMFDataProvider(@"C:\Repos\StochasticChartApplication\Tests\PerformanceTests\bin\Release\MmfGeneratedFiles\LargeData.bin", entitySize);
        }

        [IterationSetup]
        public void Setup()
        {
            _densityApi = new DensityApi(_logger, _provider);
            var globalStart = _provider.GetGlobalStartTick();
            var globalStop = _provider.GetGlobalStopTick();

            var newLength = (long) ((globalStop - globalStart) * 0.6);
            var middle = (globalStop - globalStart) / 2;

            _requestedStart = middle - newLength / 4;
            _requestedStop = middle + newLength / 4;
            _currentGroupInterval = newLength / RequestedSize;
            _currentInfo = _densityApi.GetDensityInfo(middle - newLength / 2, middle + newLength / 2, _currentGroupInterval);
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
        [Benchmark]
        public void GetLeftAndRightInfo()
        {
            
            var firstEvent = _currentInfo[0];
            var lastEvent = _currentInfo[_currentInfo.Count - 1];
            var step = (lastEvent.Stop - firstEvent.Start) / 20;
            var rightinfo = _densityApi.GetInfoForRightSide(lastEvent.Stop, lastEvent.StopIndex, lastEvent.Stop + step, _currentGroupInterval);
            Trace.WriteLine(rightinfo.Count);
            var leftInfo = _densityApi.GetInfoForLeftSide(firstEvent.Start,firstEvent.StartIndex, firstEvent.Start - step, _currentGroupInterval);
            Trace.WriteLine(leftInfo.Count);
        }
    }
}
