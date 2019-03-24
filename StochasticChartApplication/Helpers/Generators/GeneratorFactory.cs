using System;
using EventApi.Implementation.DataProviders;
using NLog;

namespace Generators
{
    public class GeneratorFactory
    {
        private readonly ILogger _logger;

        public GeneratorFactory(ILogger logger)
        {
            _logger = logger;
        }
        public IEventGenerator GetGenerator(ProviderType type)
        {
            switch (type)
            {
                case ProviderType.Array:
                    return new ArrayEventGenerator(_logger);
                case ProviderType.MemoryMappedFile:
                    return new MmfEventGenerator(_logger);
                default:
                    throw new Exception($"Unknown provider type:{type}");
            }
        }
    }
}
