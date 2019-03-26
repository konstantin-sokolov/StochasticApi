using System;
using System.Collections.Generic;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Threading.Tasks;
using EventApi.Models;
using EventsApi.Contracts.DataProviders;

namespace EventApi.Implementation.DataProviders
{
    public class MMFDataProvider : IDataProvider
    {
        private readonly int _entitySize;
        private readonly MemoryMappedFile _memoryMappedFile;

        private readonly long _globalStartTicks;
        private readonly long _globalEventsCount;
        private readonly long _globalStopTicks;
       
        public MMFDataProvider(string fileSource, int entitySize)
        {
            if (!File.Exists(fileSource))
                throw new ArgumentException($"File doesn't exist:{fileSource}", nameof(fileSource));
            _memoryMappedFile = MemoryMappedFile.CreateFromFile(fileSource, FileMode.Open);
            _entitySize = entitySize;
            var fileSize = new FileInfo(fileSource).Length;

            if (fileSize % _entitySize != 0)
                throw new Exception("File size not proportional to entity size");

            _globalEventsCount = fileSize / _entitySize;
            if (_globalEventsCount <= 0)
            {
                _globalStartTicks = -1;
                _globalStopTicks = -1;
                return;
            }

            _globalStartTicks = GetEventAtIndex(0).Ticks;
            _globalStopTicks = GetEventAtIndex(_globalEventsCount - 1).Ticks;
        }

        private IEnumerable<PayloadEvent> GetEventsBetween(long startIndex, long stopIndex)
        {
            if (stopIndex <= startIndex)
                return new PayloadEvent[0];

            var offset = startIndex * _entitySize;
            var count = stopIndex - startIndex + 1;
            var result = new PayloadEvent[count];
            var byteSize = _entitySize * count;
            using (var accessor = _memoryMappedFile.CreateViewAccessor(offset, byteSize))
            {
                for (long i = 0; i < count; i++)
                {
                    accessor.Read(i * _entitySize, out PayloadEvent payloadEvent);
                    result[i] = payloadEvent;
                }
            }

            return result;
        }

        private PayloadEvent GetEventAtIndex(long index)
        {
            if (index < 0 || index >= _globalEventsCount)
                throw new IndexOutOfRangeException();

            var offset = index * _entitySize;
            using (var accessor = _memoryMappedFile.CreateViewAccessor(offset, _entitySize))
            {
                accessor.Read(0, out PayloadEvent payloadEvent);
                return payloadEvent;
            }
        }

        public Task<PayloadEvent> GetEventAtIndexAsync(long index)
        {
            return Task.Run(() => GetEventAtIndex(index));
        }

        public Task<IEnumerable<PayloadEvent>> GetEventsBetweenAsync(long startIndex, long stopIndex)
        {
            return Task.Run(() => GetEventsBetween(startIndex,stopIndex));
        }

        public long GetGlobalEventsCount()
        {
            return _globalEventsCount;
        }

        public long GetGlobalStartTick()
        {
            return _globalStartTicks;
        }

        public long GetGlobalStopTick()
        {
            return _globalStopTicks;
        }

        public void Dispose()
        {
            _memoryMappedFile?.Dispose();
        }
    }
}