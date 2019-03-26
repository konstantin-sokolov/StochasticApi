using System;
using System.Threading.Tasks;
using EventsApi.Contracts.DataProviders;

namespace Generators
{
    public interface IEventGenerator
    {
        Task<IDataProvider> GenerateDataProviderAsync(long size, object[] parameters);
        event Action<int> EventGenerateProgressChanged;
    }
}
