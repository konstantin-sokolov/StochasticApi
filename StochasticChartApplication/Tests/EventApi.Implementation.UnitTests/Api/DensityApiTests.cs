﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using EventApi.Implementation.Api;
using EventApi.Implementation.DataProviders;
using EventApi.Implementation.UnitTests.TestCases;
using EventApi.Models;
using FluentAssertions;
using Moq;
using NLog;
using NUnit.Framework;

namespace EventApi.Implementation.UnitTests.Api
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
        public async Task GetEvents_CheckIntervals(long start, long stop,long groupInterval, IEnumerable<DensityInfo> expected)
        {
            var actual = await _densityApi.GetDensityInfoAsync(start, stop, groupInterval);
            actual.Should().BeEquivalentTo(expected);
        }

        [Test]
        public async Task GetDensityInfoAsync_CheckCancellation_ShouldBeException()
        {
            using (var ctnSource = new CancellationTokenSource())
            {
                var token = ctnSource.Token;
                ctnSource.Cancel();

                Assert.That(async () => await _densityApi.GetDensityInfoAsync(0, 1000, 10, token),
                    Throws.Exception
                        .AssignableTo(typeof(OperationCanceledException)));
            }
        }
        [Test]
        public async Task GetInfoForLeftSideAsync_CheckCancellation_ShouldBeException()
        {
            using (var ctnSource = new CancellationTokenSource())
            {
                var token = ctnSource.Token;
                ctnSource.Cancel();

                Assert.That(async () => await _densityApi.GetInfoForLeftSideAsync(100, 1, 0, 10, token),
                    Throws.Exception
                        .AssignableTo(typeof(OperationCanceledException)));
            }
        }
        [Test]
        public async Task GetInfoForRightSideAsync_CheckCancellation_ShouldBeException()
        {
            using (var ctnSource = new CancellationTokenSource())
            {
                var token = ctnSource.Token;
                ctnSource.Cancel();

                Assert.That(async () => await _densityApi.GetInfoForRightSideAsync(100, 1, 120, 10, token),
                    Throws.Exception
                        .AssignableTo(typeof(OperationCanceledException)));
            }
        }
    }
}