// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.Bot.Builder.Tests.Common.Storage
{
    public abstract class StorageTestsBase
    {
        protected async Task ReadUnknownTest(IStorage storage)
        {
            var result = await storage.ReadAsync(new[] { "unknown" });
            Assert.NotNull(result);
            Assert.Null(result.FirstOrDefault(e => e.Key == "unknown").Value);
        }

        protected async Task CreateObjectTest(IStorage storage)
        {
            var storeItems = new Dictionary<string, object>
            {
                ["createPoco"] = new PocoItem() { Id = "1" },
                ["createPocoStoreItem"] = new PocoStoreItem() { Id = "2" },
            };

            await storage.WriteAsync(storeItems);

            var readStoreItems = new CachedBotStateDictionary(await storage.ReadAsync(storeItems.Keys.ToArray()));

            var createPoco = readStoreItems.MapValueTo<PocoItem>("createPoco");
            Assert.IsType<PocoItem>(createPoco);
            Assert.NotNull(createPoco);
            Assert.Equal("1", createPoco.Id);

            var createPocoStoreItem = readStoreItems.MapValueTo<PocoStoreItem>("createPocoStoreItem");
            Assert.IsType<PocoStoreItem>(createPocoStoreItem);
            Assert.NotNull(createPocoStoreItem);
            Assert.Equal("2", createPocoStoreItem.Id);
            Assert.NotNull(createPocoStoreItem.ETag);
        }

        protected async Task TestTypedObjects(IStorage storage, bool expectTyped)
        {
            var storeItems = new CachedBotStateDictionary
            {
                ["createPoco"] = new PocoItem() { Id = "1" },
                ["createPocoStoreItem"] = new PocoStoreItem() { Id = "2" },
            };

            await storage.WriteAsync(storeItems);

            var readStoreItems = new CachedBotStateDictionary(await storage.ReadAsync(storeItems.Keys.ToArray()));

            if (expectTyped)
            {
                Assert.IsType<PocoItem>(readStoreItems.MapValueTo<PocoItem>("createPoco"));
                Assert.IsType<PocoStoreItem>(readStoreItems.MapValueTo<PocoStoreItem>("createPocoStoreItem"));
            }
            else
            {
                Assert.IsType<JObject>(readStoreItems["createPoco"]);
                Assert.IsType<JObject>(readStoreItems["createPocoStoreItem"]);
            }

            if (expectTyped)
            {
                var createPoco = readStoreItems["createPoco"] as PocoItem;

                Assert.NotNull(createPoco);
                Assert.Equal("1", createPoco.Id);

                var createPocoStoreItem = readStoreItems["createPocoStoreItem"] as PocoStoreItem;

                Assert.NotNull(createPocoStoreItem);
                Assert.Equal("2", createPocoStoreItem.Id);
                Assert.NotNull(createPocoStoreItem.ETag);
            }
            else
            {
                var createPoco = readStoreItems.MapValueTo<PocoItem>("createPoco");

                Assert.NotNull(createPoco);
                Assert.Equal("1", createPoco.Id);

                var createPocoStoreItem = readStoreItems.MapValueTo<PocoStoreItem>("createPocoStoreItem");

                Assert.NotNull(createPocoStoreItem);
                Assert.Equal("2", createPocoStoreItem.Id);
                Assert.NotNull(createPocoStoreItem.ETag);
            }
        }

        protected async Task HandleCrazyKeys(IStorage storage)
        {
            var key = "!@#$%^&*()~/\\><,.?';\"`~";
            var storeItem = new PocoStoreItem() { Id = "1" };

            var dict = new Dictionary<string, object>() { { key, storeItem } };

            await storage.WriteAsync(dict);

            var storeItems = new CachedBotStateDictionary(await storage.ReadAsync(new[] { key }));

            storeItem = storeItems.MapValueTo<PocoStoreItem>(key);

            Assert.NotNull(storeItem);
            Assert.Equal("1", storeItem.Id);
        }

        protected async Task UpdateObjectTest(IStorage storage)
        {
            var originalPocoItem = new PocoItem() { Id = "1", Count = 1 };
            var originalPocoStoreItem = new PocoStoreItem() { Id = "1", Count = 1 };

            // first write should work
            var dict = new Dictionary<string, object>()
            {
                { "pocoItem", originalPocoItem },
                { "pocoStoreItem", originalPocoStoreItem },
            };

            await storage.WriteAsync(dict);

            var loadedStoreItems = new CachedBotStateDictionary(await storage.ReadAsync(new[] { "pocoItem", "pocoStoreItem" }));

            var updatePocoItem = loadedStoreItems.MapValueTo<PocoItem>("pocoItem");
            var updatePocoStoreItem = loadedStoreItems.MapValueTo<PocoStoreItem>("pocoStoreItem");
            Assert.NotNull(updatePocoStoreItem.ETag);

            // 2nd write should work, because we have new etag, or no etag
            updatePocoItem.Count++;
            updatePocoStoreItem.Count++;

            await storage.WriteAsync(loadedStoreItems);

            var reloadedStoreItems = new CachedBotStateDictionary(await storage.ReadAsync(new[] { "pocoItem", "pocoStoreItem" }));

            var reloeadedUpdatePocoItem = reloadedStoreItems.MapValueTo<PocoItem>("pocoItem");
            var reloadedUpdatePocoStoreItem = reloadedStoreItems.MapValueTo<PocoStoreItem>("pocoStoreItem");

            Assert.NotNull(reloadedUpdatePocoStoreItem.ETag);
            Assert.NotEqual(updatePocoStoreItem.ETag, reloadedUpdatePocoStoreItem.ETag);
            Assert.Equal(2, reloeadedUpdatePocoItem.Count);
            Assert.Equal(2, reloadedUpdatePocoStoreItem.Count);

            // write with old etag should succeed for non-storeitem
            try
            {
                updatePocoItem.Count = 123;

                await storage.WriteAsync(
                    new Dictionary<string, object>() { { "pocoItem", updatePocoItem } });
            }
            catch
            {
                Assert.True(false); // This should not be hit
            }

            // write with old etag should FAIL for storeitem

            bool threwException = false;
            try
            {
                await storage.WriteAsync(new Dictionary<string, object>() { { "pocoStoreItem", updatePocoStoreItem } });
            }
            catch
            {
                threwException = true;
            }

            Assert.True(threwException);

            var reloadedStoreItems2 = new CachedBotStateDictionary(await storage.ReadAsync(new[] { "pocoItem", "pocoStoreItem" }));

            var reloadedPocoItem2 = reloadedStoreItems2.MapValueTo<PocoItem>("pocoItem");
            var reloadedPocoStoreItem2 = reloadedStoreItems2.MapValueTo<PocoStoreItem>("pocoStoreItem");

            Assert.Equal(123, reloadedPocoItem2.Count);
            Assert.Equal(2, reloadedPocoStoreItem2.Count);

            // write with wildcard etag should work
            reloadedPocoItem2.Count = 100;
            reloadedPocoStoreItem2.Count = 100;
            reloadedPocoStoreItem2.ETag = "*";

            var wildcardEtagedict = new Dictionary<string, object>()
            {
                { "pocoItem", reloadedPocoItem2 },
                { "pocoStoreItem", reloadedPocoStoreItem2 },
            };

            await storage.WriteAsync(wildcardEtagedict);

            var reloadedStoreItems3 = new CachedBotStateDictionary(await storage.ReadAsync(new[] { "pocoItem", "pocoStoreItem" }));

            Assert.Equal(100, reloadedStoreItems3.MapValueTo<PocoItem>("pocoItem").Count);
            Assert.Equal(100, reloadedStoreItems3.MapValueTo<PocoStoreItem>("pocoStoreItem").Count);

            // write with empty etag should not work
            try
            {
                var reloadedStoreItems4 = new CachedBotStateDictionary(await storage.ReadAsync(new[] { "pocoStoreItem" }));
                var reloadedStoreItem4 = reloadedStoreItems4.MapValueTo<PocoStoreItem>("pocoStoreItem");

                Assert.NotNull(reloadedStoreItem4);

                reloadedStoreItem4.ETag = string.Empty;
                var dict2 = new Dictionary<string, object>()
                {
                    { "pocoStoreItem", reloadedStoreItem4 },
                };

                await storage.WriteAsync(dict2);

                Assert.True(false); // "Should have thrown exception on write with storeitem because of empty etag"
            }
            catch
            {
            }

            var finalStoreItems = new CachedBotStateDictionary(await storage.ReadAsync(new[] { "pocoItem", "pocoStoreItem" }));
            Assert.Equal(100, finalStoreItems.MapValueTo<PocoItem>("pocoItem").Count);
            Assert.Equal(100, finalStoreItems.MapValueTo<PocoStoreItem>("pocoStoreItem").Count);
        }

        protected async Task UpdateObjectTest_AsJObjects(IStorage storage)
        {
            var originalPocoItem = new PocoItem() { Id = "1", Count = 1 };
            var originalPocoStoreItem = new PocoStoreItem() { Id = "1", Count = 1 };

            // first write should work
            var dict = new Dictionary<string, object>()
            {
                { "pocoItem", JObject.FromObject(originalPocoItem) },
                { "pocoStoreItem", JObject.FromObject(originalPocoStoreItem) },
            };

            await storage.WriteAsync(dict);

            var loadedStoreItems = await storage.ReadAsync(new[] { "pocoItem", "pocoStoreItem" });

            var updatePocoItem = loadedStoreItems["pocoItem"] as JObject;
            var updatePocoStoreItem = loadedStoreItems["pocoStoreItem"] as JObject;
            Assert.NotNull(updatePocoStoreItem.Value<string>("ETag"));

            // 2nd write should work, because we have new etag, or no etag
            updatePocoItem["Count"] = updatePocoItem.Value<int>("Count") + 1;
            updatePocoStoreItem["Count"] = updatePocoStoreItem.Value<int>("Count") + 1;

            await storage.WriteAsync(loadedStoreItems);

            var reloadedStoreItems = await storage.ReadAsync(new[] { "pocoItem", "pocoStoreItem" });

            var reloeadedUpdatePocoItem = reloadedStoreItems["pocoItem"] as JObject;
            var reloadedUpdatePocoStoreItem = reloadedStoreItems["pocoStoreItem"] as JObject;

            Assert.NotNull(reloadedUpdatePocoStoreItem.Value<string>("ETag"));
            Assert.NotEqual(updatePocoStoreItem.Value<string>("ETag"), reloadedUpdatePocoStoreItem.Value<string>("ETag"));
            Assert.Equal(2, reloeadedUpdatePocoItem.Value<int>("Count"));
            Assert.Equal(2, reloadedUpdatePocoStoreItem.Value<int>("Count"));

            // write with old etag should succeed for non-storeitem
            try
            {
                updatePocoItem["Count"] = 123;
                
                updatePocoItem["ETag"] = "*";

                await storage.WriteAsync(
                    new Dictionary<string, object>() { { "pocoItem", updatePocoItem } });
            }
            catch
            {
                Assert.True(false); // This should not be hit
            }

            // write with old etag should FAIL for storeitem

            updatePocoStoreItem["Count"] = 123;
            bool threwException = false;
            try
            {
                //updatePocoStoreItem["ETag"] = "*";
                await storage.WriteAsync(new Dictionary<string, object>() { { "pocoStoreItem", updatePocoStoreItem } });
            }
            catch
            {
                threwException = true;
            }

            Assert.True(threwException);

            var reloadedStoreItems2 = await storage.ReadAsync(new[] { "pocoItem", "pocoStoreItem" });

            var reloadedPocoItem2 = reloadedStoreItems2["pocoItem"] as JObject;
            var reloadedPocoStoreItem2 = reloadedStoreItems2["pocoStoreItem"] as JObject;

            Assert.Equal(123, reloadedPocoItem2["Count"]);
            Assert.Equal(2, reloadedPocoStoreItem2["Count"]);

            // write with wildcard etag should work
            reloadedPocoItem2["Count"] = 100;
            reloadedPocoStoreItem2["Count"] = 100;
            reloadedPocoStoreItem2["ETag"] = "*";

            var wildcardEtagedict = new Dictionary<string, object>()
            {
                { "pocoItem", reloadedPocoItem2 },
                { "pocoStoreItem", reloadedPocoStoreItem2 },
            };

            await storage.WriteAsync(wildcardEtagedict);

            var reloadedStoreItems3 = await storage.ReadAsync(new[] { "pocoItem", "pocoStoreItem" });

            Assert.Equal(100, (reloadedStoreItems3["pocoItem"] as JObject).Value<int>("Count"));
            Assert.Equal(100, (reloadedStoreItems3["pocoStoreItem"] as JObject).Value<int>("Count"));

            // write with empty etag should not work
            try
            {
                var reloadedStoreItems4 = await storage.ReadAsync(new[] { "pocoStoreItem" });
                var reloadedStoreItem4 = reloadedStoreItems4["pocoStoreItem"] as JObject;

                Assert.NotNull(reloadedStoreItem4);

                reloadedStoreItem4["ETag"] = string.Empty;
                var dict2 = new Dictionary<string, object>()
                {
                    { "pocoStoreItem", reloadedStoreItem4 },
                };

                await storage.WriteAsync(dict2);

                Assert.True(false); // "Should have thrown exception on write with storeitem because of empty etag"
            }
            catch
            {
            }

            var finalStoreItems = new Dictionary<string, object>(await storage.ReadAsync(new[] { "pocoItem", "pocoStoreItem" }));
            Assert.Equal(100, (finalStoreItems["pocoItem"] as JObject).Value<int>("Count"));
            Assert.Equal(100, (finalStoreItems["pocoStoreItem"] as JObject).Value<int>("Count"));
        }

        protected async Task DeleteObjectTest(IStorage storage)
        {
            // first write should work
            var dict = new CachedBotStateDictionary
                {
                    { "delete1", new PocoStoreItem() { Id = "1", Count = 1 } },
                };

            await storage.WriteAsync(dict);

            var storeItems = new CachedBotStateDictionary(await storage.ReadAsync(new[] { "delete1" }));
            var storeItem = storeItems.MapValueTo<PocoStoreItem>("delete1");

            Assert.NotNull(storeItem.ETag);
            Assert.Equal(1, storeItem.Count);

            await storage.DeleteAsync(new[] { "delete1" });

            var reloadedStoreItems = await storage.ReadAsync(new[] { "delete1" });

            Assert.False(reloadedStoreItems.Any(), "no store item should have been found because it was deleted");
        }

        protected async Task DeleteUnknownObjectTest(IStorage storage)
        {
            await storage.DeleteAsync(new[] { "unknown_key" });
        }

        protected async Task BatchCreateObjectTest(IStorage storage, long minimumExtraBytes = 0)
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
                new Dictionary<string, object> { ["createPoco"] = new PocoItem() { Id = "1", Count = 0, ExtraBytes = stringArray } },
                new Dictionary<string, object> { ["createPoco"] = new PocoItem() { Id = "1", Count = 1, ExtraBytes = stringArray } },
                new Dictionary<string, object> { ["createPoco"] = new PocoItem() { Id = "1", Count = 2, ExtraBytes = stringArray } },
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
                Assert.IsType<InvalidOperationException>(ex);
            }

            var readStoreItems = new CachedBotStateDictionary(await storage.ReadAsync(new[] { "createPoco" }));
            var createPoco = readStoreItems.MapValueTo<PocoItem>("createPoco");
            Assert.Equal("1", createPoco.Id);
        }

        protected async Task StatePersistsThroughMultiTurn(IStorage storage)
        {
            var userState = new UserState(storage);
            var testProperty = userState.CreateProperty<TestPocoState>("test");
            var adapter = new TestAdapter()
                .Use(new AutoSaveStateMiddleware(userState));

            await new TestFlow(
                adapter,
                async (context, cancellationToken) =>
                {
                    var state = await testProperty.GetAsync(context, () => new TestPocoState());
                    Assert.NotNull(state);
                    switch (context.Activity.Text)
                    {
                        case "set value":
                            state.Value = "test";
                            await context.SendActivityAsync("value saved");
                            break;
                        case "get value":
                            await context.SendActivityAsync(state.Value);
                            break;
                    }
                })
                .Test("set value", "value saved")
                .Test("get value", "test")
                .StartTestAsync();
        }
    }
}
