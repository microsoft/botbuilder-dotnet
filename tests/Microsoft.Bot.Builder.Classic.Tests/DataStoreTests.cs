// 
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license.
// 
// Microsoft Bot Framework: http://botframework.com
// 
// Bot Builder SDK GitHub:
// https://github.com/Microsoft/BotBuilder
// 
// Copyright (c) Microsoft Corporation
// All rights reserved.
// 
// MIT License:
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED ""AS IS"", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using Microsoft.Bot.Builder.Classic.Internals.Fibers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Classic.Dialogs.Internals;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Builder.Classic.Dialogs;
using System.Runtime.CompilerServices;
using System.Web;
using System.Net;
using Newtonsoft.Json.Linq;
using Microsoft.Bot.Builder.Classic;

namespace Microsoft.Bot.Builder.Classic.Tests
{

    public class SampleBotData
    {
        public SampleBotData()
        {
            Id = Guid.NewGuid().ToString("n");
        }

        public string Id { get; set; }
    }

    [TestClass]
    public class InMemoryStoreTests : BaseDataStoreTests
    {
        public override IBotDataStore<BotData> GetTestCaseDataStore()
        {
            return new InMemoryDataStore();
        }
    }

    public class BaseDataStoreTests
    {
        public virtual IBotDataStore<BotData> GetTestCaseDataStore()
        {
            return new InMemoryDataStore();
        }

        protected IAddress GetAddress([CallerMemberName] string testName = null)
        {
            return new Address("somebot", testName, "U1", "C1", string.Empty);
        }

        [TestMethod]
        public async Task SetGet()
        {
            var dataStore = GetTestCaseDataStore();
            var address = GetAddress();
            await _setGet(dataStore, BotStoreType.BotUserData, address);
            await _setGet(dataStore, BotStoreType.BotConversationData, address);
            await _setGet(dataStore, BotStoreType.BotPrivateConversationData, address);
        }

        protected async Task _setGet(IBotDataStore<BotData> dataStore, BotStoreType botStoreType, IAddress address)
        {
            var botData = new BotData() { Data = new SampleBotData() };

            await dataStore.SaveAsync(address, botStoreType, botData, default(CancellationToken));

            var botDataGet = await dataStore.LoadAsync(address, botStoreType, default(CancellationToken));

            Assert.IsNotNull(botDataGet.ETag);
            Assert.AreNotEqual("*", botDataGet.ETag);
            Assert.AreNotEqual(string.Empty, botDataGet.ETag);
            var savedSample = botData.Data as SampleBotData;
            var loadedSample = ((JObject)botDataGet.Data).ToObject<SampleBotData>();
            Assert.AreEqual(savedSample?.Id, loadedSample?.Id, "Stored and retrieved objects don't match");
        }

        [TestMethod]
        public async Task GetUnknownAddress()
        {
            var dataStore = GetTestCaseDataStore();
            var address = GetAddress();

            await _getUnknownAddress(dataStore, BotStoreType.BotUserData, address);
            await _getUnknownAddress(dataStore, BotStoreType.BotConversationData, address);
            await _getUnknownAddress(dataStore, BotStoreType.BotPrivateConversationData, address);
        }

        private static async Task _getUnknownAddress(IBotDataStore<BotData> dataStore, BotStoreType botStoreType, IAddress address)
        {
            var botDataGet = await dataStore.LoadAsync(address, botStoreType, default(CancellationToken));
            Assert.IsNotNull(botDataGet);
            Assert.AreEqual(botDataGet.ETag, string.Empty, $"Etag should be string.Empty for unknown object  {botStoreType}");
            Assert.IsNull(botDataGet.Data, $"Data should be null for unknown object {botStoreType}");
        }

        [TestMethod]
        public async Task SaveSemantics()
        {
            var dataStore = GetTestCaseDataStore();
            var address = GetAddress();

            await _saveSemantics(dataStore, BotStoreType.BotUserData, address);
            await _saveSemantics(dataStore, BotStoreType.BotConversationData, address);
            await _saveSemantics(dataStore, BotStoreType.BotPrivateConversationData, address);
        }

        protected async Task _saveSemantics(IBotDataStore<BotData> dataStore, BotStoreType botStoreType, IAddress address)
        {
            var botData = new BotData() { Data = new SampleBotData() };
            await dataStore.SaveAsync(address, botStoreType, botData, default(CancellationToken));
            var botDataGet = await dataStore.LoadAsync(address, botStoreType, default(CancellationToken));

            var loaded = ((JObject)botDataGet.Data).ToObject<SampleBotData>();
            Assert.AreNotEqual("*", botDataGet.ETag, $"Etag should not be * on load  {botStoreType}");

            var newSample = new SampleBotData();

            await dataStore.SaveAsync(address, botStoreType, new BotData(eTag: botDataGet.ETag, data: newSample), default(CancellationToken));
            var botDataGet2 = await dataStore.LoadAsync(address, botStoreType, default(CancellationToken));
            var loaded2 = ((JObject)botDataGet2.Data).ToObject<SampleBotData>();

            Assert.AreEqual(newSample?.Id, loaded2.Id, $"Updated data doesn't match {botStoreType}");
            Assert.AreNotEqual(botDataGet.ETag, botDataGet2.ETag, $"Etag wasn't updated {botStoreType}");

            try
            {
                newSample = new SampleBotData();
                botDataGet.Data = newSample;
                await dataStore.SaveAsync(address, botStoreType, botData, default(CancellationToken));
                Assert.Fail("Expected PreconditionFailed exception to throw on bad etag");
            }
                catch (Exception err)
            {
                Assert.AreEqual(HttpStatusCode.PreconditionFailed.ToString(), err.Message, $"Expected HttpException status code Precondition on bad etag save {botStoreType}");
            }

            botDataGet2.Data = null;
            await dataStore.SaveAsync(address, botStoreType, botDataGet2, default(CancellationToken));

            var botDataGet3 = await dataStore.LoadAsync(address, botStoreType, default(CancellationToken));
            var loaded3 = ((JObject)botDataGet3.Data)?.ToObject<SampleBotData>();
            Assert.IsNull(loaded3);
            Assert.AreEqual(string.Empty, botDataGet3.ETag);
        }
    }
}
