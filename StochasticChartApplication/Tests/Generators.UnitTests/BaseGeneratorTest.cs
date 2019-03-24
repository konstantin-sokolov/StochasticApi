using System;
using System.Linq;
using EventApi.Implementation.DataProviders;
using EventApi.Models;
using FluentAssertions;
using Moq;
using NLog;

namespace Generators.UnitTests
{
    public abstract class BaseGeneratorTest
    {
        protected ILogger _logger;

        protected BaseGeneratorTest()
        {
            var loggerMock = new Mock<ILogger>();
            _logger = loggerMock.Object;
        }

        protected void CheckDataProvider(IDataProvider provider, long expectedSize)
        {
            var actualSize = provider.GetGlobalEventsCount();
            actualSize.Should().Be(expectedSize);
            var currentTicks = long.MinValue;
            for (int i = 0; i < expectedSize; i++)
            {
                var currentEvent = provider.GetEventAtIndex(i);
                currentEvent.Ticks.Should().BeGreaterThan(currentTicks);
                currentTicks = currentEvent.Ticks;
                currentEvent.EventType.Should().Be(i % 2 == 0 ? EventType.start : EventType.stop);
            }
        }

        protected void CheckInvalidParameters(IEventGenerator generator, long collectionSize, object[] parameters)
        {
            var task = generator.GenerateDataProviderAsync(collectionSize, parameters);
            task.Exception.Should().NotBeNull();
            task.Exception.InnerExceptions.Count.Should().Be(1);
            task.Exception.InnerExceptions.FirstOrDefault().Should().BeOfType<ArgumentException>();
        }
    }
}