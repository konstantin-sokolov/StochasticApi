using System;
using System.Threading.Tasks;
using EventApi.Implementation.DataProviders;
using EventsApi.Contracts.DataProviders;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using NLog;
using NUnit.Framework;

namespace Generators.UnitTests
{
    [TestClass]
    public class ArrayGeneratorTests:BaseGeneratorTest
    {
        [TestMethod]
        public async Task GenerateDataProvider_CheckCreatedProvider()
        {
            var size = 1000;
        
            var generatorFactory = new GeneratorFactory(_logger);
            var generator = generatorFactory.GetGenerator(ProviderType.Array);
            var dataProvider = await generator.GenerateDataProviderAsync(size, null);
            await CheckDataProvider(dataProvider, size);
        }

        [TestMethod]
        public void TestGeneratedValues()
        {
            var generatorFactory = new GeneratorFactory(_logger);
            var generator = generatorFactory.GetGenerator(ProviderType.Array);

            CheckInvalidParameters(generator, 7, null);
        }
    }
}
