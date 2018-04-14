// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Core.Extensions;
using Microsoft.Bot.Builder.Core.Extensions.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.Storage;

namespace Microsoft.Bot.Builder.Azure.Tests
{
    [TestClass]
    [TestCategory("Storage")]
    [TestCategory("Storage - Azure Blob")]
    public class BlobStorageTests : StorageBaseTests
    {
        private IStorage storage;

        public TestContext TestContext { get; set; }

        private static TestContext _testContext;

        private static string emulatorPath = Environment.ExpandEnvironmentVariables(@"%ProgramFiles(x86)%\Microsoft SDKs\Azure\Storage Emulator\azurestorageemulator.exe");
        private const string noEmulatorMessage = "This test requires Azure Storage Emulator! go to https://go.microsoft.com/fwlink/?LinkId=717179 to download and install.";
        private string connectionString = null;
        private static Lazy<bool> hasStorageEmulator = new Lazy<bool>(() =>
        {
            if (File.Exists(emulatorPath))
            {
                Process p = new Process();
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.FileName = emulatorPath;
                p.StartInfo.Arguments = "status";
                p.Start();
                var output = p.StandardOutput.ReadToEnd();
                p.WaitForExit();
                return output.Contains("IsRunning: True");
            }
            return false;
        });

        [ClassInitialize]
        public static void SetupTests(TestContext testContext)
        {
            _testContext = testContext;
        }

        private string _containerName;

        private const string emulatorConnectionString = "UseDevelopmentStorage=true";

        [TestInitialize]
        public void TestInit()
        {
            connectionString = Environment.GetEnvironmentVariable("STORAGECONNECTIONSTRING") ?? emulatorConnectionString;

            if (connectionString != emulatorConnectionString || hasStorageEmulator.Value)
            {
                _containerName = TestContext.TestName.ToLowerInvariant().Replace("_", "") + TestContext.GetHashCode().ToString("x");
                storage = new AzureBlobStorage(connectionString, _containerName);
            }
        }

        [TestCleanup]
        public async Task BlobStorage_TestCleanUp()
        {
            if (storage != null)
            {
                var storageAccount = CloudStorageAccount.Parse(connectionString);
                var blobClient = storageAccount.CreateCloudBlobClient();
                var container = blobClient.GetContainerReference(_containerName);
                await container.DeleteIfExistsAsync();
            }
        }

        public bool HasStorage()
        {
            return storage != null;
        }

        // NOTE: THESE TESTS REQUIRE THAT THE AZURE STORAGE EMULATOR IS INSTALLED AND STARTED !!!!!!!!!!!!!!!!!
        [TestMethod]
        public async Task BlobStorage_CreateObjectTest()
        {
            if (HasStorage())
                await base._createObjectTest(storage);
        }

        // NOTE: THESE TESTS REQUIRE THAT THE AZURE STORAGE EMULATOR IS INSTALLED AND STARTED !!!!!!!!!!!!!!!!!
        [TestMethod]
        public async Task BlobStorage_ReadUnknownTest()
        {
            if (HasStorage())
                await base._readUnknownTest(storage);
        }

        // NOTE: THESE TESTS REQUIRE THAT THE AZURE STORAGE EMULATOR IS INSTALLED AND STARTED !!!!!!!!!!!!!!!!!
        [TestMethod]
        public async Task BlobStorage_UpdateObjectTest()
        {
            if (HasStorage())
                await base._updateObjectTest(storage);
        }

        // NOTE: THESE TESTS REQUIRE THAT THE AZURE STORAGE EMULATOR IS INSTALLED AND STARTED !!!!!!!!!!!!!!!!!
        [TestMethod]
        public async Task BlobStorage_DeleteObjectTest()
        {
            if (HasStorage())
                await base._deleteObjectTest(storage);
        }

        // NOTE: THESE TESTS REQUIRE THAT THE AZURE STORAGE EMULATOR IS INSTALLED AND STARTED !!!!!!!!!!!!!!!!!
        [TestMethod]
        public async Task BlobStorage_HandleCrazyKeys()
        {
            if (HasStorage())
                await base._handleCrazyKeys(storage);
        }

        [TestMethod]
        public async Task BlobStorage_TypedSerialization()
        {
            if (HasStorage())
                await base._typedSerialization(this.storage);
        }
    }
}
