using System;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using EventApi.Implementation.DataProviders;
using EventApi.Models;
using NLog;

namespace Generators
{
    class MmfEventGenerator : BaseEventGenerator, IEventGenerator
    {
        private readonly ILogger _logger;
        public const long BATCH_SIZE = 10000;

        public MmfEventGenerator(ILogger logger)
        {
            _logger = logger;
        }

        public async Task<IDataProvider> GenerateDataProviderAsync(long collectionSize, object[] parameters)
        {
            if (collectionSize % 2 != 0)
            {
                _logger.Error("MmfEventGenerator: Collection size should be even number");
                throw new ArgumentException("Collection size should be even number", nameof(collectionSize));
            }

            var filePath = ParseAndValidateArgs(parameters);
            try
            {
                var entitySize = Marshal.SizeOf(typeof(PayloadEvent));
                await Task.Run(() => { WriteDataToFile(filePath, entitySize, collectionSize); }).ConfigureAwait(false);
                return new MMFDataProvider(filePath, entitySize);
            }
            catch (Exception e)
            {
                _logger.Error(e);
                throw;
            }
        }

        public event Action<int> EventGenerateProgressChanged;

        private string ParseAndValidateArgs(object[] parameters)
        {
            if (parameters == null)
            {
                var message = "Wrong parameters for MemoryMappedGenerator. Parameters shouldn't be null";
                _logger.Error(message);
                throw new ArgumentException(message);
            }

            if (parameters.Length != 1)
            {
                var message = "Wrong count of parameters for MemoryMappedGenerator. There should be 1 parameter - filepath";
                _logger.Error(message);
                throw new ArgumentException(message);
            }
            var filePath = (string) parameters[0];
            if (string.IsNullOrEmpty(filePath))
            {
                var message = "Wrong value for filepath. It can't be null or empty";
                _logger.Error(message);
                throw new ArgumentException(message);
            }

            var directory = Path.GetDirectoryName(filePath);
            if (string.IsNullOrWhiteSpace(directory))
            {
                directory = Path.Combine(Directory.GetCurrentDirectory(), "MmfDataProviders");
                filePath = Path.Combine(directory, filePath);
            }
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);
            if (File.Exists(filePath))
                File.Delete(filePath);

            return filePath;
        }

        private void WriteDataToFile(string filePath, int entitySize, long collectionSize)
        {
            var fileSize = entitySize * collectionSize;
            int currentPercent = 0;
            using (var fs = new FileStream(filePath, FileMode.CreateNew))
            using (var mmf = MemoryMappedFile.CreateFromFile(fs, "Events", fileSize, MemoryMappedFileAccess.ReadWrite, HandleInheritability.None, false))
            {
                var batchCount = Math.Ceiling((double)collectionSize / BATCH_SIZE);

                for (int b = 0; b < batchCount; b++)
                {
                    var currentBatchSize = Math.Min(BATCH_SIZE, collectionSize - b * BATCH_SIZE);

                    using (var accessor = mmf.CreateViewAccessor(b * BATCH_SIZE * entitySize, currentBatchSize * entitySize))
                    {
                        for (long i = 0; i < currentBatchSize; i++)
                        {
                            var pEVent = GetNextEvent();
                            accessor.Write(i * entitySize, ref pEVent);
                            var tempPercent = (int)(100 * (i + b * BATCH_SIZE) / collectionSize);
                            if (currentPercent != tempPercent)
                            {
                                currentPercent = tempPercent;
                                EventGenerateProgressChanged?.Invoke(currentPercent);
                            }
                        }
                    }
                }
            }
        }
    }
}