using System;
using System.Threading.Tasks;
using EventApi.Implementation.DataProviders;
using EventApi.Models;
using NLog;

namespace Generators
{
    internal class ArrayEventGenerator : BaseEventGenerator, IEventGenerator
    {
        private readonly ILogger _logger;

        public ArrayEventGenerator(ILogger logger)
        {
            _logger = logger;
        }

        public async Task<IDataProvider> GenerateDataProviderAsync(long collectionSize, object[] parameters)
        {
            if (collectionSize % 2 != 0)
            {
                _logger.Error("Requested size for generation is not even.");
                throw new ArgumentException("Collection size should be even number", nameof(collectionSize));
            }

            try
            {
                _logger.Info($"Generate new ArrayDataProvider:{collectionSize}");
                PayloadEvent[] eventArray = null;
                int currentPercent = 0;
                await Task.Run(() =>
                {
                    eventArray = new PayloadEvent[collectionSize];
                    for (int i = 0; i < collectionSize; i++)
                    {
                        var newEvent = GetNextEvent();
                        eventArray[i] = newEvent;
                        var percent = (int) (100 * i / collectionSize);
                        if (currentPercent != percent)
                        {
                            currentPercent = percent;
                            EventGenerateProgressChanged?.Invoke(percent);
                        }

                    }
                }).ConfigureAwait(false);
                _logger.Info($"ArrayDataProvider successfully generated");
                return new ArrayDataProvider(eventArray);
            }
            catch (Exception e)
            {
                _logger.Error(e);
                throw;
            }
        }

        public event Action<int> EventGenerateProgressChanged;
    }
}