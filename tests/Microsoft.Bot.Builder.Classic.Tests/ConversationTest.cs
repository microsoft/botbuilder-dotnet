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

using Autofac;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Classic;
using Microsoft.Bot.Builder.Classic.ConnectorEx;
using Microsoft.Bot.Builder.Classic.Dialogs;
using Microsoft.Bot.Builder.Classic.Dialogs.Internals;
using Microsoft.Bot.Builder.Classic.Internals.Fibers;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Schema;
using Microsoft.Rest;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace Microsoft.Bot.Builder.Classic.Tests
{
    public class MockConnectorFactory : IConnectorClientFactory
    {
        protected readonly IBotDataStore<BotData> memoryDataStore = new InMemoryDataStore();
        protected readonly string botId;

        public MockConnectorFactory(string botId)
        {
            SetField.NotNull(out this.botId, nameof(botId), botId);
        }

        public IConnectorClient MakeConnectorClient()
        {
            var client = new Mock<ConnectorClient>();
            client.CallBase = true;
            return client.Object;
        }

        protected IAddress AddressFrom(string channelId, string userId, string conversationId)
        {
            var address = new Address
            (
                this.botId,
                channelId,
                userId ?? "AllUsers",
                conversationId ?? "AllConversations",
                "InvalidServiceUrl"
            );
            return address;
        }
        protected async Task<HttpOperationResponse<BotData>> UpsertData(string channelId, string userId, string conversationId, BotStoreType storeType, BotData data)
        {
            var _result = new HttpOperationResponse<BotData>();
            _result.Request = new HttpRequestMessage();
            try
            {
                var address = AddressFrom(channelId, userId, conversationId);
                await memoryDataStore.SaveAsync(address, storeType, data, CancellationToken.None);
            }
            catch (Exception e)
            {
                _result.Body = null;
                HttpStatusCode status;
                if (Enum.TryParse<HttpStatusCode>(e.Message, out status))
                    _result.Response = new HttpResponseMessage { StatusCode = status };
                else
                    _result.Response = new HttpResponseMessage { StatusCode = HttpStatusCode.InternalServerError };
                var ex = new HttpOperationException(e?.Message, e);
                ex.Request = new HttpRequestMessageWrapper(_result.Request, string.Empty);
                ex.Response = new HttpResponseMessageWrapper(_result.Response, e?.Message);
                throw ex;
            }


            _result.Body = data;
            _result.Response = new HttpResponseMessage { StatusCode = HttpStatusCode.OK };
            return _result;
        }

        protected async Task<HttpOperationResponse<BotData>> GetData(string channelId, string userId, string conversationId, BotStoreType storeType)
        {
            var _result = new HttpOperationResponse<BotData>();
            _result.Request = new HttpRequestMessage();
            BotData data;
            var address = AddressFrom(channelId, userId, conversationId);
            data = await memoryDataStore.LoadAsync(address, storeType, CancellationToken.None);
            _result.Body = data;
            _result.Response = new HttpResponseMessage { StatusCode = HttpStatusCode.OK };
            return _result;
        }

        //public Mock<StateClient> MockIBots(MockConnectorFactory mockConnectorFactory)
        //{
        //    var botsClient = new Moq.Mock<StateClient>(MockBehavior.Loose);

        //    botsClient.Setup(d => d.BotState.SetConversationDataWithHttpMessagesAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<BotData>(), It.IsAny<Dictionary<string, List<string>>>(), It.IsAny<CancellationToken>()))
        //        .Returns<string, string, BotData, Dictionary<string, List<string>>, CancellationToken>(async (channelId, conversationId, data, headers, token) =>
        //        {
        //            return await mockConnectorFactory.UpsertData(channelId, null, conversationId, BotStoreType.BotConversationData, data);
        //        });

        //    botsClient.Setup(d => d.BotState.GetConversationDataWithHttpMessagesAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Dictionary<string, List<string>>>(), It.IsAny<CancellationToken>()))
        //        .Returns<string, string, Dictionary<string, List<string>>, CancellationToken>(async (channelId, conversationId, headers, token) =>
        //        {
        //            return await mockConnectorFactory.GetData(channelId, null, conversationId, BotStoreType.BotConversationData);
        //        });


        //    botsClient.Setup(d => d.BotState.SetUserDataWithHttpMessagesAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<BotData>(), It.IsAny<Dictionary<string, List<string>>>(), It.IsAny<CancellationToken>()))
        //      .Returns<string, string, BotData, Dictionary<string, List<string>>, CancellationToken>(async (channelId, userId, data, headers, token) =>
        //      {
        //          return await mockConnectorFactory.UpsertData(channelId, userId, null, BotStoreType.BotUserData, data);
        //      });

        //    botsClient.Setup(d => d.BotState.GetUserDataWithHttpMessagesAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Dictionary<string, List<string>>>(), It.IsAny<CancellationToken>()))
        //        .Returns<string, string, Dictionary<string, List<string>>, CancellationToken>(async (channelId, userId, headers, token) =>
        //        {
        //            return await mockConnectorFactory.GetData(channelId, userId, null, BotStoreType.BotUserData);
        //        });

        //    botsClient.Setup(d => d.BotState.SetPrivateConversationDataWithHttpMessagesAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<BotData>(), It.IsAny<Dictionary<string, List<string>>>(), It.IsAny<CancellationToken>()))
        //     .Returns<string, string, string, BotData, Dictionary<string, List<string>>, CancellationToken>(async (channelId, conversationId, userId, data, headers, token) =>
        //     {
        //         return await mockConnectorFactory.UpsertData(channelId, userId, conversationId, BotStoreType.BotPrivateConversationData, data);
        //     });

        //    botsClient.Setup(d => d.BotState.GetPrivateConversationDataWithHttpMessagesAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Dictionary<string, List<string>>>(), It.IsAny<CancellationToken>()))
        //     .Returns<string, string, string, Dictionary<string, List<string>>, CancellationToken>(async (channelId, conversationId, userId, headers, token) =>
        //     {
        //         return await mockConnectorFactory.GetData(channelId, userId, conversationId, BotStoreType.BotPrivateConversationData);
        //     });

        //    return botsClient;
        //}
    }

    public class AlwaysNeedInputHintChannelCapability : IChannelCapability
    {
        private readonly IChannelCapability inner;
        public AlwaysNeedInputHintChannelCapability(IChannelCapability inner)
        {
            SetField.NotNull(out this.inner, nameof(inner), inner);
        }

        public bool NeedsInputHint()
        {
            return true;
        }

        public bool SupportsKeyboards(int buttonCount)
        {
            return this.inner.SupportsKeyboards(buttonCount);
        }

        public bool SupportsSpeak()
        {
            return this.inner.SupportsSpeak();
        }
    }

    public abstract class ConversationTestBase
    {
        [Flags]
        public enum Options { None, InMemoryBotDataStore, NeedsInputHint };

        public static IContainer Build(Options options, params object[] singletons)
        {
            var builder = new ContainerBuilder();
            builder.RegisterModule(new DialogModule_MakeRoot());

            // make a "singleton" MockConnectorFactory per unit test execution
            IConnectorClientFactory factory = null;
            builder
                .Register((c, p) => factory ?? (factory = new MockConnectorFactory(c.Resolve<IAddress>().BotId)))
                .As<IConnectorClientFactory>()
                .InstancePerLifetimeScope();

            var r =
              builder
              .Register<Queue<Activity>>(c => ((TestAdapter)c.Resolve<Microsoft.Bot.Builder.ITurnContext>().Adapter).ActiveQueue)
              .AsSelf()
              .InstancePerLifetimeScope();

            if (options.HasFlag(Options.InMemoryBotDataStore))
            {
                //Note: memory store will be single instance for the bot
                builder.RegisterType<InMemoryDataStore>()
                    .AsSelf()
                    .SingleInstance();

                builder.Register(c => new CachingBotDataStore(c.Resolve<InMemoryDataStore>(), CachingBotDataStoreConsistencyPolicy.ETagBasedConsistency))
                    .As<IBotDataStore<BotData>>()
                    .AsSelf()
                    .InstancePerLifetimeScope();
            }

            if (options.HasFlag(Options.NeedsInputHint))
            {
                builder.Register(c => new AlwaysNeedInputHintChannelCapability(new ChannelCapability(c.Resolve<IAddress>())))
                    .AsImplementedInterfaces()
                    .InstancePerLifetimeScope();
            }

            foreach (var singleton in singletons)
            {
                builder
                    .Register(c => singleton)
                    .Keyed(FiberModule.Key_DoNotSerialize, singleton.GetType());
            }

            return builder.Build();
        }
    }


    [TestClass]
    public sealed class ConversationTest : ConversationTestBase
    {
        //[TestMethod]
        //public async Task InMemoryBotDataStoreTest()
        //{
        //    var chain = Chain.PostToChain().Select(m => m.Text).ContinueWith<string, string>(async (context, result) =>
        //        {
        //            int t = 0;
        //            context.UserData.TryGetValue("count", out t);
        //            if (t > 0)
        //            {
        //                int value;
        //                Assert.IsTrue(context.ConversationData.TryGetValue("conversation", out value));
        //                Assert.AreEqual(t - 1, value);
        //                Assert.IsTrue(context.UserData.TryGetValue("user", out value));
        //                Assert.AreEqual(t + 1, value);
        //                Assert.IsTrue(context.PrivateConversationData.TryGetValue("PrivateConversationData", out value));
        //                Assert.AreEqual(t + 2, value);
        //            }

        //            context.ConversationData.SetValue("conversation", t);
        //            context.UserData.SetValue("user", t + 2);
        //            context.PrivateConversationData.SetValue("PrivateConversationData", t + 3);
        //            context.UserData.SetValue("count", ++t);
        //            return Chain.Return($"{t}:{await result}");
        //        }).PostToUser();
        //    Func<IDialog<object>> MakeRoot = () => chain;

        //    using (new FiberTestBase.ResolveMoqAssembly(chain))
        //    using (var container = Build(Options.InMemoryBotDataStore, chain))
        //    {
        //        var msg = DialogTestBase.MakeTestMessage();
        //        msg.Text = "test";
        //        using (var scope = DialogModule.BeginLifetimeScope(container, msg))
        //        {
        //            scope.Resolve<Func<IDialog<object>>>(TypedParameter.From(MakeRoot));

        //            await Conversation.SendAsync(scope, msg);
        //            var reply = scope.Resolve<Queue<Activity>>().Dequeue();
        //            Assert.AreEqual("1:test", reply.Text);
        //            var store = scope.Resolve<CachingBotDataStore>();
        //            Assert.AreEqual(0, store.cache.Count);
        //            var dataStore = scope.Resolve<InMemoryDataStore>();
        //            Assert.AreEqual(3, dataStore.store.Count);
        //        }

        //        for (int i = 0; i < 10; i++)
        //        {
        //            using (var scope = DialogModule.BeginLifetimeScope(container, msg))
        //            {
        //                scope.Resolve<Func<IDialog<object>>>(TypedParameter.From(MakeRoot));
        //                await Conversation.SendAsync(scope, msg);
        //                var reply = scope.Resolve<Queue<Activity>>().Dequeue();
        //                Assert.AreEqual($"{i + 2}:test", reply.Text);
        //                var store = scope.Resolve<CachingBotDataStore>();
        //                Assert.AreEqual(0, store.cache.Count);
        //                var dataStore = scope.Resolve<InMemoryDataStore>();
        //                Assert.AreEqual(3, dataStore.store.Count);
        //                string val = string.Empty;
        //                Assert.IsTrue(scope.Resolve<IBotData>().PrivateConversationData.TryGetValue(DialogModule.BlobKey, out val));
        //                Assert.AreNotEqual(string.Empty, val);
        //            }
        //        }
        //    }
        //}

        [TestMethod]
        public async Task InputHintTest()
        {
            var chain = Chain.PostToChain().Select(m => m.Text).ContinueWith<string, string>(async (context, result) =>
            {
                var text = await result;
                if (text.ToLower().StartsWith("inputhint"))
                {
                    var reply = context.MakeMessage();
                    reply.Text = "reply";
                    reply.InputHint = InputHints.ExpectingInput;
                    await context.PostAsync(reply);
                    return Chain.Return($"{text}");
                }
                else if (!text.ToLower().StartsWith("reset"))
                {
                    for (int i = 0; i < 10; i++)
                    {
                        await context.PostAsync($"message:{i}");
                    }
                    return Chain.Return($"{text}");
                }
                else
                {
                    return Chain.From(() => new PromptDialog.PromptConfirm("Are you sure you want to reset the count?",
                            "Didn't get that!", 3, PromptStyle.Keyboard)).ContinueWith<bool, string>(async (ctx, res) =>
                            {
                                string reply;
                                if (await res)
                                {
                                    ctx.UserData.SetValue("count", 0);
                                    reply = "Reset count.";
                                }
                                else
                                {
                                    reply = "Did not reset count.";
                                }
                                return Chain.Return(reply);
                            });
                }

            }).PostToUser();
            Func<IDialog<object>> MakeRoot = () => chain;

            using (new FiberTestBase.ResolveMoqAssembly(chain))
            using (var container = Build(Options.InMemoryBotDataStore | Options.NeedsInputHint, chain))
            {
                var msg = DialogTestBase.MakeTestMessage();
                msg.Text = "test";
                await new TestAdapter().ProcessActivityAsync((Activity)msg, async (context, cancellationToken) =>
                {
                    using (var scope = DialogModule.BeginLifetimeScope(container, context))
                    {
                        scope.Resolve<Func<IDialog<object>>>(TypedParameter.From(MakeRoot));
                        await Conversation.SendAsync(scope, context);

                        var queue = ((TestAdapter)context.Adapter).ActiveQueue;
                        Assert.IsTrue(queue.Count > 0);
                        while (queue.Count > 0)
                        {
                            var toUser = queue.Dequeue();
                            if (queue.Count > 0)
                            {
                                Assert.IsTrue(toUser.InputHint == InputHints.IgnoringInput);
                            }
                            else
                            {
                                Assert.IsTrue(toUser.InputHint == InputHints.AcceptingInput);
                            }
                        }
                    }
                });


                msg.Text = "inputhint";
                await new TestAdapter().ProcessActivityAsync((Activity)msg, async (context, cancellationToken) =>
                {
                    using (var scope = DialogModule.BeginLifetimeScope(container, context))
                    {
                        scope.Resolve<Func<IDialog<object>>>(TypedParameter.From(MakeRoot));
                        await Conversation.SendAsync(scope, context);

                        var queue = ((TestAdapter)context.Adapter).ActiveQueue;
                        Assert.IsTrue(queue.Count == 2);
                        var toUser = queue.Dequeue();
                        Assert.AreEqual("reply", toUser.Text);
                        Assert.IsTrue(toUser.InputHint == InputHints.ExpectingInput);
                    }
                });

                msg.Text = "reset";
                await new TestAdapter().ProcessActivityAsync((Activity)msg, async (context, cancellationToken) =>
                {
                    using (var scope = DialogModule.BeginLifetimeScope(container, context))
                    {
                        scope.Resolve<Func<IDialog<object>>>(TypedParameter.From(MakeRoot));
                        await Conversation.SendAsync(scope, context);

                        var queue = ((TestAdapter)context.Adapter).ActiveQueue;
                        Assert.IsTrue(queue.Count == 1);
                        var toUser = queue.Dequeue();
                        Assert.IsTrue(toUser.InputHint == InputHints.ExpectingInput);
                        Assert.IsNotNull(toUser.LocalTimestamp);
                    }
                });
            }
        }


        [TestMethod]
        public async Task SendResumeAsyncTest()
        {
            var chain = Chain.PostToChain().Select(m => m.Text).Switch(
                new RegexCase<IDialog<string>>(new Regex("^resume"), (context, data) => { context.UserData.SetValue("resume", true); return Chain.Return("resumed!"); }),
                new DefaultCase<string, IDialog<string>>((context, data) => { return Chain.Return(data); })).Unwrap().PostToUser();

            using (new FiberTestBase.ResolveMoqAssembly(chain))
            using (var container = Build(Options.InMemoryBotDataStore, chain))
            {
                var msg = DialogTestBase.MakeTestMessage();
                msg.Text = "testMsg";
                await new TestAdapter().ProcessActivityAsync((Activity)msg, async (context, cancellationToken) =>
                {
                    using (var scope = DialogModule.BeginLifetimeScope(container, context))
                    {
                        Func<IDialog<object>> MakeRoot = () => chain;
                        scope.Resolve<Func<IDialog<object>>>(TypedParameter.From(MakeRoot));

                        await Conversation.SendAsync(scope, context);
                        var queue = ((TestAdapter)context.Adapter).ActiveQueue;
                        var reply = queue.Dequeue();

                        var botData = scope.Resolve<IBotData>();
                        await botData.LoadAsync(default(CancellationToken));
                        var dataBag = scope.Resolve<Func<IBotDataBag>>()();
                        Assert.IsTrue(dataBag.ContainsKey(ResumptionContext.RESUMPTION_CONTEXT_KEY));
                        Assert.IsNotNull(scope.Resolve<ConversationReference>());
                    }
                });

                var conversationReference = msg.ToConversationReference();
                var continuationActivity = conversationReference.GetContinuationActivity();
                await new TestAdapter().ProcessActivityAsync((Activity)continuationActivity, async (context, cancellationToken) =>
                {
                    using (var scope = DialogModule.BeginLifetimeScope(container, context))
                    {
                        Func<IDialog<object>> MakeRoot = () => { throw new InvalidOperationException(); };
                        scope.Resolve<Func<IDialog<object>>>(TypedParameter.From(MakeRoot));

                        await scope.Resolve<IPostToBot>().PostAsync(new Activity { Text = "resume" }, CancellationToken.None);

                        var queue = ((TestAdapter)context.Adapter).ActiveQueue;
                        var reply = queue.Dequeue();
                        Assert.AreEqual("resumed!", reply.Text);

                        var botData = scope.Resolve<IBotData>();
                        await botData.LoadAsync(default(CancellationToken));
                        Assert.IsTrue(botData.UserData.GetValue<bool>("resume"));
                    }
                });
            }
        }
    }
}
