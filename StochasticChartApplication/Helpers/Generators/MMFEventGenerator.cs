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
            var filePath = ParseArgs(parameters);

            var entitySize = Marshal.SizeOf(typeof(PayloadEvent));
            await Task.Run(() => { WriteDataToFile(filePath, entitySize, collectionSize); }).ConfigureAwait(false);

            return new MMFDataProvider(filePath, entitySize);
        }

        public event Action<double> EventGenerateProgressChanged;

        private string ParseArgs(object[] parameters)
        {
            if (parameters.Length != 1)
                throw new ArgumentException("Wrong count of parameters for MemoryMappedGenerator. There should be 1 parameter - filepath");
            return (string) parameters[0];
        }

        private void WriteDataToFile(string filePath, int entitySize, long collectionSize)
        {
            var fileSize = entitySize * collectionSize;
            using (var fs = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite))
            using (var mmf = MemoryMappedFile.CreateFromFile(fs, "Events", fileSize, MemoryMappedFileAccess.ReadWrite, HandleInheritability.None, false))
            {
                using (var accessor = mmf.CreateViewAccessor(0, fileSize))
                {
                    for (long i = 0; i < collectionSize; i++)
                    {
                        var pEVent = GetNextEvent();
                        accessor.Write(i * entitySize, ref pEVent);
                        EventGenerateProgressChanged?.Invoke((double) i / collectionSize);
                    }
                }
            }
        }
    }
}