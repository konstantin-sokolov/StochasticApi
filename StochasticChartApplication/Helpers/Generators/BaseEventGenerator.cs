using System;
using EventApi.Models;

namespace Generators
{
    abstract class BaseEventGenerator
    {
        private readonly Random _random = new Random();
        private long _currentTicks = 0;
        private bool _currentEventStart = false;

        protected PayloadEvent GetNextEvent()
        {
            _currentEventStart = !_currentEventStart;
            var ticks = _currentTicks + _random.Next(10000) + 5000;
            _currentTicks = ticks;
            return new PayloadEvent(_currentEventStart ? EventType.start : EventType.stop, ticks);
        }
    }
}