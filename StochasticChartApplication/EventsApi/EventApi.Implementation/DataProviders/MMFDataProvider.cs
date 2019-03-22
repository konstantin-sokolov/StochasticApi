using System;
using System.Collections.Generic;
using System.IO;
using System.IO.MemoryMappedFiles;
using EventApi.Models;

namespace EventApi.Implementation.DataProviders
{
    public class MMFDataProvider : IDataProvider, IDisposable
    {
        private readonly int _entitySize;
        private readonly MemoryMappedFile _memoryMappedFile;
        private readonly long _fileSize;

        private long _globalStartTicks;
        private long _globalEventsCount;
        private long _globalStopTicks;
       
        public MMFDataProvider(string fileSource, int entitySize)
        {
            if (!File.Exists(fileSource))
                throw new ArgumentException($"File doesn't exist:{fileSource}", nameof(fileSource));
            _memoryMappedFile = MemoryMappedFile.CreateFromFile(fileSource, FileMode.Open);
            _entitySize = entitySize;
            _fileSize = new FileInfo(fileSource).Length;
        }

        public PayloadEvent GetEventAtIndex(long index)
        {
            var offset = index * _entitySize;
            using (var accessor = _memoryMappedFile.CreateViewAccessor(offset, _entitySize))
            {
                accessor.Read(0, out PayloadEvent payloadEvent);
                return payloadEvent;
            }
        }

        public IEnumerable<PayloadEvent> GetEventsBetween(long startIndex, long stopIndex)
        {
            if (stopIndex<=startIndex)
                return new PayloadEvent[0];

            var offset = startIndex * _entitySize;
            var count = stopIndex - startIndex;
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

        public void Init()
        {
            if (_fileSize % _entitySize != 0)
                throw new Exception("File size not proportional to entity size");
            _globalEventsCount = _fileSize / _entitySize;
            if (_globalEventsCount <= 0)
            {
                _globalStartTicks = -1;
                _globalStopTicks = -1;
                return;
            }

            _globalStartTicks = GetEventAtIndex(0).Ticks;
            _globalStopTicks = GetEventAtIndex(_globalEventsCount - 1).Ticks;
        }

        public void Dispose()
        {
            _memoryMappedFile?.Dispose();
        }
    }
}