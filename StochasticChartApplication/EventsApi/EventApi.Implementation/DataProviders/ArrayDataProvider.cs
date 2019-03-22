using System;
using System.Collections.Generic;
using EventApi.Models;

namespace EventApi.Implementation.DataProviders
{
    public class ArrayDataProvider:IDataProvider
    {
        private readonly PayloadEvent[] _array;

        public ArrayDataProvider(PayloadEvent[] array)
        {
            _array = array ?? throw new ArgumentNullException(nameof(array));
        }

        public PayloadEvent GetEventAtIndex(long index)
        {
            return _array[index];
        }

        public IEnumerable<PayloadEvent> GetEventsBetween(long startIndex, long stopIndex)
        {
            if (stopIndex <= startIndex)
                return new PayloadEvent[0];
            var result = new PayloadEvent[stopIndex - startIndex + 1];
            Array.Copy(_array, startIndex, result, 0, stopIndex - startIndex + 1);
            return result;
        }

        public long GetGlobalEventsCount()
        {
            return _array.Length;
        }

        public long GetGlobalStartTick()
        {
            if (_array.Length > 0)
                return _array[0].Ticks;
            return -1;
        }

        public long GetGlobalStopTick()
        {
            if (_array.Length > 0)
                return _array[_array.Length - 1].Ticks;
            return -1;
        }

        public void Init()
        {
            //do nothing
        }

        public void Dispose()
        {
            //do nothing
        }
    }
}
