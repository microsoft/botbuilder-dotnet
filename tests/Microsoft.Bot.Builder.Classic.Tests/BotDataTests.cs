//// 
//// Copyright (c) Microsoft. All rights reserved.
//// Licensed under the MIT license.
//// 
//// Microsoft Bot Framework: http://botframework.com
//// 
//// Bot Builder SDK GitHub:
//// https://github.com/Microsoft/BotBuilder
//// 
//// Copyright (c) Microsoft Corporation
//// All rights reserved.
//// 
//// MIT License:
//// Permission is hereby granted, free of charge, to any person obtaining
//// a copy of this software and associated documentation files (the
//// "Software"), to deal in the Software without restriction, including
//// without limitation the rights to use, copy, modify, merge, publish,
//// distribute, sublicense, and/or sell copies of the Software, and to
//// permit persons to whom the Software is furnished to do so, subject to
//// the following conditions:
//// 
//// The above copyright notice and this permission notice shall be
//// included in all copies or substantial portions of the Software.
//// 
//// THE SOFTWARE IS PROVIDED ""AS IS"", WITHOUT WARRANTY OF ANY KIND,
//// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
//// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
//// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
//// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
//// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
//// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
////

//using System;
//using System.Collections.Generic;
//using System.Net;
//using System.Text;
//using System.Threading;
//using System.Threading.Tasks;
//using Autofac;
//using Microsoft.Bot.Builder.Classic.Dialogs;
//using Microsoft.Bot.Builder.Classic.Dialogs.Internals;
//using Microsoft.Bot.Schema;
//using Microsoft.Rest;
//using Microsoft.VisualStudio.TestTools.UnitTesting;

//namespace Microsoft.Bot.Builder.Classic.Tests
//{
//    public abstract class BotDataTests
//    {
//        protected abstract IBotData MakeBotData();

//        public static void SetGet<T>(IBotData data, Func<IBotData, IBotDataBag> findBag, string key, T value)
//        {
//            int count;
//            {
//                var bag = findBag(data);
//                count = bag.Count;
//                T existing;
//                Assert.IsFalse(bag.TryGetValue(key, out existing));

//                bag.SetValue(key, value);

//                Assert.IsTrue(bag.TryGetValue(key, out existing));
//                Assert.AreEqual(existing, value);

//                existing = bag.GetValue<T>(key);
//                Assert.AreEqual(existing, value);

//                Assert.AreEqual(count + 1, bag.Count);
//            }

//            {
//                var bag = findBag(data);
//                Assert.IsTrue(bag.RemoveValue(key));
//                Assert.AreEqual(count, bag.Count);

//                Assert.IsFalse(bag.RemoveValue(key));
//                Assert.AreEqual(count, bag.Count);

//                T existing;
//                Assert.IsFalse(bag.TryGetValue(key, out existing));
//            }
//        }

//        [TestMethod]
//        public async Task BotDataBag_SetGet()
//        {
//            var data = MakeBotData();
//            await data.LoadAsync(default(CancellationToken));
//            var bag = data.PrivateConversationData;
//            Assert.AreEqual(0, bag.Count);

//            SetGet(data, d => d.PrivateConversationData, "blob", Encoding.UTF8.GetBytes("PrivateConversationData"));
//            SetGet(data, d => d.ConversationData, "blob", Encoding.UTF8.GetBytes("ConversationData"));
//            SetGet(data, d => d.UserData, "blob", Encoding.UTF8.GetBytes("UserData"));
//        }

//        [TestMethod]
//        public async Task BotDataBag_Stream()
//        {
//            var data = MakeBotData();
//            await data.LoadAsync(default(CancellationToken));
//            var bag = data.PrivateConversationData;
//            var key = "PrivateConversationData";

//            Assert.AreEqual(0, bag.Count);

//            using (var stream = new BotDataBagStream(bag, key))
//            {
//                Assert.AreEqual(0, stream.Length);
//            }

//            Assert.AreEqual(1, bag.Count);

//            var blob = Encoding.UTF8.GetBytes(key);
//            using (var stream = new BotDataBagStream(bag, key))
//            {
//                Assert.AreEqual(0, stream.Length);
//                stream.WriteAsync(blob, 0, blob.Length);
//                Assert.AreEqual(blob.Length, stream.Length);
//            }

//            Assert.AreEqual(1, bag.Count);

//            using (var stream = new BotDataBagStream(bag, key))
//            {
//                Assert.AreEqual(blob.Length, stream.Length);
//                var read = new byte[blob.Length];
//                stream.ReadAsync(read, 0, blob.Length);
//                CollectionAssert.AreEqual(read, blob);
//            }

//            Assert.AreEqual(1, bag.Count);
//        }
//    }

