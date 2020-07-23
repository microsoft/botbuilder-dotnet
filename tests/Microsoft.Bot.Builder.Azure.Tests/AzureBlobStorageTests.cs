// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Azure.Tests
{
    [TestClass]
    public class AzureBlobStorageTests : BlobStorageBaseTests
    {
        public TestContext TestContext { get; set; }

        protected override string ContainerName
        {
            get
            {
                var containerName = TestContext.TestName.ToLower().Replace("_", string.Empty);
                NameValidator.ValidateContainerName(containerName);
                return containerName;
            }
        }

        [TestInitialize]
        public async Task Init()
        {
            if (StorageEmulatorHelper.CheckEmulator())
            {
                await CloudStorageAccount.Parse(ConnectionString)
                                .CreateCloudBlobClient()
                                .GetContainerReference(ContainerName)
                                .DeleteIfExistsAsync().ConfigureAwait(false);
            }
        }

        [TestCleanup]
        public async Task Cleanup()
        {
            if (StorageEmulatorHelper.CheckEmulator())
            {
                await CloudStorageAccount.Parse(ConnectionString)
                                .CreateCloudBlobClient()
                                .GetContainerReference(ContainerName)
                                .DeleteIfExistsAsync().ConfigureAwait(false);
            }
        }

        [TestMethod]
        public void BlobStorageParamTest()
        {
            if (StorageEmulatorHelper.CheckEmulator())
            {
                Assert.ThrowsException<FormatException>(() => new AzureBlobStorage("123", ContainerName));

                Assert.ThrowsException<ArgumentNullException>(() =>
                    new AzureBlobStorage((CloudStorageAccount)null, ContainerName));

                Assert.ThrowsException<ArgumentNullException>(() =>
                    new AzureBlobStorage((string)null, ContainerName));

                Assert.ThrowsException<ArgumentNullException>(() =>
                    new AzureBlobStorage((CloudStorageAccount)null, null));

                Assert.ThrowsException<ArgumentNullException>(() => new AzureBlobStorage((string)null, null));

                Assert.ThrowsException<ArgumentNullException>(() =>
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
            else
            {
                return new AzureBlobStorage(storageAccount, ContainerName);
            }
        }
    }
}
