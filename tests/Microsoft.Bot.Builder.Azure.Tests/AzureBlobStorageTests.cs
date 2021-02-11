// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Microsoft.Bot.Builder.Azure.Tests
{
    public class AzureBlobStorageTests : BlobStorageBaseTests, IAsyncLifetime
    {
        private readonly string _testName;

        public AzureBlobStorageTests(ITestOutputHelper testOutputHelper)
        {
            var helper = (TestOutputHelper)testOutputHelper;

            var test = (ITest)helper.GetType().GetField("test", BindingFlags.NonPublic | BindingFlags.Instance)
                .GetValue(helper);

            _testName = test.TestCase.TestMethod.Method.Name;

            if (StorageEmulatorHelper.CheckEmulator())
            {
                CloudStorageAccount.Parse(ConnectionString)
                    .CreateCloudBlobClient()
                    .GetContainerReference(ContainerName)
                    .DeleteIfExistsAsync().ConfigureAwait(false);
            }
        }

        protected override string ContainerName
        {
            get
            {
                var containerName = _testName.ToLower().Replace("_", string.Empty);
                NameValidator.ValidateContainerName(containerName);
                return containerName;
            }
        }

        public Task InitializeAsync()
        {
            return Task.CompletedTask;
        }

        public async Task DisposeAsync()
        {
            if (StorageEmulatorHelper.CheckEmulator())
            {
                await CloudStorageAccount.Parse(ConnectionString)
                                .CreateCloudBlobClient()
                                .GetContainerReference(ContainerName)
                                .DeleteIfExistsAsync().ConfigureAwait(false);
            }
        }

        [Fact]
        public void BlobStorageParamTest()
        {
            if (StorageEmulatorHelper.CheckEmulator())
            {
                Assert.Throws<FormatException>(() => new AzureBlobStorage("123", ContainerName));

                Assert.Throws<ArgumentNullException>(() =>
                    new AzureBlobStorage((CloudStorageAccount)null, ContainerName));

                Assert.Throws<ArgumentNullException>(() =>
                    new AzureBlobStorage((string)null, ContainerName));

                Assert.Throws<ArgumentNullException>(() =>
                    new AzureBlobStorage((CloudStorageAccount)null, null));

                Assert.Throws<ArgumentNullException>(() => new AzureBlobStorage((string)null, null));

                Assert.Throws<ArgumentNullException>(() =>
                    new AzureBlobStorage(CloudStorageAccount.Parse(ConnectionString), ContainerName, (JsonSerializer)null));
            }
        }

        protected override IStorage GetStorage(bool typeNameHandlingNone = false)
        {
            var storageAccount = CloudStorageAccount.Parse(ConnectionString);
            if (typeNameHandlingNone)
            {
                return new AzureBlobStorage(
                    storageAccount,
                    ContainerName,
                    new JsonSerializer() { TypeNameHandling = TypeNameHandling.None });
            }

            return new AzureBlobStorage(storageAccount, ContainerName);
        }
    }
}
