// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Tests
{
    public class StorageBaseTests
    {
        protected async Task _readUnknownTest(IStorage storage)
        {
            var result = await storage.ReadAsync(new[] { "unknown" });
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

            await storage.WriteAsync(storeItems);

            var readStoreItems = new Dictionary<string, object>(await storage.ReadAsync(storeItems.Keys.ToArray()));

            Assert.IsInstanceOfType(readStoreItems["createPoco"], typeof(PocoItem));
            Assert.IsInstanceOfType(readStoreItems["createPocoStoreItem"], typeof(PocoStoreItem));

            var createPoco = readStoreItems["createPoco"] as PocoItem;

            Assert.IsNotNull(createPoco, "createPoco should not be null");
            Assert.AreEqual(createPoco.Id, "1", "createPoco.id should be 1");

            var createPocoStoreItem = readStoreItems["createPocoStoreItem"] as PocoStoreItem;

            Assert.IsNotNull(createPocoStoreItem, "createPocoStoreItem should not be null");
            Assert.AreEqual(createPocoStoreItem.Id, "2", "createPocoStoreItem.id should be 2");
            Assert.IsNotNull(createPocoStoreItem.ETag, "createPocoStoreItem.eTag  should not be null");
        }

        protected async Task _handleCrazyKeys(IStorage storage)
        {
            var key = "!@#$%^&*()~/\\><,.?';\"`~";
            var storeItem = new PocoStoreItem() { Id = "1" };

            var dict = new Dictionary<string, object>() { { key, storeItem } };

            await storage.WriteAsync(dict);

            var storeItems = await storage.ReadAsync(new[] { key });

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
            var dict = new Dictionary<string, object>()
            {
                { "pocoItem", originalPocoItem },
                { "pocoStoreItem", originalPocoStoreItem }
            };

            await storage.WriteAsync(dict);

            var loadedStoreItems = new Dictionary<string, object>(await storage.ReadAsync(new[] { "pocoItem", "pocoStoreItem" }));

            var updatePocoItem = loadedStoreItems["pocoItem"] as PocoItem;
            var updatePocoStoreItem = loadedStoreItems["pocoStoreItem"] as PocoStoreItem;
            Assert.IsNotNull(updatePocoStoreItem.ETag, "updatePocoItem.eTag  should not be null");

            // 2nd write should work, because we have new etag, or no etag
            updatePocoItem.Count++;
            updatePocoStoreItem.Count++;

            await storage.WriteAsync(loadedStoreItems);

            var reloadedStoreItems = new Dictionary<string, object>(await storage.ReadAsync(new[] { "pocoItem", "pocoStoreItem" }));

            var reloeadedUpdatePocoItem = reloadedStoreItems["pocoItem"] as PocoItem;
            var reloadedUpdatePocoStoreItem = reloadedStoreItems["pocoStoreItem"] as PocoStoreItem;

            Assert.IsNotNull(reloadedUpdatePocoStoreItem.ETag, "updatePocoItem.eTag  should not be null");
            Assert.AreNotEqual(updatePocoStoreItem.ETag, reloadedUpdatePocoStoreItem.ETag, "updatePocoItem.eTag  should not be different");
            Assert.AreEqual(2, reloeadedUpdatePocoItem.Count, "updatePocoItem.Count should be 2");
            Assert.AreEqual(2, reloadedUpdatePocoStoreItem.Count, "updatePocoStoreItem.Count should be 2");

            // write with old etag should succeed for non-storeitem
            try
            {
                updatePocoItem.Count = 123;
                
                await storage.WriteAsync(
                    new Dictionary<string, object>() { { "pocoItem", updatePocoItem } }
                );
            }
            catch
            {
                Assert.Fail("Should not throw exception on write with pocoItem");
            }

            // write with old etag should FAIL for storeitem
            try
            {
                updatePocoStoreItem.Count = 123;
                
                await storage.WriteAsync(
                    new Dictionary<string, object>() { { "pocoStoreItem", updatePocoStoreItem } }
                );

                Assert.Fail("Should have thrown exception on write with store item because of old etag");
            }
            catch
            {
            }

            var reloadedStoreItems2 = new Dictionary<string, object>(await storage.ReadAsync(new[] { "pocoItem", "pocoStoreItem" }));

            var reloadedPocoItem2 = reloadedStoreItems2["pocoItem"] as PocoItem;
            var reloadedPocoStoreItem2 = reloadedStoreItems2["pocoStoreItem"] as PocoStoreItem;

            Assert.AreEqual(123, reloadedPocoItem2.Count);
            Assert.AreEqual(2, reloadedPocoStoreItem2.Count);

            // write with wildcard etag should work
            reloadedPocoItem2.Count = 100;
            reloadedPocoStoreItem2.Count = 100;
            reloadedPocoStoreItem2.ETag = "*";

            var wildcardEtagedict = new Dictionary<string, object>() {
                { "pocoItem", reloadedPocoItem2 },
                { "pocoStoreItem", reloadedPocoStoreItem2 }                    
            };

            await storage.WriteAsync(wildcardEtagedict);

            var reloadedStoreItems3 = new Dictionary<string, object>(await storage.ReadAsync(new [] { "pocoItem", "pocoStoreItem" }));

            Assert.AreEqual(100, (reloadedStoreItems3["pocoItem"] as PocoItem).Count);
            Assert.AreEqual(100, (reloadedStoreItems3["pocoStoreItem"] as PocoStoreItem).Count);

            // write with empty etag should not work
            try
            {
                var reloadedStoreItem4 = (await storage.ReadAsync(new [] { "pocoStoreItem" })).OfType<PocoStoreItem>().First();

                reloadedStoreItem4.ETag = string.Empty;
                var dict2 = new Dictionary<string, object>()
                {                
                    { "pocoStoreItem", reloadedStoreItem4 }
                };


                await storage.WriteAsync(dict2);

                Assert.Fail("Should have thrown exception on write with storeitem because of empty etag");
            }
            catch
            {
            }


            var finalStoreItems = new Dictionary<string, object>(await storage.ReadAsync(new[] { "pocoItem", "pocoStoreItem" }));
            Assert.AreEqual(100, (finalStoreItems["pocoItem"] as PocoItem).Count);
            Assert.AreEqual(100, (finalStoreItems["pocoStoreItem"] as PocoStoreItem).Count);
        }

        protected async Task _deleteObjectTest(IStorage storage)
        {
            //first write should work
            var dict = new Dictionary<string, object>()
                {
                    { "delete1", new PocoStoreItem() { Id = "1", Count = 1 } }
                };

            await storage.WriteAsync(dict);

            var storeItems = await storage.ReadAsync(new [] { "delete1" });
            var storeItem = storeItems.First().Value as PocoStoreItem;

            Assert.IsNotNull(storeItem.ETag, "etag should be set");
            Assert.AreEqual(1, storeItem.Count);

            await storage.DeleteAsync(new[] { "delete1" });

            var reloadedStoreItems = await storage.ReadAsync(new[] { "delete1" });

            Assert.IsFalse(reloadedStoreItems.Any(), "no store item should have been found because it was deleted");
        }

        protected async Task _deleteUnknownObjectTest(IStorage storage)
        {
            await storage.DeleteAsync(new[] { "unknown_key" });
        }

        protected async Task _batchCreateObjectTest(IStorage storage, long minimumExtraBytes = 0)
        {
            string[] stringArray = null;

            if (minimumExtraBytes > 0)
            {
                // chunks of maximum string size to fill the extra bytes request
                var extraStringCount = (int)(minimumExtraBytes / int.MaxValue);
                stringArray = Enumerable.Range(0, extraStringCount).Select(i => new string('X', int.MaxValue / 2)).ToArray();

                // Append the remaining string size
                stringArray = stringArray.Append(new string('X', (int)(minimumExtraBytes % int.MaxValue) / 2)).ToArray();
            }

            var storeItemsList = new List<Dictionary<string, object>>(new[]
                {
                new Dictionary<string, object> {["createPoco"] = new PocoItem() { Id = "1", Count = 0, ExtraBytes = stringArray }},
                new Dictionary<string, object> {["createPoco"] = new PocoItem() { Id = "1", Count = 1, ExtraBytes = stringArray }},
                new Dictionary<string, object> {["createPoco"] = new PocoItem() { Id = "1", Count = 2, ExtraBytes = stringArray }}
            });

            // TODO: this code as a generic test doesn't make much sense - for now just eliminating the custom exception 
            // Writing large objects in parallel might raise an InvalidOperationException
            try
            {
                await Task.WhenAll(
                    storeItemsList.Select(storeItems =>
                        Task.Run(async () => await storage.WriteAsync(storeItems))));
            }
            catch (Exception ex)
            {
                Assert.IsInstanceOfType(ex, typeof(InvalidOperationException));
            }

            var readStoreItems = new Dictionary<string, object>(await storage.ReadAsync(new[] { "createPoco" }));
            Assert.IsInstanceOfType(readStoreItems["createPoco"], typeof(PocoItem));
            var createPoco = readStoreItems["createPoco"] as PocoItem;
            Assert.AreEqual(createPoco.Id, "1", "createPoco.id should be 1");
        }
    }

    public class PocoItem
    {
        public string Id { get; set; }

        public int Count { get; set; }

        public string[] ExtraBytes { get; set; }
    }

    public class PocoStoreItem : IStoreItem
    {
        public string ETag { get; set; }

        public string Id { get; set; }

        public int Count { get; set; }
    }
}
