// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
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
            Assert.IsNull(result["unknown"], "unknown should be null");
        }

        protected async Task _createObjectTest(IStorage storage)
        {
            dynamic storeItems = new StoreItems();

            storeItems.createPoco = new PocoItem() { Id = "1" };

            storeItems.createPocoStoreItem = new PocoStoreItem() { Id = "2" };

            storeItems.createStoreItem = new StoreItem();
            storeItems.createStoreItem.Id = "3" ;
            storeItems.createStoreItem.dyno  = "dynamicStuff";

            await storage.Write(storeItems);

            dynamic result = await storage.Read(((StoreItems)storeItems).GetDynamicMemberNames().ToArray());

            Assert.IsInstanceOfType(result.createPoco, typeof(PocoItem));
            Assert.IsInstanceOfType(result.createPocoStoreItem, typeof(PocoStoreItem));
            Assert.IsInstanceOfType(result.createStoreItem, typeof(StoreItem));

            Assert.IsNotNull(result.createPoco, "createPoco should not be null");
            Assert.AreEqual(result.createPoco.Id, "1", "createPoco.id should be 1");

            Assert.IsNotNull(result.createPocoStoreItem, "createPocoStoreItem should not be null");
            Assert.AreEqual(result.createPocoStoreItem.Id, "2", "createPocoStoreItem.id should be 2");
            Assert.IsNotNull(result.createPocoStoreItem.eTag, "createPocoStoreItem.eTag  should not be null");

            Assert.IsNotNull(result.createStoreItem, "createStoreItem should not be null");
            Assert.AreEqual(result.createStoreItem.Id, "3", "createStoreItem.id should be 3");
            Assert.IsNotNull(result.createStoreItem.eTag, "CreateStoreItem.eTag should not be null");
            Assert.AreEqual(result.createStoreItem.dyno, "dynamicStuff", "createStoreItem.dyno should be dynoStuff");
        }

        protected async Task _handleCrazyKeys(IStorage storage)
        {
            var storeItems = new StoreItems();
            string key = "!@#$%^&*()~/\\><,.?';\"`~";
            storeItems[key] = new PocoStoreItem() { Id = "1" };

            await storage.Write(storeItems);

            dynamic result = await storage.Read(key);
            Assert.IsNotNull(result[key], $"result['{key}'] should not be null");
            Assert.AreEqual(result[key].Id, "1", "strong .id should be 1");
        }

        public class TypedObject
        {
            public string Name { get; set; }
        }

        protected async Task _typedSerialization(IStorage storage)
        {
            string key = "typed";
            var storeItems = new StoreItems();
            dynamic testItem = new StoreItem();
            testItem.Id = "1";
            testItem.x = new TypedObject() { Name = "test" };
            storeItems[key] = testItem;

            await storage.Write(storeItems);

            dynamic result = await storage.Read(key);
            Assert.IsNotNull(result[key], $"result['{key}'] should not be null");
            Assert.AreEqual(result[key].x.Name, "test", "typed object property should be 'test'");
            Assert.AreEqual(result[key].x.GetType(), typeof(TypedObject), "typed object type should be same");
        }

        protected async Task _updateObjectTest(IStorage storage)
        {
            dynamic storeItems = new StoreItems();
            storeItems.updatePocoItem = new PocoItem() { Id = "1", Count = 1 };
            storeItems.updatePocoStoreItem = new PocoStoreItem() { Id = "1", Count = 1 };
            storeItems.updateStoreItem = new StoreItem();
            storeItems.updateStoreItem.Id = "3";
            storeItems.updateStoreItem.Count = 1;

            //first write should work
            await storage.Write(storeItems);

            dynamic result = await storage.Read(((StoreItems)storeItems).GetDynamicMemberNames().ToArray());
            Assert.IsNotNull(result.updatePocoStoreItem.eTag, "updatePocoItem.eTag  should not be null");
            Assert.IsNotNull(result.updateStoreItem.eTag, "updateStoreItem.eTag should not be null");

            // 2nd write should work, because we have new etag, or no etag
            result.updatePocoItem.Count++;
            result.updatePocoStoreItem.Count++;
            result.updateStoreItem.Count++;
            await storage.Write(result);

            dynamic result2 = await storage.Read(((StoreItems)storeItems).GetDynamicMemberNames().ToArray());
            Assert.IsNotNull(result2.updatePocoStoreItem.eTag, "updatePocoItem.eTag  should not be null");
            Assert.IsNotNull(result2.updateStoreItem.eTag, "updateStoreItem.eTag should not be null");
            Assert.AreNotEqual(result.updatePocoStoreItem.eTag, result2.updatePocoStoreItem.eTag, "updatePocoItem.eTag  should not be different");
            Assert.AreNotEqual(result.updateStoreItem.eTag, result2.updateStoreItem.eTag, "updateStoreItem.eTag  should not be different");
            Assert.AreEqual(result2.updatePocoItem.Count, 2, "updatePocoItem.Count should be 2");
            Assert.AreEqual(result2.updatePocoStoreItem.Count, 2, "updatePocoStoreItem.Count should be 2");
            Assert.AreEqual(result2.updateStoreItem.Count, 2, "updateStoreItem.Count should be 2");

            // write with old etag should succeed for updatePocoItem, but fail for the other 2
            try
            {
                dynamic storeItemsUpdate = new StoreItems();
                storeItemsUpdate.updatePocoItem = result.updatePocoItem;
                storeItemsUpdate.updatePocoItem.Count++;
                await storage.Write(storeItemsUpdate);
            }
            catch
            {
                Assert.Fail("Should not throw exception on write with pocoItem");
            }

            try
            {
                dynamic storeItemsUpdate = new StoreItems();
                storeItemsUpdate.updatePocoStoreItem = result.updatePocoStoreItem;
                storeItemsUpdate.updatePocoStoreItem.Count++;
                await storage.Write(storeItemsUpdate);
                Assert.Fail("Should not throw exception on write with pocoStoreItem because of old etag");
            }
            catch
            {
            }
            try
            {
                dynamic storeItemsUpdate = new StoreItems();
                storeItemsUpdate.updateStoreItem = result.updateStoreItem;
                storeItemsUpdate.updateStoreItem.Count++;
                await storage.Write(storeItemsUpdate);
                Assert.Fail("Should not throw exception on write with StoreItem because of old etag");
            }
            catch
            {
            }

            dynamic result3 = await storage.Read(((StoreItems)storeItems).GetDynamicMemberNames().ToArray());
            Assert.AreEqual(result3.updatePocoItem.Count, 3, "updatePocoItem.Count should be 3");
            Assert.AreEqual(result3.updatePocoStoreItem.Count, 2, "updatePocoStoreItem.Count should be 2");
            Assert.AreEqual(result3.updateStoreItem.Count, 2, "updateStoreItem.Count should be 2");

            // write with wildcard etag should work
            result3.updatePocoItem.Count = 100;
            result3.updatePocoStoreItem.Count = 100;
            result3.updatePocoStoreItem.eTag = "*";
            result3.updateStoreItem.Count = 100;
            result3.updateStoreItem.eTag = "*";
            await storage.Write(result3);

            dynamic result4 = await storage.Read(((StoreItems)storeItems).GetDynamicMemberNames().ToArray());
            Assert.AreEqual(result3.updatePocoItem.Count, 100, "updatePocoItem.Count should be 100");
            Assert.AreEqual(result3.updatePocoStoreItem.Count, 100, "updatePocoStoreItem.Count should be 100");
            Assert.AreEqual(result3.updateStoreItem.Count, 100, "updateStoreItem.Count should be 100");

            // write with empty etag should not work
            try
            {
                dynamic storeItemsUpdate = new StoreItems();
                storeItemsUpdate.updatePocoStoreItem = FlexObject.Clone(result4.updatePocoStoreItem);
                storeItemsUpdate.updatePocoStoreItem.eTag = "";
                await storage.Write(result);
                Assert.Fail("Should not throw exception on write with pocoStoreItem because of empty etag");
            }
            catch
            {
            }
            try
            {
                dynamic storeItemsUpdate = new StoreItems();
                storeItemsUpdate.updateStoreItem = FlexObject.Clone(result4.updateStoreItem);
                storeItemsUpdate.updateStoreItem.eTag = "";
                await storage.Write(result);
                Assert.Fail("Should not throw exception on write with storeItem because of empty etag");
            }
            catch
            {
            }

            dynamic result5 = await storage.Read(((StoreItems)storeItems).GetDynamicMemberNames().ToArray());
            Assert.AreEqual(result3.updatePocoItem.Count, 100, "updatePocoItem.Count should be 100");
            Assert.AreEqual(result3.updatePocoStoreItem.Count, 100, "updatePocoStoreItem.Count should be 100");
            Assert.AreEqual(result3.updateStoreItem.Count, 100, "updateStoreItem.Count should be 100");
        }

        protected async Task _deleteObjectTest(IStorage storage)
        {
            dynamic storeItems = new StoreItems();
            storeItems.delete1 = new PocoStoreItem() { Id = "1", Count = 1 };

            //first write should work
            await storage.Write(storeItems);

            dynamic result = await storage.Read("delete1");
            Assert.IsTrue(!String.IsNullOrEmpty(result.delete1.eTag), "etag should be set");
            Assert.AreEqual(result.delete1.Count, 1, "count should be 1");

            await storage.Delete("delete1");

            StoreItems result2 = await storage.Read("delete1");
            Assert.IsNull(result2["delete1"], "delete1 should be null");
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
