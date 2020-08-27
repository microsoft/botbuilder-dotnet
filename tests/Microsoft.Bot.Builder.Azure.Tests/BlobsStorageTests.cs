// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Microsoft.Bot.Builder.Azure.Blobs;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Azure.Tests
{
    [TestClass]
    public class BlobsStorageTests : BlobStorageBaseTests
    {
        public TestContext TestContext { get; set; }

        protected override string ContainerName
        {
            get
            {
                return $"blobs{TestContext.TestName.ToLower().Replace("_", string.Empty)}";
            }
        }

        [TestInitialize]
        public async Task Init()
        {
            if (StorageEmulatorHelper.CheckEmulator())
            {
                await new BlobContainerClient(ConnectionString, ContainerName)
                    .DeleteIfExistsAsync().ConfigureAwait(false);
            }
        }

        [TestCleanup]
        public async Task Cleanup()
        {
            if (StorageEmulatorHelper.CheckEmulator())
            {
                await new BlobContainerClient(ConnectionString, ContainerName)
                    .DeleteIfExistsAsync().ConfigureAwait(false);
            }
        }

        [TestMethod]
        public void BlobStorageParamTest()
        {
            if (StorageEmulatorHelper.CheckEmulator())
            {
                Assert.ThrowsException<ArgumentNullException>(() =>
                    new BlobsStorage(null, ContainerName));

                Assert.ThrowsException<ArgumentNullException>(() =>
                    new BlobsStorage(ConnectionString, null));

                Assert.ThrowsException<ArgumentNullException>(() =>
                    new BlobsStorage(string.Empty, ContainerName));

                Assert.ThrowsException<ArgumentNullException>(() =>
                    new BlobsStorage(ConnectionString, string.Empty));
            }
        }

        protected override IStorage GetStorage(bool typeNameHandlingNone = false)
        {
            if (typeNameHandlingNone)
            {
                return new BlobsStorage(ConnectionString, ContainerName, new JsonSerializer() { TypeNameHandling = TypeNameHandling.None });
            }
            else
            {
                return new BlobsStorage(ConnectionString, ContainerName);
            }
        }
    }
}
