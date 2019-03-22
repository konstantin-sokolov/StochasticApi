using System;
using System.Threading.Tasks;
using EventApi.Implementation.DataProviders;
using EventApi.Models;

namespace Generators
{
    class ArrayEventGenerator : BaseEventGenerator, IEventGenerator
    {
        public async Task<IDataProvider> GenerateDataProviderAsync(long collectionSize, object[] parameters)
        {
            PayloadEvent[] eventArray = null;
            int currentPercent = 0;
            await Task.Run(() =>
            {
                eventArray = new PayloadEvent[collectionSize];
                for (int i = 0; i < collectionSize; i++)
                {
                    var newEvent = GetNextEvent();
                    eventArray[i] = newEvent;
                    var percent = (int)(100 * i / collectionSize);
                    if (currentPercent != percent)
                    {
                        currentPercent = percent;
                        EventGenerateProgressChanged?.Invoke(percent);
                    }
                   
                }
            }).ConfigureAwait(false);
            return new ArrayDataProvider(eventArray);
        }

        public event Action<int> EventGenerateProgressChanged;
    }
}