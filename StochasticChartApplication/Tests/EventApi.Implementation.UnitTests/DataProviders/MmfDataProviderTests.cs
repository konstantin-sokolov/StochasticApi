using System;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using AutoFixture;
using EventApi.Implementation.DataProviders;
using EventApi.Models;
using FluentAssertions;
using NUnit.Framework;

namespace EventApi.Implementation.UnitTests.DataProviders
{
    [TestFixture]
    [Parallelizable(ParallelScope.All)]
    class MmfDataProviderTests
    {
        readonly IFixture _fixture = new Fixture();
        private string _generatedFile;
        private PayloadEvent[] _expectedValues;
        private IDataProvider _dataProvider;
        private void WriteDataToFile(string filePath, int entitySize, PayloadEvent[] array)
        {
            var fileSize = entitySize * array.Length;
            using (var fs = new FileStream(filePath, FileMode.CreateNew))
            using (var mmf = MemoryMappedFile.CreateFromFile(fs, "Events", fileSize, MemoryMappedFileAccess.ReadWrite, HandleInheritability.None, false))
            {
                using (var accessor = mmf.CreateViewAccessor(0, array.Length * entitySize))
                {
                    for (long i = 0; i < array.Length; i++)
                    {
                        accessor.Write(i * entitySize, ref array[i]);
                    }
                }
            }
        }

        [OneTimeSetUp]
        public void Init()
        {
            var arraySize = 6;
            var entitySize = Marshal.SizeOf(typeof(PayloadEvent));
            _expectedValues = _fixture.CreateMany<PayloadEvent>(arraySize).ToArray();
            _generatedFile = Path.Combine(Directory.GetCurrentDirectory(), _fixture.Create<string>() + ".bin");
            WriteDataToFile(_generatedFile, entitySize, _expectedValues);
            _dataProvider = new MMFDataProvider(_generatedFile, entitySize);
        }

        [OneTimeTearDown]
        public void Clean()
        {
            _dataProvider?.Dispose();
            if (File.Exists(_generatedFile))
                File.Delete(_generatedFile);
        }

        [Test]
        public async Task GetEventAtIndex_SimpleValue_ReturnedEquals()
        {
            //Check
            for (int i = 0; i < _expectedValues.Length; i++)
            {
                var value = await _dataProvider.GetEventAtIndexAsync(i);
                value.Should().BeEquivalentTo(_expectedValues[i]);
            }
        }

        [Test]
        [TestCase(-1)]
        [TestCase(7)]
        public void GetEventAtIndex_IndexOutOfRange_ExceptionThrow(long index)
        {
            //Check
            Assert.ThrowsAsync<IndexOutOfRangeException>(async () => await _dataProvider.GetEventAtIndexAsync(index));
        }

        [Test]
        public async Task GetEventsBetween_GetAllElements_ArrayShouldBeEquivalentToSource()
        { 
            //Check
            var actual = await _dataProvider.GetEventsBetweenAsync(0, 5);
            actual.Should().BeEquivalentTo(_expectedValues);
        }

        [Test]
        public async Task GetEventsBetween_GetSubsequence_EquivalentElements()
        {
            //Check
            var events = await _dataProvider.GetEventsBetweenAsync(2, 3);
            var actual = events.ToArray();
            actual.Length.Should().Be(2);
            actual[0].Should().BeEquivalentTo(_expectedValues[2]);
            actual[1].Should().BeEquivalentTo(_expectedValues[3]);
        }

        [Test]
        public void GetGlobalEventsCount_ReturnsCount_ShouldBeSuccess()
        {
            //Check
            var globalCount = _dataProvider.GetGlobalEventsCount();
            globalCount.Should().Be(_expectedValues.Length);
        }

        [Test]
        public void GetGlobalStartTick_ShouldReturnTicksOfFirstEvent()
        {
            //Check
            var startTicks = _dataProvider.GetGlobalStartTick();
            startTicks.Should().Be(_expectedValues[0].Ticks);
        }

        [Test]
        public void GetGlobalStopTick_ShouldReturnTicksOfLastEvent()
        {
            //Check
            var stopTicks = _dataProvider.GetGlobalStopTick();
            stopTicks.Should().Be(_expectedValues[_expectedValues .Length- 1].Ticks);
        }
    }
}
