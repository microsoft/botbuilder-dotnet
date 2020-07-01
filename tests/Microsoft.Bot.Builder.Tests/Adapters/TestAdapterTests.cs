// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Reflection;
using System.Security;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Xunit;

namespace Microsoft.Bot.Builder.Tests.Adapters
{
    public class TestAdapterTests
    {
        [Fact]
        public async Task TestAdapter_ExceptionTypesOnTest()
        {
            string uniqueExceptionId = Guid.NewGuid().ToString();
            TestAdapter adapter = new TestAdapter(TestAdapter.CreateConversation("TestAdapter_ExceptionTypesOnTest"));
            await Assert.ThrowsAsync<Exception>(() =>
                  new TestFlow(adapter, async (context, cancellationToken) =>
                  {
                      await context.SendActivityAsync(context.Activity.CreateReply("one"));
                  })
                          .Test("foo", (activity) => throw new Exception(uniqueExceptionId))
                          .StartTestAsync());  
        }

        [Fact]
        public async Task TestAdapter_ExceptionInBotOnReceive()
        {
            string uniqueExceptionId = Guid.NewGuid().ToString();
            TestAdapter adapter = new TestAdapter(TestAdapter.CreateConversation("TestAdapter_ExceptionInBotOnReceive"));
            await Assert.ThrowsAsync<Exception>(() => 
                new TestFlow(adapter, (context, cancellationToken) => 
                { 
                    throw new Exception(uniqueExceptionId); 
                })
                    .Test("test", activity => Assert.Null(null), "uh oh!")
                    .StartTestAsync());
        }

        [Fact]
        public async Task TestAdapter_ExceptionTypesOnAssertReply()
        {
            string uniqueExceptionId = Guid.NewGuid().ToString();
            TestAdapter adapter = new TestAdapter(TestAdapter.CreateConversation("TestAdapter_ExceptionTypesOnAssertReply"));
            await Assert.ThrowsAsync<Exception>(() =>
                new TestFlow(adapter, async (context, cancellationToken) =>
                {
                    await context.SendActivityAsync(context.Activity.CreateReply("one"));
                })
                    .Send("foo")
                    .AssertReply(
                        (activity) => throw new Exception(uniqueExceptionId), "should throw")

                    .StartTestAsync());
        }

        [Fact]
        public async Task TestAdapter_SaySimple()
        {
            var adapter = new TestAdapter(TestAdapter.CreateConversation("TestAdapter_SaySimple"));
            await new TestFlow(adapter, MyBotLogic)
                .Test("foo", "echo:foo", "say with string works")
                .StartTestAsync();
        }

        [Fact]
        public async Task TestAdapter_Say()
        {
            var adapter = new TestAdapter(TestAdapter.CreateConversation("TestAdapter_Say"));
            await new TestFlow(adapter, MyBotLogic)
                .Test("foo", "echo:foo", "say with string works")
                .Test("foo", new Activity(ActivityTypes.Message, text: "echo:foo"), "say with activity works")
                .Test("foo", (activity) => Assert.Equal("echo:foo", activity.AsMessageActivity().Text), "say with validator works")
                .StartTestAsync();
        }

        [Fact]
        public async Task TestAdapter_SendReply()
        {
            var adapter = new TestAdapter(TestAdapter.CreateConversation("TestAdapter_SendReply"));
            await new TestFlow(adapter, MyBotLogic)
                .Send("foo").AssertReply("echo:foo", "send/reply with string works")
                .Send("foo").AssertReply(new Activity(ActivityTypes.Message, text: "echo:foo"), "send/reply with activity works")
                .Send("foo").AssertReply((activity) => Assert.Equal("echo:foo", activity.AsMessageActivity().Text), "send/reply with validator works")
                .StartTestAsync();
        }

        [Fact]
        public async Task TestAdapter_ReplyOneOf()
        {
            var adapter = new TestAdapter(TestAdapter.CreateConversation("TestAdapter_ReplyOneOf"));
            await new TestFlow(adapter, MyBotLogic)
                .Send("foo").AssertReplyOneOf(new string[] { "echo:bar", "echo:foo", "echo:blat" }, "say with string works")
                .StartTestAsync();
        }

