using System;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using EventApi.Implementation.DataProviders;
using EventApi.Models;

namespace Generators
{
    class MmfEventGenerator : BaseEventGenerator, IEventGenerator
    {
        public async Task<IDataProvider> GenerateDataProviderAsync(long collectionSize, object[] parameters)
        {
            if (collectionSize % 2 != 0)
                throw new ArgumentException("Collection size should be even number", nameof(collectionSize));

            var filePath = ParseAndValidateArgs(parameters);
            var entitySize = Marshal.SizeOf(typeof(PayloadEvent));
            await Task.Run(() => { WriteDataToFile(filePath, entitySize, collectionSize); }).ConfigureAwait(false);

            return new MMFDataProvider(filePath, entitySize);
        }

        public event Action<int> EventGenerateProgressChanged;

        private string ParseAndValidateArgs(object[] parameters)
        {
            if (parameters == null)
                throw new ArgumentException("Wrong parameters for MemoryMappedGenerator. Parameters shouldn't be null");

            if (parameters.Length != 1)
                throw new ArgumentException("Wrong count of parameters for MemoryMappedGenerator. There should be 1 parameter - filepath");
            var filePath = (string) parameters[0];
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentException("Wrong value for filepath. It can't be null or empty");
            return filePath;
        }

        private void WriteDataToFile(string filePath, int entitySize, long collectionSize)
        {
            var directory = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            var fileSize = entitySize * collectionSize;
            int currentPercent = 0;
            using (var fs = new FileStream(filePath, FileMode.CreateNew))
            using (var mmf = MemoryMappedFile.CreateFromFile(fs, "Events", fileSize, MemoryMappedFileAccess.ReadWrite, HandleInheritability.None, false))
            {
                using (var accessor = mmf.CreateViewAccessor(0, fileSize))
                {
                    for (long i = 0; i < collectionSize; i++)
                    {
                        var pEVent = GetNextEvent();
                        accessor.Write(i * entitySize, ref pEVent);
                        var percent = (int)(100 * i / collectionSize);
                        if (currentPercent != percent)
                        {
                            currentPercent = percent;
                            EventGenerateProgressChanged?.Invoke(percent);
                        }
                    }
                }
            }
        }
    }
}