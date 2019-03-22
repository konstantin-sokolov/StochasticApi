using System;
using System.Runtime.Remoting.Messaging;
using System.Threading.Tasks;
using EventApi.Implementation.DataProviders;

namespace Generators
{
    public interface IEventGenerator
    {
        Task<IDataProvider> GenerateDataProviderAsync(long size, object[] parameters);
        event Action<double> EventGenerateProgressChanged;
    }
}
