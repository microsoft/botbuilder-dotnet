using System;
using System.Dynamic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Storage;
using System.IO;

namespace Microsoft.Bot.Builder.Tests
{
    [TestClass]
    [TestCategory("Storage")]
    [TestCategory("Storage - Memory")]
    public class Storage_MemoryTests : Storage_BaseTests, IStorageTests
    {
        private IStorage storage;

        public Storage_MemoryTests() { }

        [TestInitialize]
        public void initialize()
        {
            storage = new MemoryStorage();
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

        [TestMethod]
        public async Task TypedSerialization()
        {
            await base._typedSerialization(this.storage);
        }
    }

    public class TestItem : StoreItem
    {
        public string Id { get; set; }

        public int Count { get; set; }
    }

    public interface IStorageTests
    {
        Task ReadUnknownTest();

        Task CreateObjectTest();

        Task HandleCrazyKeys();

        Task UpdateObjectTest();

        Task DeleteObjectTest();
    }
}
