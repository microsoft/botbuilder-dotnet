// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

// These tests require Azure Storage Emulator v5.7
// The emulator must be installed at this path C:\Program Files (x86)\Microsoft SDKs\Azure\Storage Emulator\AzureStorageEmulator.exe
// More info: https://docs.microsoft.com/azure/storage/common/storage-use-emulator
namespace Microsoft.Bot.Builder.Azure.Tests
{
    [TestClass]
    [TestCategory("Storage")]
    [TestCategory("Storage - BlobTranscripts")]
    public class AzureBlobTranscriptStoreTests : TranscriptStoreBaseTests
    {
        public TestContext TestContext { get; set; }

        protected override string ContainerName
        {
            get { return $"blobtranscript{TestContext.TestName.ToLower()}"; }
        }

        protected override ITranscriptStore TranscriptStore
        {
            get { return new AzureBlobTranscriptStore(BlobStorageEmulatorConnectionString, ContainerName); }
        }

        [TestInitialize]
        public async Task Init()
        {
            if (StorageEmulatorHelper.CheckEmulator())
            {
                await CloudStorageAccount.Parse(BlobStorageEmulatorConnectionString)
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
                await CloudStorageAccount.Parse(BlobStorageEmulatorConnectionString)
                    .CreateCloudBlobClient()
                    .GetContainerReference(ContainerName)
                    .DeleteIfExistsAsync().ConfigureAwait(false);
            }
        }

        // These tests require Azure Storage Emulator v5.7
        [TestMethod]
        public async Task LongIdAddTest()
        {
            if (StorageEmulatorHelper.CheckEmulator())
            {
                try
                {
                    var a = CreateActivity(0, 0, LongId);

                    await TranscriptStore.LogActivityAsync(a);
                    Assert.Fail("Should have thrown ");
                }
                catch (StorageException)
                {
                    return;
                }

                Assert.Fail("Should have thrown ");
            }
        }

        // These tests require Azure Storage Emulator v5.7
        [TestMethod]
        public void BlobTranscriptParamTest()
        {
            if (StorageEmulatorHelper.CheckEmulator())
            {
                Assert.ThrowsException<FormatException>(() => new AzureBlobTranscriptStore("123", ContainerName));

                Assert.ThrowsException<ArgumentNullException>(() =>
                    new AzureBlobTranscriptStore((CloudStorageAccount)null, ContainerName));

                Assert.ThrowsException<ArgumentNullException>(() =>
                    new AzureBlobTranscriptStore((string)null, ContainerName));

                Assert.ThrowsException<ArgumentNullException>(() =>
                    new AzureBlobTranscriptStore((CloudStorageAccount)null, null));

                Assert.ThrowsException<ArgumentNullException>(() => new AzureBlobTranscriptStore((string)null, null));
            }
        }
    }
}
