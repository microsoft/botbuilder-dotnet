using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Azure;
using Microsoft.Bot.Builder.Tests;
using System;
using System.Threading.Tasks;
using System.IO;

namespace Microsoft.Bot.Builder.Azure.Tests
{
    [TestClass]
    [TestCategory("Storage")]
    [TestCategory("Storage - Azure Tables")]
    public class TableStorageTests : Storage_BaseTests, IStorageTests
    {
        private IStorage storage;

        public TestContext TestContext { get; set; }

        private static TestContext _testContext;

        private static string emulatorPath = Environment.ExpandEnvironmentVariables(@"%ProgramFiles(x86)%\Microsoft SDKs\Azure\Storage Emulator\azurestorageemulator2.exe");
        private const string noEmulatorMessage = "This test requires Azure Storage Emulator! go to https://go.microsoft.com/fwlink/?LinkId=717179 to download and install.";

        public bool hasStorageEmulator()
        {
            return File.Exists(emulatorPath);
        }

        [ClassInitialize]
        public static void SetupTests(TestContext testContext)
        {
            _testContext = testContext;
        }

        [TestInitialize]
        public void TestInit()
        {
            if (hasStorageEmulator())
                storage = new AzureTableStorage("UseDevelopmentStorage=true", TestContext.TestName + TestContext.GetHashCode().ToString("x"));
        }

        [TestCleanup]
        public async Task TestCleanUp()
        {
            if (storage != null)
            {
                AzureTableStorage store = (AzureTableStorage)storage;
                await store.Table.DeleteIfExistsAsync();
            }
        }

        // NOTE: THESE TESTS REQUIRE THAT THE AZURE STORAGE EMULATOR IS INSTALLED AND STARTED !!!!!!!!!!!!!!!!!
        [TestMethod]
        public async Task CreateObjectTest()
        {
            Assert.IsTrue(hasStorageEmulator(), noEmulatorMessage);
            await base._createObjectTest(storage);
        }

        // NOTE: THESE TESTS REQUIRE THAT THE AZURE STORAGE EMULATOR IS INSTALLED AND STARTED !!!!!!!!!!!!!!!!!
        [TestMethod]
        public async Task ReadUnknownTest()
        {
            Assert.IsTrue(hasStorageEmulator(), noEmulatorMessage);
            await base._readUnknownTest(storage);
        }

        // NOTE: THESE TESTS REQUIRE THAT THE AZURE STORAGE EMULATOR IS INSTALLED AND STARTED !!!!!!!!!!!!!!!!!
        [TestMethod]
        public async Task UpdateObjectTest()
        {
            Assert.IsTrue(hasStorageEmulator(), noEmulatorMessage);
            await base._updateObjectTest(storage);
        }

        // NOTE: THESE TESTS REQUIRE THAT THE AZURE STORAGE EMULATOR IS INSTALLED AND STARTED !!!!!!!!!!!!!!!!!
        [TestMethod]
        public async Task DeleteObjectTest()
        {
            Assert.IsTrue(hasStorageEmulator(), noEmulatorMessage);
            await base._deleteObjectTest(storage);
        }

        // NOTE: THESE TESTS REQUIRE THAT THE AZURE STORAGE EMULATOR IS INSTALLED AND STARTED !!!!!!!!!!!!!!!!!
        [TestMethod]
        public async Task HandleCrazyKeys()
        {
            Assert.IsTrue(hasStorageEmulator(), noEmulatorMessage);
            await base._handleCrazyKeys(storage);
        }
    }
}
