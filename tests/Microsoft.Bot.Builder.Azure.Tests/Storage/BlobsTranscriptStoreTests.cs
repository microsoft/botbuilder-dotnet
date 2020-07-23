// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Azure.Storage;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// These tests require Azure Storage Emulator v5.7
// The emulator must be installed at this path C:\Program Files (x86)\Microsoft SDKs\Azure\Storage Emulator\AzureStorageEmulator.exe
// More info: https://docs.microsoft.com/azure/storage/common/storage-use-emulator
namespace Microsoft.Bot.Builder.Azure.Tests
{
    [TestClass]
    [TestCategory("Storage")]
    [TestCategory("Storage - BlobsTranscriptStore")]
    public class BlobsTranscriptStoreTests : TranscriptStoreBaseTests
    {
        public TestContext TestContext { get; set; }

        protected override string ContainerName
        {
            get { return $"blobtranscript{TestContext.TestName.ToLower()}"; }
        }

        protected override ITranscriptStore TranscriptStore
        {
            get { return new BlobsTranscriptStore(ConnectionString, ContainerName); }
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
                catch (System.Xml.XmlException xmlEx)
                {
                    // Unfortunately, Azure.Storage.Blobs v12.4.4 currently throws this XmlException for long keys :(
                    if (xmlEx.Message == "'\"' is an unexpected token. Expecting whitespace. Line 1, position 50.")
                    {
                        return;
                    }
                }

                //catch (RequestFailedException ex)
                //when ((HttpStatusCode)ex.Status == HttpStatusCode.PreconditionFailed)
                //{
                //    return;
                //}

                Assert.Fail("Should have thrown ");
            }
        }

        // These tests require Azure Storage Emulator v5.7
        [TestMethod]
        public void BlobTranscriptParamTest()
        {
            if (StorageEmulatorHelper.CheckEmulator())
            {
                Assert.ThrowsException<ArgumentNullException>(() =>
                    new BlobsTranscriptLogger(null, ContainerName));

                Assert.ThrowsException<ArgumentNullException>(() =>
                    new BlobsTranscriptLogger(ConnectionString, null));

                Assert.ThrowsException<ArgumentNullException>(() =>
                    new BlobsTranscriptLogger(string.Empty, ContainerName));

                Assert.ThrowsException<ArgumentNullException>(() =>
                    new BlobsTranscriptLogger(ConnectionString, string.Empty));
            }
        }
    }
}
