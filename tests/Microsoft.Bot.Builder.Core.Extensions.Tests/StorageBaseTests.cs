// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Core.Extensions.Tests
{
    public class StorageBaseTests
    {
        protected async Task _readUnknownTest(IStorage storage)
        {
            var result = await storage.Read(new[] { "unknown" });
            Assert.IsNotNull(result, "result should not be null");
            Assert.IsNull(result.FirstOrDefault(e => e.Key == "unknown").Value, "\"unknown\" key should have returned no value");
        }

        protected async Task _createObjectTest(IStorage storage)
        {
            var storeItems = new Dictionary<string, object>
            {
                ["createPoco"] = new PocoItem() { Id = "1" },
                ["createPocoStoreItem"] = new PocoStoreItem() { Id = "2" },
            };

            await storage.Write(storeItems);

            var readStoreItems = new Dictionary<string, object>(await storage.Read(storeItems.Keys.ToArray()));

            Assert.IsInstanceOfType(readStoreItems["createPoco"], typeof(PocoItem));
            Assert.IsInstanceOfType(readStoreItems["createPocoStoreItem"], typeof(PocoStoreItem));

            var createPoco = readStoreItems["createPoco"] as PocoItem;

            Assert.IsNotNull(createPoco, "createPoco should not be null");
            Assert.AreEqual(createPoco.Id, "1", "createPoco.id should be 1");

            var createPocoStoreItem = readStoreItems["createPocoStoreItem"] as PocoStoreItem;

            Assert.IsNotNull(createPocoStoreItem, "createPocoStoreItem should not be null");
            Assert.AreEqual(createPocoStoreItem.Id, "2", "createPocoStoreItem.id should be 2");
            Assert.IsNotNull(createPocoStoreItem.eTag, "createPocoStoreItem.eTag  should not be null");
        }

        protected async Task _handleCrazyKeys(IStorage storage)
        {
            var key = "!@#$%^&*()~/\\><,.?';\"`~";
            var storeItem = new PocoStoreItem() { Id = "1" };

            await storage.Write(new[] { new KeyValuePair<string, object>(key, storeItem) });

            var storeItems = await storage.Read(key);

            storeItem = storeItems.FirstOrDefault(si => si.Key == key).Value as PocoStoreItem;

            Assert.IsNotNull(storeItem);
            Assert.AreEqual("1", storeItem.Id);
        }

        public class TypedObject
        {
            public string Name { get; set; }
        }

        protected async Task _updateObjectTest(IStorage storage)
        {
            var originalPocoItem = new PocoItem() { Id = "1", Count = 1 };
            var originalPocoStoreItem = new PocoStoreItem() { Id = "1", Count = 1 };

            // first write should work
            await storage.Write(new [] 
            {
                new KeyValuePair<string, object>("pocoItem", originalPocoItem),
                new KeyValuePair<string, object>("pocoStoreItem", originalPocoStoreItem )
            });

            var loadedStoreItems = new Dictionary<string, object>(await storage.Read("pocoItem", "pocoStoreItem"));

            var updatePocoItem = loadedStoreItems["pocoItem"] as PocoItem;
            var updatePocoStoreItem = loadedStoreItems["pocoStoreItem"] as PocoStoreItem;
            Assert.IsNotNull(updatePocoStoreItem.eTag, "updatePocoItem.eTag  should not be null");

            // 2nd write should work, because we have new etag, or no etag
            updatePocoItem.Count++;
            updatePocoStoreItem.Count++;

            await storage.Write(loadedStoreItems);

            var reloadedStoreItems = new Dictionary<string, object>(await storage.Read("pocoItem", "pocoStoreItem"));

            var reloeadedUpdatePocoItem = reloadedStoreItems["pocoItem"] as PocoItem;
            var reloadedUpdatePocoStoreItem = reloadedStoreItems["pocoStoreItem"] as PocoStoreItem;

            Assert.IsNotNull(reloadedUpdatePocoStoreItem.eTag, "updatePocoItem.eTag  should not be null");
            Assert.AreNotEqual(updatePocoStoreItem.eTag, reloadedUpdatePocoStoreItem.eTag, "updatePocoItem.eTag  should not be different");
            Assert.AreEqual(2, reloeadedUpdatePocoItem.Count, "updatePocoItem.Count should be 2");
            Assert.AreEqual(2, reloadedUpdatePocoStoreItem.Count, "updatePocoStoreItem.Count should be 2");

            // write with old etag should succeed for non-storeitem
            try
            {
                updatePocoItem.Count = 123;

                await storage.Write(new[]
                {
                    new KeyValuePair<string, object>("pocoItem", updatePocoItem)
                });
            }
            catch
            {
                Assert.Fail("Should not throw exception on write with pocoItem");
            }

            // write with old etag should FAIL for storeitem
            try
            {
                updatePocoStoreItem.Count = 123;

                await storage.Write(new[]
                {
                    new KeyValuePair<string, object>("pocoStoreItem", updatePocoStoreItem)
                });

                Assert.Fail("Should have thrown exception on write with store item because of old etag");
            }
            catch
            {
            }

            var reloadedStoreItems2 = new Dictionary<string, object>(await storage.Read("pocoItem", "pocoStoreItem"));

            var reloadedPocoItem2 = reloadedStoreItems2["pocoItem"] as PocoItem;
            var reloadedPocoStoreItem2 = reloadedStoreItems2["pocoStoreItem"] as PocoStoreItem;

            Assert.AreEqual(123, reloadedPocoItem2.Count);
            Assert.AreEqual(2, reloadedPocoStoreItem2.Count);

            // write with wildcard etag should work
            reloadedPocoItem2.Count = 100;
            reloadedPocoStoreItem2.Count = 100;
            reloadedPocoStoreItem2.eTag = "*";

            await storage.Write(new[]
            {
                new KeyValuePair<string, object>("pocoItem", reloadedPocoItem2),
                new KeyValuePair<string, object>("pocoStoreItem", reloadedPocoStoreItem2)
            });

            var reloadedStoreItems3 = new Dictionary<string, object>(await storage.Read("pocoItem", "pocoStoreItem"));

            Assert.AreEqual(100, (reloadedStoreItems3["pocoItem"] as PocoItem).Count);
            Assert.AreEqual(100, (reloadedStoreItems3["pocoStoreItem"] as PocoStoreItem).Count);

            // write with empty etag should not work
            try
            {
                var reloadedStoreItem4 = (await storage.Read("pocoStoreItem")).OfType<PocoStoreItem>().First();

                reloadedStoreItem4.eTag = "";
                await storage.Write(new[]
                {
                    new KeyValuePair<string, object>("pocoStoreItem", reloadedStoreItem4)
                });

                Assert.Fail("Should have thrown exception on write with storeitem because of empty etag");
            }
            catch
            {
            }


            var finalStoreItems = new Dictionary<string, object>(await storage.Read("pocoItem", "pocoStoreItem"));
            Assert.AreEqual(100, (finalStoreItems["pocoItem"] as PocoItem).Count);
            Assert.AreEqual(100, (finalStoreItems["pocoStoreItem"] as PocoStoreItem).Count);
        }

        protected async Task _deleteObjectTest(IStorage storage)
        {
            //first write should work
            await storage.Write(new[] { new KeyValuePair<string, object>("delete1", new PocoStoreItem() { Id = "1", Count = 1 }) });

            var storeItems = await storage.Read("delete1");
            var storeItem = storeItems.First().Value as PocoStoreItem;

            Assert.IsNotNull(storeItem.eTag, "etag should be set");
            Assert.AreEqual(1, storeItem.Count);

            await storage.Delete("delete1");

            var reloadedStoreItems = await storage.Read("delete1");

            Assert.IsFalse(reloadedStoreItems.Any(), "no store item should have been found because it was deleted");
        }
    }

    public class PocoItem
    {
        public string Id { get; set; }

        public int Count { get; set; }
    }

    public class PocoStoreItem : IStoreItem
    {
        public string eTag { get; set; }

        public string Id { get; set; }

        public int Count { get; set; }
    }
}
