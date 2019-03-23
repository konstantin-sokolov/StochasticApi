using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using EventApi.Implementation.DataProviders;
using Generators;
using StochasticUi.ViewModel;

namespace PerformanceTests
{
    [SimpleJob(RunStrategy.ColdStart, launchCount: 30)]
    [RPlotExporter, RankColumn]
    public class ViewModelPerfTest
    {
        //private readonly List<string> _generatedFiles = new List<string>();
        //private IEventGenerator _generator;

        //[Params(1000L * 1000)]
        //public int N;

        //[GlobalSetup]
        //public void Setup()
        //{
        //    GeneratorFactory factory = new GeneratorFactory();
        //    _generator = factory.GetGenerator(ProviderType.MemoryMappedFile);
        //}

        //[GlobalCleanup]
        //public void CleanUp()
        //{
        //    foreach (var file in _generatedFiles)
        //    {
        //        if (File.Exists(file))
        //            File.Delete(file);
        //    }
        //}

        //[Benchmark]
        //public void GenerateDataSet()
        //{
        //    var fileName = Path.Combine(Directory.GetCurrentDirectory(), "MmfGeneratedFiles", $"TestGenerator{_generatedFiles.EventsCount}.bin");
        //    var dataProvider = _generator.GenerateDataProviderAsync(N, new object[] { fileName }).Result;
        //    dataProvider.Dispose();
        //    _generatedFiles.Add(fileName);
        //}
    }
}
