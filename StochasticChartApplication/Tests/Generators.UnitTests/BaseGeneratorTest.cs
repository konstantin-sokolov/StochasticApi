using System;
using System.Linq;
using EventApi.Implementation.DataProviders;
using EventApi.Models;
using FluentAssertions;
using NUnit.Framework;

namespace Generators.UnitTests
{
    public class BaseGeneratorTest  
    {
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

        protected void CheckInvalidParameters(IEventGenerator generator, long collectionSize,object[] parameters)
        {
            
            var task = generator.GenerateDataProviderAsync(collectionSize, parameters);
            task.Exception.Should().NotBeNull();
            task.Exception.InnerExceptions.Count.Should().Be(1);
            task.Exception.InnerExceptions.FirstOrDefault().Should().BeOfType<ArgumentException>();

        }
    }
}