//    [TestClass]
//    public sealed class BotDataTests_JObject : BotDataTests
//    {
//        protected override IBotData MakeBotData()
//        {
//            var msg = DialogTestBase.MakeTestMessage();

//            return new JObjectBotData(Address.FromActivity(msg), new CachingBotDataStore(new InMemoryDataStore(), CachingBotDataStoreConsistencyPolicy.ETagBasedConsistency));
//        }
//    }

//    [TestClass]
//    public sealed class BotDataTests_Dictionary : BotDataTests
//    {
//        protected override IBotData MakeBotData()
//        {
//            var msg = DialogTestBase.MakeTestMessage();
//            return new DictionaryBotData(Address.FromActivity(msg), new CachingBotDataStore(new InMemoryDataStore(), CachingBotDataStoreConsistencyPolicy.ETagBasedConsistency));
//        }
//    }

//    [TestClass]
//    public sealed class BotDataTest_Consistency : DialogTestBase
//    {
//        static IConnectorClientFactory connectorFactory;

//        IDialog<object> chain = Chain.PostToChain().ContinueWith<IMessageActivity, string>(async (context, result) =>
//        {
//            var res = (Activity)await result;
//            int t = 0;
//            // saving a conflicting data by directly calling the state client
//            var stateClient = connectorFactory.MakeStateClient();
//            var data = await stateClient.BotState.GetPrivateConversationDataAsync(res.ChannelId, res.Conversation.Id, res.From.Id);
//            data.SetProperty("mycount", 10);
//            await stateClient.BotState.SetPrivateConversationDataAsync(res.ChannelId, res.Conversation.Id, res.From.Id, data);

//            // save some data using context
//            context.PrivateConversationData.TryGetValue("count", out t);
//            context.PrivateConversationData.SetValue("count", ++t);
//            return Chain.Return($"{t}:{res.Text}");
//        }).PostToUser();

//        [TestMethod]
//        public async Task EnforceETagConsistency()
//        {
//            Func<IDialog<object>> MakeRoot = () => chain;

//            using (new FiberTestBase.ResolveMoqAssembly(chain))
//            using (var container = Build(Options.MockConnectorFactory, chain))
//            {
//                var msg = DialogTestBase.MakeTestMessage();
//                msg.Text = "test";
//                using (var scope = DialogModule.BeginLifetimeScope(container, msg))
//                {
//                    connectorFactory = scope.Resolve<IConnectorClientFactory>();

//                    scope.Resolve<Func<IDialog<object>>>(TypedParameter.From(MakeRoot));
//                    try
//                    {
//                        await Conversation.SendAsync(scope, msg);
//                        Assert.Fail();
//                    }
//                    catch (HttpOperationException e)
//                    {
//                        Assert.AreEqual(e.Response.StatusCode, HttpStatusCode.PreconditionFailed);
//                        var connectorFactory = scope.Resolve<IConnectorClientFactory>();
//                        var stateClient = connectorFactory.MakeStateClient();
//                        var data = await stateClient.BotState.GetPrivateConversationDataAsync(msg.ChannelId, msg.Conversation.Id, msg.From.Id);
//                        Assert.AreEqual(10, data.GetProperty<int>("mycount"));
//                    }
//                    catch (Exception)
//                    {
//                        Assert.Fail();
//                    }
//                }
//            }
//        }

//        [TestMethod]
//        public async Task LastWriteWinsConsistency()
//        {
//            Func<IDialog<object>> MakeRoot = () => chain;

//            using (new FiberTestBase.ResolveMoqAssembly(chain))
//            using (var container = Build(Options.MockConnectorFactory | Options.LastWriteWinsCachingBotDataStore, chain))
//            {
//                var msg = DialogTestBase.MakeTestMessage();
//                msg.Text = "test";
//                using (var scope = DialogModule.BeginLifetimeScope(container, msg))
//                {
//                    connectorFactory = scope.Resolve<IConnectorClientFactory>();

//                    scope.Resolve<Func<IDialog<object>>>(TypedParameter.From(MakeRoot));
//                    await Conversation.SendAsync(scope, msg);
//                    var reply = scope.Resolve<Queue<Activity>>().Dequeue();
//                    Assert.AreEqual("1:test", reply.Text);
//                    var stateClient = connectorFactory.MakeStateClient();
//                    var data = await stateClient.BotState.GetPrivateConversationDataAsync(msg.ChannelId, msg.Conversation.Id, msg.From.Id);
//                    Assert.AreEqual(1, data.GetProperty<int>("count"));
//                    Assert.AreEqual(0, data.GetProperty<int>("mycount")); // The overwritten data should be 0
//                }
//            }
//        }
//    }

//}
