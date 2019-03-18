using System;
using System.Linq;
using System.Runtime.InteropServices;
using AutoFixture;
using EventApi.Implementation.DataProviders;
using EventApi.Models;
using FluentAssertions;
using NUnit.Framework;

namespace EventApi.Implementation.UnitTests
{
    [TestFixture]
    [Parallelizable(ParallelScope.All)]
    class ArrayDataProviderTests
    {
        readonly IFixture _fixture = new Fixture();
        private int _entitySize;

        [OneTimeSetUp]
        public void Init()
        {
            _entitySize = Marshal.SizeOf(typeof(PayloadEvent));
        }

        [Test]
        public void GetEventAtIndex_SimpleValue_ReturnedEquals()
        {
            var arraySize = 6;
            var array = _fixture.CreateMany<PayloadEvent>(arraySize).ToArray();
            var dataProvider = new ArrayDataProvider(array);
            for (int i = 0; i < arraySize; i++)
            {
                var value = dataProvider.GetEventAtIndex(i);
                value.Should().BeEquivalentTo(array[i]);
            }
        }

        [Test]
        [TestCase(-1)]
        [TestCase(7)]
        public void GetEventAtIndex_IndexOutOfRange_ExceptionThrow(long index)
        {
            var arraySize = 6;
            var array = _fixture.CreateMany<PayloadEvent>(arraySize).ToArray();
            var dataProvider = new ArrayDataProvider(array);
            Assert.Throws<IndexOutOfRangeException>(() => dataProvider.GetEventAtIndex(index));
        }

        [Test]
        public void GetEventsBetween_GetAllElements_ArrayShouldBeEquivalentToSource()
        {
            var arraySize = 6;
            var array = _fixture.CreateMany<PayloadEvent>(arraySize).ToArray();
            var dataProvider = new ArrayDataProvider(array);
            var actual = dataProvider.GetEventsBetween(0, 5);
            actual.Should().BeEquivalentTo(array);
        }

        [Test]
        public void GetEventsBetween_GetSubsequence_EquivalentElements()
        {
            var arraySize = 6;
            var array = _fixture.CreateMany<PayloadEvent>(arraySize).ToArray();
            var dataProvider = new ArrayDataProvider(array);
            var actual = dataProvider.GetEventsBetween(2, 3);
            actual.Length.Should().Be(2);
            actual[0].Should().BeEquivalentTo(array[2]);
            actual[1].Should().BeEquivalentTo(array[3]);
        }

        [Test]
        public void GetGlobalEventsCount_ReturnsCount_ShouldBeSuccess()
        {
            var arraySize = 6;
            var array = _fixture.CreateMany<PayloadEvent>(arraySize).ToArray();
            var dataProvider = new ArrayDataProvider(array);
            var globalCount = dataProvider.GetGlobalEventsCount();
            globalCount.Should().Be(arraySize);
        }

        [Test]
        public void GetGlobalStartTick_ShouldReturnTicksOfFirstEvent()
        {
            var arraySize = 6;
            var array = _fixture.CreateMany<PayloadEvent>(arraySize).ToArray();
            var dataProvider = new ArrayDataProvider(array);
            var startTicks = dataProvider.GetGlobalStartTick();
            startTicks.Should().Be(array[0].Ticks);
        }

        [Test]
        public void GetGlobalStopTick_ShouldReturnTicksOfLastEvent()
        {
            var arraySize = 6;
            var array = _fixture.CreateMany<PayloadEvent>(arraySize).ToArray();
            var dataProvider = new ArrayDataProvider(array);
            var stopTicks = dataProvider.GetGlobalStopTick();
            stopTicks.Should().Be(array[arraySize-1].Ticks);
        }
    }
}
