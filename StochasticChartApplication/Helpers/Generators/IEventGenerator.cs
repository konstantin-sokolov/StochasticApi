using System;
using System.Threading.Tasks;
using EventApi.Implementation.DataProviders;

namespace Generators
{
    public interface IEventGenerator
    {
        Task<IDataProvider> GenerateDataProviderAsync(long size, object[] parameters);
        event Action<int> EventGenerateProgressChanged;
    }
}
