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
    [TestCategory("Storage - Azure Tables")]
    public class TableStorageTests : StorageBaseTests
    {
        private IStorage storage;

        public TestContext TestContext { get; set; }

        private static TestContext _testContext;

        private static string emulatorPath = Environment.ExpandEnvironmentVariables(@"%ProgramFiles(x86)%\Microsoft SDKs\Azure\Storage Emulator\azurestorageemulator.exe");
        private const string noEmulatorMessage = "This test requires Azure Storage Emulator! go to https://go.microsoft.com/fwlink/?LinkId=717179 to download and install.";

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

        [TestInitialize]
        public void TestInit()
        {
            var cloudStorageAccount = (hasStorageEmulator.Value) ? CloudStorageAccount.DevelopmentStorageAccount : null;
            var connectionString = Environment.GetEnvironmentVariable("STORAGECONNECTIONSTRING") ;
            if (!String.IsNullOrEmpty(connectionString))
                cloudStorageAccount = CloudStorageAccount.Parse(connectionString);

            if (cloudStorageAccount != null)
            {
                storage = new AzureTableStorage(cloudStorageAccount, TestContext.TestName.Replace("_","") + TestContext.GetHashCode().ToString("x"));
            }
        }

        [TestCleanup]
        public async Task TableStorage_TestCleanUp()
        {
            if (storage != null)
            {
                AzureTableStorage store = (AzureTableStorage)storage;
                await store.Table.DeleteIfExistsAsync();
            }
        }

        public bool HasStorage()
        {
            return storage != null;
        }

        // NOTE: THESE TESTS REQUIRE THAT THE AZURE STORAGE EMULATOR IS INSTALLED AND STARTED !!!!!!!!!!!!!!!!!
        [TestMethod]
        public async Task TableStorage_CreateObjectTest()
        {
            if (HasStorage())
                await base._createObjectTest(storage);
        }

        // NOTE: THESE TESTS REQUIRE THAT THE AZURE STORAGE EMULATOR IS INSTALLED AND STARTED !!!!!!!!!!!!!!!!!
        [TestMethod]
        public async Task TableStorage_ReadUnknownTest()
        {
            if (HasStorage())
                await base._readUnknownTest(storage);
        }

        // NOTE: THESE TESTS REQUIRE THAT THE AZURE STORAGE EMULATOR IS INSTALLED AND STARTED !!!!!!!!!!!!!!!!!
        [TestMethod]
        public async Task TableStorage_UpdateObjectTest()
        {
            if (HasStorage())
                await base._updateObjectTest(storage);
        }

        // NOTE: THESE TESTS REQUIRE THAT THE AZURE STORAGE EMULATOR IS INSTALLED AND STARTED !!!!!!!!!!!!!!!!!
        [TestMethod]
        public async Task TableStorage_DeleteObjectTest()
        {
            if (HasStorage())
                await base._deleteObjectTest(storage);
        }

        // NOTE: THESE TESTS REQUIRE THAT THE AZURE STORAGE EMULATOR IS INSTALLED AND STARTED !!!!!!!!!!!!!!!!!
        [TestMethod]
        public async Task TableStorage_HandleCrazyKeys()
        {
            if (HasStorage())
                await base._handleCrazyKeys(storage);
        }
    }
}
