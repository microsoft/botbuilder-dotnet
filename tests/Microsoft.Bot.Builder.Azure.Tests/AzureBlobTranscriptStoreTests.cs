// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

// These tests require Azure Storage Emulator v5.7
// The emulator must be installed at this path C:\Program Files (x86)\Microsoft SDKs\Azure\Storage Emulator\AzureStorageEmulator.exe
// More info: https://docs.microsoft.com/azure/storage/common/storage-use-emulator
namespace Microsoft.Bot.Builder.Azure.Tests
{
    [Trait("TestCategory", "Storage")]
    [Trait("TestCategory", "Storage - BlobTranscripts")]
    public class AzureBlobTranscriptStoreTests : TranscriptStoreBaseTests, IAsyncLifetime
    {
        private readonly string _testName;

        public AzureBlobTranscriptStoreTests(ITestOutputHelper testOutputHelper)
        {
            var helper = (TestOutputHelper)testOutputHelper;

            var test = (ITest)helper.GetType().GetField("test", BindingFlags.NonPublic | BindingFlags.Instance)
                .GetValue(helper);

            _testName = test.TestCase.TestMethod.Method.Name;

            if (StorageEmulatorHelper.CheckEmulator())
            {
                CloudStorageAccount.Parse(BlobStorageEmulatorConnectionString)
                    .CreateCloudBlobClient()
                    .GetContainerReference(ContainerName)
                    .DeleteIfExistsAsync().ConfigureAwait(false);
            }
        }

        protected override string ContainerName => $"blobtranscript{_testName.ToLower()}";

        protected override ITranscriptStore TranscriptStore => new AzureBlobTranscriptStore(BlobStorageEmulatorConnectionString, ContainerName);

        public Task InitializeAsync()
        {
            return Task.CompletedTask;
        }

        public async Task DisposeAsync()
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
        [Fact]
        public async Task LongIdAddTest()
        {
            if (StorageEmulatorHelper.CheckEmulator())
            {
                try
                {
                    var a = CreateActivity(0, 0, LongId);

                    await TranscriptStore.LogActivityAsync(a);
                    
                    throw new XunitException("Should have thrown an error");
                }
                catch (StorageException)
                {
                    return;
                }
            }
        }

        // These tests require Azure Storage Emulator v5.7
        [Fact]
        public void BlobTranscriptParamTest()
        {
            if (StorageEmulatorHelper.CheckEmulator())
            {
                Assert.Throws<FormatException>(() => new AzureBlobTranscriptStore("123", ContainerName));

                Assert.Throws<ArgumentNullException>(() =>
                    new AzureBlobTranscriptStore((CloudStorageAccount)null, ContainerName));

                Assert.Throws<ArgumentNullException>(() =>
                    new AzureBlobTranscriptStore((string)null, ContainerName));

                Assert.Throws<ArgumentNullException>(() =>
                    new AzureBlobTranscriptStore((CloudStorageAccount)null, null));

                Assert.Throws<ArgumentNullException>(() => new AzureBlobTranscriptStore((string)null, null));
            }
        }
    }
}
