using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Azure;
using Microsoft.Bot.Builder.Tests;
using System;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;

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
            if (hasStorageEmulator.Value)
            {
                storage = new AzureTableStorage("UseDevelopmentStorage=true", TestContext.TestName + TestContext.GetHashCode().ToString("x"));
            }
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

        public bool CheckStorageEmulator()
        {
            if (!hasStorageEmulator.Value)
                Debug.WriteLine(noEmulatorMessage);
            if (Debugger.IsAttached)
                Assert.IsTrue(hasStorageEmulator.Value, noEmulatorMessage);
            return hasStorageEmulator.Value;
        }

        // NOTE: THESE TESTS REQUIRE THAT THE AZURE STORAGE EMULATOR IS INSTALLED AND STARTED !!!!!!!!!!!!!!!!!
        [TestMethod]
        public async Task CreateObjectTest()
        {
            if (CheckStorageEmulator())
                await base._createObjectTest(storage);
        }

        // NOTE: THESE TESTS REQUIRE THAT THE AZURE STORAGE EMULATOR IS INSTALLED AND STARTED !!!!!!!!!!!!!!!!!
        [TestMethod]
        public async Task ReadUnknownTest()
        {
            if (CheckStorageEmulator())
                await base._readUnknownTest(storage);
        }

        // NOTE: THESE TESTS REQUIRE THAT THE AZURE STORAGE EMULATOR IS INSTALLED AND STARTED !!!!!!!!!!!!!!!!!
        [TestMethod]
        public async Task UpdateObjectTest()
        {
            if (CheckStorageEmulator())
                await base._updateObjectTest(storage);
        }

        // NOTE: THESE TESTS REQUIRE THAT THE AZURE STORAGE EMULATOR IS INSTALLED AND STARTED !!!!!!!!!!!!!!!!!
        [TestMethod]
        public async Task DeleteObjectTest()
        {
            if (CheckStorageEmulator())
                await base._deleteObjectTest(storage);
        }

        // NOTE: THESE TESTS REQUIRE THAT THE AZURE STORAGE EMULATOR IS INSTALLED AND STARTED !!!!!!!!!!!!!!!!!
        [TestMethod]
        public async Task HandleCrazyKeys()
        {
            if (CheckStorageEmulator())
                await base._handleCrazyKeys(storage);
        }

        [TestMethod]
        public async Task TypedSerialization()
        {
            if (CheckStorageEmulator())
                await base._typedSerialization(this.storage);
        }
    }
}
