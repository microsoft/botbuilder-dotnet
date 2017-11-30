using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Tests
{
    public class Storage_BaseTests
    {
        protected async Task _readUnknownTest(IStorage storage)
        {
            var result = await storage.Read(new[] { "unknown" });
            Assert.IsNotNull(result, "result should not be null");
            Assert.IsNull(result["unknown"], "unknown should be null");
        }

        protected async Task _createObjectTest(IStorage storage)
        {
            var storeItems = new StoreItems();
            storeItems["create1"] = new TestItem() { Id = "1" };
            dynamic newItem2 = new TestItem() { Id = "2" };
            newItem2.dyno = "dynamicStuff";
            storeItems["create2"] = newItem2;

            await storage.Write(storeItems);

            dynamic result = await storage.Read(new string[] { "create1", "create2" });
            Assert.IsNotNull(result.create1, "create1 should not be null");
            Assert.AreEqual(result.create1.Id, "1", "strong create1.id should be 1");
            Assert.IsNotNull(result.create2, "create2 should not be null");
            Assert.AreEqual(result.create2.Id, "2", "create2.id should be 2");
            Assert.AreEqual(result.create2.dyno, "dynamicStuff", "create2.dyno should be dynoStuff");
        }

        protected async Task _handleCrazyKeys(IStorage storage)
        {
            var storeItems = new StoreItems();
            string key = "!@#$%^&*()~/\\><,.?';\"`~";
            storeItems[key] = new TestItem() { Id = "1" };

            await storage.Write(storeItems);

            dynamic result = await storage.Read(key);
            Assert.IsNotNull(result[key], $"result['{key}'] should not be null");
            Assert.AreEqual(result[key].Id, "1", "strong .id should be 1");
        }

        protected async Task _updateObjectTest(IStorage storage)
        {
            dynamic storeItems = new StoreItems();
            storeItems.update = new TestItem() { Id = "1", Count = 1 };

            //first write should work
            await storage.Write(storeItems);

            dynamic result = await storage.Read("update");
            Assert.IsTrue(!String.IsNullOrEmpty(result.update.eTag), "etag should be set");
            Assert.AreEqual(result.update.Count, 1, "count should be 1");

            // 2nd write should work, because we have new etag
            result.update.Count++;
            await storage.Write(result);

            dynamic result2 = await storage.Read("update");
            Assert.IsTrue(!String.IsNullOrEmpty(result2.update.eTag), "etag should be set on second write too");
            Assert.AreNotEqual(result.update.eTag, result2.update.eTag, "etag should be differnt on new write");
            Assert.AreEqual(result2.update.Count, 2, "Count should be 2");

            // write with old etag should fail
            try
            {
                await storage.Write(result);
                Assert.Fail("Should throw exception on write with old etag");
            }
            catch { }

            dynamic result3 = await storage.Read("update");
            Assert.AreEqual(result3.update.Count, 2, "count should still be be two");

            // write with wildcard etag should work
            result3.update.Count = 100;
            result3.update.eTag = "*";
            await storage.Write(result3);

            dynamic result4 = await storage.Read("update");
            Assert.AreEqual(result4.update.Count, 100, "count should be 100");

            // write with empty etag should not work
            result4.update.Count = 200;
            result4.update.eTag = "";
            try
            {
                await storage.Write(result4);
                Assert.Fail("Should throw exception on write with empty etag");
            }
            catch { }

            dynamic result5 = await storage.Read("update");
            Assert.AreEqual(result5.update.Count, 100, "count should be 100");
        }

        protected async Task _deleteObjectTest(IStorage storage)
        {
            dynamic storeItems = new StoreItems();
            storeItems.delete1 = new TestItem() { Id = "1", Count = 1 };

            //first write should work
            await storage.Write(storeItems);

            dynamic result = await storage.Read("delete1");
            Assert.IsTrue(!String.IsNullOrEmpty(result.delete1.eTag), "etag should be set");
            Assert.AreEqual(result.delete1.Count, 1, "count should be 1");

            await storage.Delete("delete1");

            StoreItems result2 = await storage.Read("delete1");
            Assert.IsFalse(result2.ContainsKey("delete1"), "delete1 should be null");
        }
    }
}
