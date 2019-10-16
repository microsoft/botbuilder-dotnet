// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Security;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Tests.Adapters
{
    [TestClass]
    [TestCategory("Adapter")]
    public class TestAdapterTests
    {
        public TestContext TestContext { get; set; }

        public async Task MyBotLogic(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            switch (turnContext.Activity.AsMessageActivity().Text)
            {
                case "count":
                    await turnContext.SendActivityAsync(turnContext.Activity.CreateReply("one"));
                    await turnContext.SendActivityAsync(turnContext.Activity.CreateReply("two"));
                    await turnContext.SendActivityAsync(turnContext.Activity.CreateReply("three"));
                    break;
                case "ignore":
                    break;
                default:
                    await turnContext.SendActivityAsync(
                        turnContext.Activity.CreateReply($"echo:{turnContext.Activity.AsMessageActivity().Text}"));
                    break;
            }
        }

        [TestMethod]
        public async Task TestAdapter_ExceptionTypesOnTest()
        {
            string uniqueExceptionId = Guid.NewGuid().ToString();
            TestAdapter adapter = new TestAdapter(TestAdapter.CreateConversation(TestContext.TestName));

            try
            {
                await new TestFlow(adapter, async (context, cancellationToken) =>
                {
                    await context.SendActivityAsync(context.Activity.CreateReply("one"));
                })
                    .Test("foo", (activity) => throw new Exception(uniqueExceptionId))
                    .StartTestAsync();

                Assert.Fail("An Exception should have been thrown");
            }
            catch (Exception ex)
            {
                Assert.IsTrue(ex.Message == uniqueExceptionId, "Incorrect Exception Text");
            }
        }

        [TestMethod]
        public async Task TestAdapter_ExceptionInBotOnReceive()
        {
            string uniqueExceptionId = Guid.NewGuid().ToString();
            TestAdapter adapter = new TestAdapter(TestAdapter.CreateConversation(TestContext.TestName));

            try
            {
                await new TestFlow(adapter, (context, cancellationToken) => { throw new Exception(uniqueExceptionId); })
                    .Test("test", activity => Assert.IsNull(null), "uh oh!")
                    .StartTestAsync();

                Assert.Fail("An Exception should have been thrown");
            }
            catch (Exception ex)
            {
                Assert.IsTrue(ex.InnerException.Message == uniqueExceptionId, "Incorrect Exception Text");
            }
        }

        [TestMethod]
        public async Task TestAdapter_ExceptionTypesOnAssertReply()
        {
            string uniqueExceptionId = Guid.NewGuid().ToString();
            TestAdapter adapter = new TestAdapter(TestAdapter.CreateConversation(TestContext.TestName));

            try
            {
                await new TestFlow(adapter, async (context, cancellationToken) =>
                {
                    await context.SendActivityAsync(context.Activity.CreateReply("one"));
                })
                    .Send("foo")
                    .AssertReply(
                        (activity) => throw new Exception(uniqueExceptionId), "should throw")
                    .StartTestAsync();

                Assert.Fail("An Exception should have been thrown");
            }
            catch (Exception ex)
            {
                Assert.IsTrue(ex.Message == uniqueExceptionId, "Incorrect Exception Text");
            }
        }

        [TestMethod]
        public async Task TestAdapter_SaySimple()
        {
            var adapter = new TestAdapter(TestAdapter.CreateConversation(TestContext.TestName));
            await new TestFlow(adapter, MyBotLogic)
                .Test("foo", "echo:foo", "say with string works")
                .StartTestAsync();
        }

        [TestMethod]
        public async Task TestAdapter_Say()
        {
            var adapter = new TestAdapter(TestAdapter.CreateConversation(TestContext.TestName));
            await new TestFlow(adapter, MyBotLogic)
                .Test("foo", "echo:foo", "say with string works")
                .Test("foo", new Activity(ActivityTypes.Message, text: "echo:foo"), "say with activity works")
                .Test("foo", (activity) => Assert.AreEqual("echo:foo", activity.AsMessageActivity().Text), "say with validator works")
                .StartTestAsync();
        }

        [TestMethod]
        public async Task TestAdapter_SendReply()
        {
            var adapter = new TestAdapter(TestAdapter.CreateConversation(TestContext.TestName));
            await new TestFlow(adapter, MyBotLogic)
                .Send("foo").AssertReply("echo:foo", "send/reply with string works")
                .Send("foo").AssertReply(new Activity(ActivityTypes.Message, text: "echo:foo"), "send/reply with activity works")
                .Send("foo").AssertReply((activity) => Assert.AreEqual("echo:foo", activity.AsMessageActivity().Text), "send/reply with validator works")
                .StartTestAsync();
        }

        [TestMethod]
        public async Task TestAdapter_ReplyOneOf()
        {
            var adapter = new TestAdapter(TestAdapter.CreateConversation(TestContext.TestName));
            await new TestFlow(adapter, MyBotLogic)
                .Send("foo").AssertReplyOneOf(new string[] { "echo:bar", "echo:foo", "echo:blat" }, "say with string works")
                .StartTestAsync();
        }

        [TestMethod]
        public async Task TestAdapter_MultipleReplies()
        {
            var adapter = new TestAdapter(TestAdapter.CreateConversation(TestContext.TestName));
            await new TestFlow(adapter, MyBotLogic)
                .Send("foo").AssertReply("echo:foo")
                .Send("bar").AssertReply("echo:bar")
                .Send("ignore")
                .Send("count")
                    .AssertReply("one")
                    .AssertReply("two")
                    .AssertReply("three")
                .StartTestAsync();
        }

        [DataTestMethod]
        [DataRow(typeof(SecurityException))]
        [DataRow(typeof(ArgumentException))]
        [DataRow(typeof(ArgumentNullException))]
        public async Task TestAdapter_TestFlow(Type exceptionType)
        {
            var adapter = new TestAdapter(TestAdapter.CreateConversation(TestContext.TestName));

            TestFlow testFlow = new TestFlow(adapter, (ctx, cancellationToken) =>
                {
                    Exception innerException = (Exception)Activator.CreateInstance(exceptionType);
                    var taskSource = new TaskCompletionSource<bool>();
                    taskSource.SetException(innerException);
                    return taskSource.Task;
                })
                .Send(new Activity());
            await testFlow.StartTestAsync()
                .ContinueWith(action =>
                {
                    Assert.IsInstanceOfType(action.Exception.InnerException, exceptionType);
                });
        }

        [TestMethod]
        public async Task TestAdapter_GetUserTokenAsyncReturnsNull()
        {
            TestAdapter adapter = new TestAdapter();
            Activity activity = new Activity()
            {
                ChannelId = "directline",
                From = new ChannelAccount()
                {
                    Id = "testUser",
                },
            };
            TurnContext turnContext = new TurnContext(adapter, activity);

            var token = await adapter.GetUserTokenAsync(turnContext, "myConnection", null, CancellationToken.None);
            Assert.IsNull(token);
        }

        [TestMethod]
        public async Task TestAdapter_GetUserTokenAsyncReturnsNullWithCode()
        {
            TestAdapter adapter = new TestAdapter();
            Activity activity = new Activity()
            {
                ChannelId = "directline",
                From = new ChannelAccount()
                {
                    Id = "testUser",
                },
            };
            TurnContext turnContext = new TurnContext(adapter, activity);

            var token = await adapter.GetUserTokenAsync(turnContext, "myConnection", "abc123", CancellationToken.None);
            Assert.IsNull(token);
        }

        [TestMethod]
        public async Task TestAdapter_GetUserTokenAsyncReturnsToken()
        {
            TestAdapter adapter = new TestAdapter();
            string connectionName = "myConnection";
            string channelId = "directline";
            string userId = "testUser";
            string token = "abc123";
            Activity activity = new Activity()
            {
                ChannelId = channelId,
                From = new ChannelAccount()
                {
                    Id = userId,
                },
            };
            TurnContext turnContext = new TurnContext(adapter, activity);

            adapter.AddUserToken(connectionName, channelId, userId, token);

            var tokenResponse = await adapter.GetUserTokenAsync(turnContext, connectionName, null, CancellationToken.None);
            Assert.IsNotNull(tokenResponse);
            Assert.AreEqual(token, tokenResponse.Token);
            Assert.AreEqual(connectionName, tokenResponse.ConnectionName);
        }

        [TestMethod]
        public async Task TestAdapter_GetUserTokenAsyncReturnsTokenWithMagicCode()
        {
            TestAdapter adapter = new TestAdapter();
            string connectionName = "myConnection";
            string channelId = "directline";
            string userId = "testUser";
            string token = "abc123";
            string magicCode = "888999";
            Activity activity = new Activity()
            {
                ChannelId = channelId,
                From = new ChannelAccount()
                {
                    Id = userId,
                },
            };
            TurnContext turnContext = new TurnContext(adapter, activity);

            adapter.AddUserToken(connectionName, channelId, userId, token, magicCode);

            // First it's null
            var tokenResponse = await adapter.GetUserTokenAsync(turnContext, connectionName, null, CancellationToken.None);
            Assert.IsNull(tokenResponse);

            // Can be retrieved with magic code
            tokenResponse = await adapter.GetUserTokenAsync(turnContext, connectionName, magicCode, CancellationToken.None);
            Assert.IsNotNull(tokenResponse);
            Assert.AreEqual(token, tokenResponse.Token);
            Assert.AreEqual(connectionName, tokenResponse.ConnectionName);

            // Then can be retrieved without magic code
            tokenResponse = await adapter.GetUserTokenAsync(turnContext, connectionName, null, CancellationToken.None);
            Assert.IsNotNull(tokenResponse);
            Assert.AreEqual(token, tokenResponse.Token);
            Assert.AreEqual(connectionName, tokenResponse.ConnectionName);
        }

        [TestMethod]
        public async Task TestAdapter_GetSignInLink()
        {
            TestAdapter adapter = new TestAdapter();
            string connectionName = "myConnection";
            string channelId = "directline";
            string userId = "testUser";
            Activity activity = new Activity()
            {
                ChannelId = channelId,
                From = new ChannelAccount()
                {
                    Id = userId,
                },
            };
            TurnContext turnContext = new TurnContext(adapter, activity);

            var link = await adapter.GetOauthSignInLinkAsync(turnContext, connectionName, userId, null, CancellationToken.None);
            Assert.IsNotNull(link);
            Assert.IsTrue(link.Length > 0);
        }

        [TestMethod]
        public async Task TestAdapter_GetSignInLinkWithNoUserId()
        {
            TestAdapter adapter = new TestAdapter();
            string connectionName = "myConnection";
            string channelId = "directline";
            string userId = "testUser";
            Activity activity = new Activity()
            {
                ChannelId = channelId,
                From = new ChannelAccount()
                {
                    Id = userId,
                },
            };
            TurnContext turnContext = new TurnContext(adapter, activity);

            var link = await adapter.GetOauthSignInLinkAsync(turnContext, connectionName, CancellationToken.None);
            Assert.IsNotNull(link);
            Assert.IsTrue(link.Length > 0);
        }

        [TestMethod]
        public async Task TestAdapter_SignOutNoop()
        {
            TestAdapter adapter = new TestAdapter();
            string connectionName = "myConnection";
            string channelId = "directline";
            string userId = "testUser";
            Activity activity = new Activity()
            {
                ChannelId = channelId,
                From = new ChannelAccount()
                {
                    Id = userId,
                },
            };
            TurnContext turnContext = new TurnContext(adapter, activity);

            await adapter.SignOutUserAsync(turnContext);
            await adapter.SignOutUserAsync(turnContext, connectionName);
            await adapter.SignOutUserAsync(turnContext, connectionName, userId);
            await adapter.SignOutUserAsync(turnContext, null, userId);
        }

        [TestMethod]
        public async Task TestAdapter_SignOut()
        {
            TestAdapter adapter = new TestAdapter();
            string connectionName = "myConnection";
            string channelId = "directline";
            string userId = "testUser";
            string token = "abc123";
            Activity activity = new Activity()
            {
                ChannelId = channelId,
                From = new ChannelAccount()
                {
                    Id = userId,
                },
            };
            TurnContext turnContext = new TurnContext(adapter, activity);

            adapter.AddUserToken(connectionName, channelId, userId, token);

            var tokenResponse = await adapter.GetUserTokenAsync(turnContext, connectionName, null, CancellationToken.None);
            Assert.IsNotNull(tokenResponse);
            Assert.AreEqual(token, tokenResponse.Token);
            Assert.AreEqual(connectionName, tokenResponse.ConnectionName);

            await adapter.SignOutUserAsync(turnContext, connectionName, userId);
            tokenResponse = await adapter.GetUserTokenAsync(turnContext, connectionName, null, CancellationToken.None);
            Assert.IsNull(tokenResponse);
        }

        [TestMethod]
        public async Task TestAdapter_SignOutAll()
        {
            TestAdapter adapter = new TestAdapter();
            string channelId = "directline";
            string userId = "testUser";
            string token = "abc123";
            Activity activity = new Activity()
            {
                ChannelId = channelId,
                From = new ChannelAccount()
                {
                    Id = userId,
                },
            };
            TurnContext turnContext = new TurnContext(adapter, activity);

            adapter.AddUserToken("ABC", channelId, userId, token);
            adapter.AddUserToken("DEF", channelId, userId, token);

            var tokenResponse = await adapter.GetUserTokenAsync(turnContext, "ABC", null, CancellationToken.None);
            Assert.IsNotNull(tokenResponse);
            Assert.AreEqual(token, tokenResponse.Token);
            Assert.AreEqual("ABC", tokenResponse.ConnectionName);

            tokenResponse = await adapter.GetUserTokenAsync(turnContext, "DEF", null, CancellationToken.None);
            Assert.IsNotNull(tokenResponse);
            Assert.AreEqual(token, tokenResponse.Token);
            Assert.AreEqual("DEF", tokenResponse.ConnectionName);

            await adapter.SignOutUserAsync(turnContext, null, userId);
            tokenResponse = await adapter.GetUserTokenAsync(turnContext, "ABC", null, CancellationToken.None);
            Assert.IsNull(tokenResponse);
            tokenResponse = await adapter.GetUserTokenAsync(turnContext, "DEF", null, CancellationToken.None);
            Assert.IsNull(tokenResponse);
        }

        [TestMethod]
        public async Task TestAdapter_GetTokenStatus()
        {
            TestAdapter adapter = new TestAdapter();
            string channelId = "directline";
            string userId = "testUser";
            string token = "abc123";
            Activity activity = new Activity()
            {
                ChannelId = channelId,
                From = new ChannelAccount()
                {
                    Id = userId,
                },
            };
            TurnContext turnContext = new TurnContext(adapter, activity);

            adapter.AddUserToken("ABC", channelId, userId, token);
            adapter.AddUserToken("DEF", channelId, userId, token);

            var status = await adapter.GetTokenStatusAsync(turnContext, userId);
            Assert.IsNotNull(status);
            Assert.AreEqual(2, status.Length);
        }

        [TestMethod]
        public async Task TestAdapter_GetTokenStatusWithFilter()
        {
            TestAdapter adapter = new TestAdapter();
            string channelId = "directline";
            string userId = "testUser";
            string token = "abc123";
            Activity activity = new Activity()
            {
                ChannelId = channelId,
                From = new ChannelAccount()
                {
                    Id = userId,
                },
            };
            TurnContext turnContext = new TurnContext(adapter, activity);

            adapter.AddUserToken("ABC", channelId, userId, token);
            adapter.AddUserToken("DEF", channelId, userId, token);

            var status = await adapter.GetTokenStatusAsync(turnContext, userId, "DEF");
            Assert.IsNotNull(status);
            Assert.AreEqual(1, status.Length);
        }

        [DataTestMethod]
        [DataRow(Channels.Test)]
        [DataRow(Channels.Emulator)]
        [DataRow(Channels.Msteams)]
        [DataRow(Channels.Webchat)]
        [DataRow(Channels.Cortana)]
        [DataRow(Channels.Directline)]
        [DataRow(Channels.Facebook)]
        [DataRow(Channels.Slack)]
        [DataRow(Channels.Telegram)]
        public async Task ShouldUseCustomChannelId(string targetChannel)
        {
            var sut = new TestAdapter(targetChannel);

            var receivedChannelId = string.Empty;
            async Task TestCallback(ITurnContext context, CancellationToken token)
            {
                receivedChannelId = context.Activity.ChannelId;
                await context.SendActivityAsync("test reply from the bot", cancellationToken: token);
            }

            await sut.SendTextToBotAsync("test message", TestCallback, CancellationToken.None);
            var reply = sut.GetNextReply();
            Assert.AreEqual(targetChannel, receivedChannelId);
            Assert.AreEqual(targetChannel, reply.ChannelId);
        }
    }
}