        [Fact]
        public async Task TestAdapter_MultipleReplies()
        {
            var adapter = new TestAdapter(TestAdapter.CreateConversation("TestAdapter_MultipleReplies"));
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

        [Fact]
        public async Task TestAdapter_TestFlow_SecurityException()
        {
            var adapter = new TestAdapter(TestAdapter.CreateConversation("TestAdapter_TestFlow_SecurityException"));

            TestFlow testFlow = new TestFlow(adapter, (ctx, cancellationToken) =>
                {
                    Exception innerException = (Exception)Activator.CreateInstance(typeof(SecurityException));
                    var taskSource = new TaskCompletionSource<bool>();
                    taskSource.SetException(innerException);
                    return taskSource.Task;
                })
                .Send(new Activity());
            await testFlow.StartTestAsync()
                .ContinueWith(action =>
                {
                    Assert.IsType<SecurityException>(action.Exception.InnerException);
                });
        }

        [Fact]
        public async Task TestAdapter_TestFlow_ArgumentException()
        {
            var adapter = new TestAdapter(TestAdapter.CreateConversation("TestAdapter_TestFlow_ArgumentException"));

            TestFlow testFlow = new TestFlow(adapter, (ctx, cancellationToken) =>
            {
                Exception innerException = (Exception)Activator.CreateInstance(typeof(ArgumentException));
                var taskSource = new TaskCompletionSource<bool>();
                taskSource.SetException(innerException);
                return taskSource.Task;
            })
                .Send(new Activity());
            await testFlow.StartTestAsync()
                .ContinueWith(action =>
                {
                    Assert.IsType<ArgumentException>(action.Exception.InnerException);
                });
        }

        [Fact]
        public async Task TestAdapter_TestFlow_ArgumentNullException()
        {
            var adapter = new TestAdapter(TestAdapter.CreateConversation("TestAdapter_TestFlow_ArgumentNullException"));

            TestFlow testFlow = new TestFlow(adapter, (ctx, cancellationToken) =>
            {
                Exception innerException = (Exception)Activator.CreateInstance(typeof(ArgumentNullException));
                var taskSource = new TaskCompletionSource<bool>();
                taskSource.SetException(innerException);
                return taskSource.Task;
            })
                .Send(new Activity());
            await testFlow.StartTestAsync()
                .ContinueWith(action =>
                {
                    Assert.IsType<ArgumentNullException>(action.Exception.InnerException);
                });
        }

        [Fact]
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
            Assert.Null(token);

            var oAuthAppCredentials = MicrosoftAppCredentials.Empty;
            token = await adapter.GetUserTokenAsync(turnContext, oAuthAppCredentials, "myConnection", null, CancellationToken.None);
            Assert.Null(token);
        }

        [Fact]
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
            Assert.Null(token);

            var oAuthAppCredentials = MicrosoftAppCredentials.Empty;
            token = await adapter.GetUserTokenAsync(turnContext, oAuthAppCredentials, "myConnection", "abc123", CancellationToken.None);
            Assert.Null(token);
        }

        [Fact]
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
            Assert.NotNull(tokenResponse);
            Assert.Equal(token, tokenResponse.Token);
            Assert.Equal(connectionName, tokenResponse.ConnectionName);

