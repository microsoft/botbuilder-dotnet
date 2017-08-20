using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Azure;
using Microsoft.Bot.Builder.Tests;
using System;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Azure.Tests
{
    [TestClass]
    public class TableStorageTests : StorageTests, IStorageTests
    {
        private IStorage storage;

        public TestContext TestContext { get; set; }

        private static TestContext _testContext;

        [ClassInitialize]
        public static void SetupTests(TestContext testContext)
        {
            _testContext = testContext;
        }

        [TestInitialize]
        public void TestInit()
        {
            storage = new AzureTableStorage("UseDevelopmentStorage=true", TestContext.TestName + TestContext.GetHashCode().ToString("x"));
        }

        [TestCleanup]
        public async Task TestCleanUp()
        {
            AzureTableStorage store = (AzureTableStorage)storage;
            await store.Table.DeleteIfExistsAsync();
        }

        [TestMethod]
        public async Task CreateObjectTest()
        {
            await base._createObjectTest(storage);
        }

        [TestMethod]
        public async Task ReadUnknownTest()
        {
            await base._readUnknownTest(storage);
        }

        [TestMethod]
        public async Task UpdateObjectTest()
        {
            await base._updateObjectTest(storage);
        }

        [TestMethod]
        public async Task DeleteObjectTest()
        {
            await base._deleteObjectTest(storage);
        }

        [TestMethod]
        public async Task HandleCrazyKeys()
        {
            await base._handleCrazyKeys(storage);
        }
    }
}
