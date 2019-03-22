using System;
using EventApi.Implementation.DataProviders;

namespace Generators
{
    public class GeneratorFactory
    {
        public IEventGenerator GetGenerator(ProviderType type)
        {
            switch (type)
            {
                case ProviderType.Array:
                    return new ArrayEventGenerator();
                case ProviderType.MemoryMappedFile:
                    return new MmfEventGenerator();
                default:
                    throw new Exception($"Unknown provider type:{type}");
            }
        }
    }
}
