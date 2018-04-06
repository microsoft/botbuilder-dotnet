// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Core.Extensions;
using Microsoft.Bot.Builder.Core.Extensions.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
            if (hasStorageEmulator.Value)
            {
                storage = new AzureTableStorage("UseDevelopmentStorage=true", TestContext.TestName.Replace("_","") + TestContext.GetHashCode().ToString("x"));
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
        public async Task TableStorage_CreateObjectTest()
        {
            if (CheckStorageEmulator())
                await base._createObjectTest(storage);
        }

        // NOTE: THESE TESTS REQUIRE THAT THE AZURE STORAGE EMULATOR IS INSTALLED AND STARTED !!!!!!!!!!!!!!!!!
        [TestMethod]
        public async Task TableStorage_ReadUnknownTest()
        {
            if (CheckStorageEmulator())
                await base._readUnknownTest(storage);
        }

        // NOTE: THESE TESTS REQUIRE THAT THE AZURE STORAGE EMULATOR IS INSTALLED AND STARTED !!!!!!!!!!!!!!!!!
        [TestMethod]
        public async Task TableStorage_UpdateObjectTest()
        {
            if (CheckStorageEmulator())
                await base._updateObjectTest(storage);
        }

        // NOTE: THESE TESTS REQUIRE THAT THE AZURE STORAGE EMULATOR IS INSTALLED AND STARTED !!!!!!!!!!!!!!!!!
        [TestMethod]
        public async Task TableStorage_DeleteObjectTest()
        {
            if (CheckStorageEmulator())
                await base._deleteObjectTest(storage);
        }

        // NOTE: THESE TESTS REQUIRE THAT THE AZURE STORAGE EMULATOR IS INSTALLED AND STARTED !!!!!!!!!!!!!!!!!
        [TestMethod]
        public async Task TableStorage_HandleCrazyKeys()
        {
            if (CheckStorageEmulator())
                await base._handleCrazyKeys(storage);
        }

        // NOTE: THESE TESTS REQUIRE THAT THE AZURE STORAGE EMULATOR IS INSTALLED AND STARTED !!!!!!!!!!!!!!!!!
        [TestMethod]
        public async Task TableStorage_TypedSerialization()
        {
            if (CheckStorageEmulator())
                await base._typedSerialization(this.storage);
        }

        // NOTE: THESE TESTS REQUIRE THAT THE AZURE STORAGE EMULATOR IS INSTALLED AND STARTED !!!!!!!!!!!!!!!!!
        // Save a larger than 64KB object into Table Storage
        // </summary>
        [TestMethod]
        public async Task TableStorage_CreateLargerObjectTest()
        {
            if (CheckStorageEmulator())
            {
                var bigString = RandomString(74000);
                var storeItems = new StoreItems();
                storeItems.Add("BigObject", new
                {
                    Text = bigString
                });

                await storage.Write(storeItems);

                var storedItems = await storage.Read("BigObject");
                Assert.AreEqual(bigString, storedItems.Get<dynamic>("BigObject").Text);
            }
        }

        // NOTE: THESE TESTS REQUIRE THAT THE AZURE STORAGE EMULATOR IS INSTALLED AND STARTED !!!!!!!!!!!!!!!!!
        // Save a larger than 64KB object into Table Storage
        // </summary>
        [TestMethod]
        public async Task TableStorage_UpdateLargeObjectWithEtagTest()
        {
            if (CheckStorageEmulator())
            {
                var bigString = RandomString(74000);
                var storeItems = new StoreItems();
                storeItems.Add("BigObjectWithEtag", new BigPocoItem
                {
                    Text = bigString
                });

                await storage.Write(storeItems);

                // Read and Update
                var storedItems = await storage.Read("BigObjectWithEtag");

                var biggerString = RandomString(100000);
                storedItems.Get<dynamic>("BigObjectWithEtag").Text = biggerString;
                await storage.Write(storedItems);

                // Assert updated correctly
                storedItems = await storage.Read("BigObjectWithEtag");
                Assert.AreEqual(biggerString, storedItems.Get<dynamic>("BigObjectWithEtag").Text);
            }
        }

        // NOTE: THESE TESTS REQUIRE THAT THE AZURE STORAGE EMULATOR IS INSTALLED AND STARTED !!!!!!!!!!!!!!!!!
        // Save a larger than 64KB object into Table Storage
        // </summary>
        [TestMethod]
        public async Task TableStorage_DeleteLargerObjectTest()
        {
            if (CheckStorageEmulator())
            {
                var bigString = RandomString(74000);
                var storeItems = new StoreItems();
                storeItems.Add("BigObject", new BigPocoItem
                {
                    Text = bigString
                });

                await storage.Write(storeItems);

                await storage.Delete("BigObject");

                storeItems = await storage.Read("BigObject");
                Assert.IsFalse(storeItems.ContainsKey("BigObject"));
            }
        }

        // NOTE: THESE TESTS REQUIRE THAT THE AZURE STORAGE EMULATOR IS INSTALLED AND STARTED !!!!!!!!!!!!!!!!!
        // Save a larger than 64KB object into Table Storage, then a smaller one
        // </summary>
        [TestMethod]
        public async Task TableStorage_UpdateLargeObjectWithSmaller()
        {
            if (CheckStorageEmulator())
            {
                var bigString = RandomString(74000);
                var newString = "Hello World!";

                var storeItems = new StoreItems();
                storeItems.Add("BigToSmallObject", new BigPocoItem
                {
                    Text = bigString
                });

                await storage.Write(storeItems);
                var storedItem = await storage.Read("BigToSmallObject");
                storedItem["BigToSmallObject"].Text = newString;
                await storage.Write(storedItem);

                storedItem = await storage.Read("BigToSmallObject");

                Assert.AreEqual(newString, storedItem.Get<dynamic>("BigToSmallObject").Text);
            }
        }

        [TestMethod]
        public void TableStorage_SplitBigItemIntoChunks()
        {
            var bigString = RandomString(74000);
            var obj = new { Text = bigString };

            var splitter = new AzureTableStorage.StoreItemContainer("test", obj);
            var chunks = splitter.Split();

            Assert.AreEqual(3, chunks.Count());
        }

        [TestMethod]
        public void TableStorage_JoinChunksIntoBiggerItem()
        {
            var bigString = RandomString(74000);
            var obj = new { Text = bigString };

            var splitter = new AzureTableStorage.StoreItemContainer("test", obj);
            var chunks = splitter.Split();

            var joined = AzureTableStorage.StoreItemContainer.Join(chunks);

            Assert.AreEqual(bigString, ((dynamic)joined.Object).Text);
        }

        [TestMethod]
        public void TableStorage_JoinChunksPreservesOriginalKey()
        {
            string key = "!@#$%^&*()~/\\><,.?';\"`~";
            var bigString = RandomString(74000);
            var obj = new { Text = bigString };

            var splitter = new AzureTableStorage.StoreItemContainer(key, obj);
            var chunks = splitter.Split();

            var joined = AzureTableStorage.StoreItemContainer.Join(chunks);

            Assert.AreEqual(key, joined.Key);
        }

        private static string RandomString(int length)
        {
            var random = new Random();
            const string pool = "abcdefghijklmnopqrstuvwxyz0123456789";
            var chars = Enumerable.Range(0, length)
                .Select(x => pool[random.Next(0, pool.Length)]);
            return new string(chars.ToArray());
        }

        public class BigPocoItem : IStoreItem
        {
            public string eTag { get; set; }

            public string Text { get; set; }
        }
    }
}
