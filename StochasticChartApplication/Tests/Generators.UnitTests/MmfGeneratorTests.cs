﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AutoFixture;
using EventApi.Implementation.DataProviders;
using FluentAssertions;
using Generators.UnitTests.TestCases;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NUnit.Framework;
using Assert = NUnit.Framework.Assert;

namespace Generators.UnitTests
{
    [TestClass]
    public class MmfGeneratorTests : BaseGeneratorTest
    {
        private readonly List<string> _generatedFiles = new List<string>();
        private readonly IFixture _fixture = new Fixture();

        [OneTimeTearDown]
        public void CleanUp()
        {
            foreach (var generatedFile in _generatedFiles)
                File.Delete(generatedFile);
        }

        [TestMethod]
        public void GenerateDataProvider_CheckCreatedProvider()
        {
            var size = 1000L;
            var fileName = Path.Combine(Directory.GetCurrentDirectory(), "GeneratorUnitTest", _fixture.Create<string>() + ".bin");
            var generatorFactory = new GeneratorFactory();
            var generator = generatorFactory.GetGenerator(ProviderType.MemoryMappedFile);
            var dataProvider = generator.GenerateDataProviderAsync(size, new []{ fileName }).Result;
            _generatedFiles.Add(fileName);
            CheckDataProvider(dataProvider, size);
            dataProvider.Dispose();
            Assert.That(File.Exists(fileName));
        }

        [TestMethod]
        [TestCaseSource(typeof(MmfTestCases), nameof(MmfTestCases.BadArgumentsTestCase))]
        public void GenerateDataProvider_ShouldBeException_BadArgumentsTestCase(long collectionSize, Wrapper wrapper)
        {
            var generatorFactory = new GeneratorFactory();
            var generator = generatorFactory.GetGenerator(ProviderType.MemoryMappedFile);
            CheckInvalidParameters(generator, collectionSize, wrapper.Params);
        }
    }
}
