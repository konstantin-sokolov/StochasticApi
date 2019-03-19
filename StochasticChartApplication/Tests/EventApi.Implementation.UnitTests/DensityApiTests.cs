using System.Collections.Generic;
using System.Linq;
using AutoFixture;
using EventApi.Implementation.Api;
using EventApi.Implementation.DataProviders;
using EventApi.Implementation.UnitTests.TestCases;
using EventApi.Models;
using FluentAssertions;
using Moq;
using NLog;
using NUnit.Framework;

namespace EventApi.Implementation.UnitTests
{
    [TestFixture]
    public class DensityApiTests
    {
        private DensityApi _densityApi;
        private readonly IFixture _fixture = new Fixture();
        private PayloadEvent[] _array;

        [OneTimeSetUp]
        public void Init()
        {
            _array = _fixture.CreateMany<PayloadEvent>(6).ToArray();
            _array[0].Ticks = 50;
            _array[1].Ticks = 70;
            _array[2].Ticks = 100;
            _array[3].Ticks = 120;
            _array[4].Ticks = 150;
            _array[5].Ticks = 200;

            var loggerMock = new Mock<ILogger>();
            var dataProvider = new ArrayDataProvider(_array);
            _densityApi = new DensityApi(loggerMock.Object, dataProvider);
        }

        [Test]
        [TestCaseSource(typeof(DensityApiTestCases),nameof(DensityApiTestCases.TestCases))]
        public void GetEvents_CheckIntervals(long start, long stop,long groupInterval, IEnumerable<DensityInfo> expected)
        {
            var actual = _densityApi.GetDensityInfo(start, stop, groupInterval);
            actual.Should().BeEquivalentTo(expected);
        }
    }
}