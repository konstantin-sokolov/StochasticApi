using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
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
    public class EventApiTests
    {
        private Api.EventApi _eventApi;
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
            _eventApi = new Api.EventApi(loggerMock.Object, dataProvider);
        }

        [Test]
        [TestCaseSource(typeof(EventApiTestCases), nameof(EventApiTestCases.TestCases))]
        public async Task GetEvents_CheckIntervals(long start, long stop, int[] indexes)
        {
            var actual = await _eventApi.GetEventsAsync(start, stop);
            var actualArray = actual.ToArray();
            actualArray.Length.Should().Be(indexes.Length);
            for (int i = 0; i < indexes.Length; i++)
                actualArray[i].Should().BeEquivalentTo(_array[indexes[i]]);
        }

        [Test]
        public async Task GetEvents_CheckCancellation_ShouldBeException()
        {
            using (var ctnSource = new CancellationTokenSource())
            {
                var token = ctnSource.Token;
                ctnSource.Cancel();

                Assert.That(async () => await _eventApi.GetEventsAsync(0, 10, token),
                    Throws.Exception
                        .AssignableTo(typeof(OperationCanceledException)));
            }
        }
    }
}