// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Reflection;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Microsoft.Bot.Builder.Azure.Blobs;
using Newtonsoft.Json;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Microsoft.Bot.Builder.Azure.Tests
{
    public class BlobsStorageTests : BlobStorageBaseTests, IAsyncLifetime
    {
        private readonly string _testName;

        public BlobsStorageTests(ITestOutputHelper testOutputHelper)
        {
            var helper = (TestOutputHelper)testOutputHelper;

            var test = (ITest)helper.GetType().GetField("test", BindingFlags.NonPublic | BindingFlags.Instance)
                .GetValue(helper);

            _testName = test.TestCase.TestMethod.Method.Name;

            if (StorageEmulatorHelper.CheckEmulator())
            {
                new BlobContainerClient(ConnectionString, ContainerName)
                    .DeleteIfExistsAsync().ConfigureAwait(false);
            }
        }

        protected override string ContainerName => $"blobs{_testName.ToLower().Replace("_", string.Empty)}";

        public Task InitializeAsync()
        {
            return Task.CompletedTask;
        }

        public async Task DisposeAsync()
        {
            if (StorageEmulatorHelper.CheckEmulator())
            {
                await new BlobContainerClient(ConnectionString, ContainerName)
                    .DeleteIfExistsAsync().ConfigureAwait(false);
            }
        }

        [Fact]
        public void BlobStorageParamTest()
        {
            if (StorageEmulatorHelper.CheckEmulator())
            {
                Assert.Throws<ArgumentNullException>(() =>
                    new BlobsStorage(null, ContainerName));

                Assert.Throws<ArgumentNullException>(() =>
                    new BlobsStorage(ConnectionString, null));

                Assert.Throws<ArgumentNullException>(() =>
                    new BlobsStorage(string.Empty, ContainerName));

                Assert.Throws<ArgumentNullException>(() =>
                    new BlobsStorage(ConnectionString, string.Empty));
            }
        }

        protected override IStorage GetStorage(bool typeNameHandlingNone = false)
        {
            if (typeNameHandlingNone)
            {
                return new BlobsStorage(ConnectionString, ContainerName, new JsonSerializer() { TypeNameHandling = TypeNameHandling.None });
            }

            return new BlobsStorage(ConnectionString, ContainerName);
        }
    }
}