            var oAuthAppCredentials = MicrosoftAppCredentials.Empty;
            tokenResponse = await adapter.GetUserTokenAsync(turnContext, oAuthAppCredentials, connectionName, null, CancellationToken.None);
            Assert.NotNull(tokenResponse);
            Assert.Equal(token, tokenResponse.Token);
            Assert.Equal(connectionName, tokenResponse.ConnectionName);
        }

        [Fact]
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
            Assert.Null(tokenResponse);

            // Can be retrieved with magic code
            tokenResponse = await adapter.GetUserTokenAsync(turnContext, connectionName, magicCode, CancellationToken.None);
            Assert.NotNull(tokenResponse);
            Assert.Equal(token, tokenResponse.Token);
            Assert.Equal(connectionName, tokenResponse.ConnectionName);

            // Then can be retrieved without magic code
            tokenResponse = await adapter.GetUserTokenAsync(turnContext, connectionName, null, CancellationToken.None);
            Assert.NotNull(tokenResponse);
            Assert.Equal(token, tokenResponse.Token);
            Assert.Equal(connectionName, tokenResponse.ConnectionName);

            // Then can be retrieved using customized AppCredentials
            var oAuthAppCredentials = MicrosoftAppCredentials.Empty;
            tokenResponse = await adapter.GetUserTokenAsync(turnContext, oAuthAppCredentials, connectionName, null, CancellationToken.None);
            Assert.NotNull(tokenResponse);
            Assert.Equal(token, tokenResponse.Token);
            Assert.Equal(connectionName, tokenResponse.ConnectionName);
        }

        [Fact]
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
            Assert.NotNull(link);
            Assert.True(link.Length > 0);

            var oAuthAppCredentials = MicrosoftAppCredentials.Empty;
            link = await adapter.GetOauthSignInLinkAsync(turnContext, oAuthAppCredentials, connectionName, userId, null, CancellationToken.None);
            Assert.NotNull(link);
            Assert.True(link.Length > 0);
        }

        [Fact]
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
            Assert.NotNull(link);
            Assert.True(link.Length > 0);

            var oAuthAppCredentials = MicrosoftAppCredentials.Empty;
            link = await adapter.GetOauthSignInLinkAsync(turnContext, oAuthAppCredentials, connectionName, CancellationToken.None);
            Assert.NotNull(link);
            Assert.True(link.Length > 0);
        }

        [Fact]
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
            var oAuthAppCredentials = MicrosoftAppCredentials.Empty;

            await adapter.SignOutUserAsync(turnContext);
            await adapter.SignOutUserAsync(turnContext, connectionName);
            await adapter.SignOutUserAsync(turnContext, connectionName, userId);
            await adapter.SignOutUserAsync(turnContext, connectionName: null, userId);
            await adapter.SignOutUserAsync(turnContext, oAuthAppCredentials, connectionName, userId);
        }

        [Fact]
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
            Assert.NotNull(tokenResponse);
            Assert.Equal(token, tokenResponse.Token);
            Assert.Equal(connectionName, tokenResponse.ConnectionName);

            await adapter.SignOutUserAsync(turnContext, connectionName, userId);
            tokenResponse = await adapter.GetUserTokenAsync(turnContext, connectionName, null, CancellationToken.None);
            Assert.Null(tokenResponse);

            adapter.AddUserToken(connectionName, channelId, userId, token);
            var oAuthAppCredentials = MicrosoftAppCredentials.Empty;

            tokenResponse = await adapter.GetUserTokenAsync(turnContext, oAuthAppCredentials, connectionName, null, CancellationToken.None);
            Assert.NotNull(tokenResponse);
            Assert.Equal(token, tokenResponse.Token);
            Assert.Equal(connectionName, tokenResponse.ConnectionName);

            await adapter.SignOutUserAsync(turnContext, oAuthAppCredentials, connectionName, userId);
            tokenResponse = await adapter.GetUserTokenAsync(turnContext, connectionName, null, CancellationToken.None);
            Assert.Null(tokenResponse);
        }

        [Fact]
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
            Assert.NotNull(tokenResponse);
            Assert.Equal(token, tokenResponse.Token);
            Assert.Equal("ABC", tokenResponse.ConnectionName);

            tokenResponse = await adapter.GetUserTokenAsync(turnContext, "DEF", null, CancellationToken.None);
            Assert.NotNull(tokenResponse);
            Assert.Equal(token, tokenResponse.Token);
            Assert.Equal("DEF", tokenResponse.ConnectionName);

            await adapter.SignOutUserAsync(turnContext, connectionName: null, userId);
            tokenResponse = await adapter.GetUserTokenAsync(turnContext, "ABC", null, CancellationToken.None);
            Assert.Null(tokenResponse);
            tokenResponse = await adapter.GetUserTokenAsync(turnContext, "DEF", null, CancellationToken.None);
            Assert.Null(tokenResponse);

            adapter.AddUserToken("ABC", channelId, userId, token);
            adapter.AddUserToken("DEF", channelId, userId, token);

            var oAuthAppCredentials = MicrosoftAppCredentials.Empty;
            tokenResponse = await adapter.GetUserTokenAsync(turnContext, oAuthAppCredentials, "ABC", null, CancellationToken.None);
            Assert.NotNull(tokenResponse);
            Assert.Equal(token, tokenResponse.Token);
            Assert.Equal("ABC", tokenResponse.ConnectionName);

            tokenResponse = await adapter.GetUserTokenAsync(turnContext, oAuthAppCredentials, "DEF", null, CancellationToken.None);
            Assert.NotNull(tokenResponse);
            Assert.Equal(token, tokenResponse.Token);
            Assert.Equal("DEF", tokenResponse.ConnectionName);

            await adapter.SignOutUserAsync(turnContext, connectionName: null, userId);
            tokenResponse = await adapter.GetUserTokenAsync(turnContext, oAuthAppCredentials, "ABC", null, CancellationToken.None);
            Assert.Null(tokenResponse);
            tokenResponse = await adapter.GetUserTokenAsync(turnContext, oAuthAppCredentials, "DEF", null, CancellationToken.None);
            Assert.Null(tokenResponse);
        }

        [Fact]
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
            Assert.NotNull(status);
            Assert.Equal(2, status.Length);

            var oAuthAppCredentials = MicrosoftAppCredentials.Empty;
            status = await adapter.GetTokenStatusAsync(turnContext, oAuthAppCredentials, userId);
            Assert.NotNull(status);
            Assert.Equal(2, status.Length);
        }

        [Fact]
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
            Assert.NotNull(status);
            Assert.Single(status);

            var oAuthAppCredentials = MicrosoftAppCredentials.Empty;
            status = await adapter.GetTokenStatusAsync(turnContext, oAuthAppCredentials, userId, "DEF");
            Assert.NotNull(status);
            Assert.Single(status);
        }

        [Theory]
        [InlineData(Channels.Test)]
        [InlineData(Channels.Emulator)]
        [InlineData(Channels.Msteams)]
        [InlineData(Channels.Webchat)]
        [InlineData(Channels.Cortana)]
        [InlineData(Channels.Directline)]
        [InlineData(Channels.Facebook)]
        [InlineData(Channels.Slack)]
        [InlineData(Channels.Telegram)]
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
            Assert.Equal(targetChannel, receivedChannelId);
            Assert.Equal(targetChannel, reply.ChannelId);
        }

        private async Task MyBotLogic(ITurnContext turnContext, CancellationToken cancellationToken)
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
    }
}
