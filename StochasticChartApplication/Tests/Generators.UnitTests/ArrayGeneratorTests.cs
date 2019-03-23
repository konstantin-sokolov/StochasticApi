using System;
using EventApi.Implementation.DataProviders;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Generators.UnitTests
{
    [TestClass]
    public class ArrayGeneratorTests:BaseGeneratorTest
    {
        [TestMethod]
        public void GenerateDataProvider_CheckCreatedProvider()
        {
            var size = 1000;
            var generatorFactory = new GeneratorFactory();
            var generator = generatorFactory.GetGenerator(ProviderType.Array);
            var dataProvider = generator.GenerateDataProviderAsync(size, null).Result;
            CheckDataProvider(dataProvider, size);
        }

        [TestMethod]
        public void TestGeneratedValues()
        {
            var generatorFactory = new GeneratorFactory();
            var generator = generatorFactory.GetGenerator(ProviderType.Array);

            CheckInvalidParameters(generator, 7, null);
        }
    }
}
