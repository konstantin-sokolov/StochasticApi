using System;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using EventApi.Models;

namespace PerformanceTests
{
    [SimpleJob(RunStrategy.ColdStart, launchCount: 1000000)]
    [RPlotExporter, RankColumn]
    public class MmfReadPerfTest
    {
        private MemoryMappedFile _mmf;
        private string _filePath;
        private const int COLLECTION_SIZE = 1000 * 1000;
        public MmfReadPerfTest()
        {
            _filePath = Path.Combine(Directory.GetCurrentDirectory(), "MmfGeneratedFiles", "ReadLargeFile.bin");
            Write(_filePath);
        }

        [GlobalSetup]
        public void Setup()
        {
            _mmf = MemoryMappedFile.CreateFromFile(_filePath, FileMode.Open, "fileHandle");
        }

        [GlobalCleanup]
        public void CleanUp()
        {
            if (File.Exists(_filePath))
                File.Delete(_filePath);
        }

        [Benchmark]
        public void ReadDataFromMmf()
        {
            Read();
        }


        static void Write(string filePath)
        {
            long batchSize = 1000L;
            long collectionSize = COLLECTION_SIZE;
            var stSize = Marshal.SizeOf(typeof(PayloadEvent));
            var fileSize = stSize * collectionSize;
            int currentPercent = 0;
            var batchCount = collectionSize / batchSize;

            using (var mmFile = MemoryMappedFile.CreateFromFile(
                filePath,
                FileMode.Create, "fileHandle", fileSize))
            {
                for (int b = 0; b < batchCount; b++)
                {
                    using (var myAccessor = mmFile.CreateViewAccessor(b * batchSize * stSize, batchSize * stSize))
                    {
                        for (long i = 0; i < batchSize; i++)
                        {
                            var newEl = new PayloadEvent(EventType.start, 100);
                            myAccessor.Write(i * stSize, ref newEl);
                            var tempPercent = (int)(100 * (i + b * batchSize) / collectionSize);
                            if (currentPercent != tempPercent)
                            {
                                currentPercent = tempPercent;
                                Console.WriteLine($"Percents:{currentPercent}");
                            }
                        }
                    }
                }

            }

        }

        public void Read()
        {
            var random = new Random(5);
            var stSize = Marshal.SizeOf(typeof(PayloadEvent));
            int collectionSize = COLLECTION_SIZE;
            var index = random.Next(collectionSize);
            long readOffset = (long) index * stSize;
            using (var accessor = _mmf.CreateViewAccessor(readOffset, stSize))
            {
                accessor.Read(0, out PayloadEvent _);
            }
        }
    }
}
