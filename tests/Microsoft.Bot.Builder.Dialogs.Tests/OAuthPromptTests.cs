// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Dialogs.Tests
{
    [TestClass]
    public class OAuthPromptTests
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void OAuthPromptWithEmptySettingsShouldFail()
        {
            var confirmPrompt = new OAuthPrompt("abc", null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void OAuthPromptWithEmptyIdShouldFail()
        {
            var emptyId = string.Empty;
            var confirmPrompt = new OAuthPrompt(emptyId, new OAuthPromptSettings());
        }

        [TestMethod]
        public async Task OAuthPromptWithDefaultTypeHandlingForStorage()
        {
            await OAuthPrompt(new MemoryStorage());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task OAuthPromptBeginDialogWithNoDialogContext()
        {
            var prompt = new OAuthPrompt("abc", new OAuthPromptSettings());
            await prompt.BeginDialogAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task OAuthPromptBeginDialogWithWrongOptions()
        {
            var prompt = new OAuthPrompt("abc", new OAuthPromptSettings());
            var convoState = new ConversationState(new MemoryStorage());
            var dialogState = convoState.CreateProperty<DialogState>("dialogState");

            var adapter = new TestAdapter()
                .Use(new AutoSaveStateMiddleware(convoState));

            // Create new DialogSet.
            var dialogs = new DialogSet(dialogState);
            dialogs.Add(prompt);

            var tc = new TurnContext(adapter, new Activity() { Type = ActivityTypes.Message, Conversation = new ConversationAccount() { Id = "123" }, ChannelId = "test" });

            var dc = await dialogs.CreateContextAsync(tc);

            await prompt.BeginDialogAsync(dc, CancellationToken.None);
        }

        [TestMethod]

        public async Task OAuthPromptWithNoneTypeHandlingForStorage()
        {
            await OAuthPrompt(new MemoryStorage(new JsonSerializer() { TypeNameHandling = TypeNameHandling.None }));
        }

        [TestMethod]
        public async Task OAuthPromptWithMagicCode()
        {
            var convoState = new ConversationState(new MemoryStorage());
            var dialogState = convoState.CreateProperty<DialogState>("dialogState");

            var adapter = new TestAdapter()
                .Use(new AutoSaveStateMiddleware(convoState));

            var connectionName = "myConnection";
            var token = "abc123";
            var magicCode = "888999";

            // Create new DialogSet.
            var dialogs = new DialogSet(dialogState);
            dialogs.Add(new OAuthPrompt("OAuthPrompt", new OAuthPromptSettings() { Text = "Please sign in", ConnectionName = connectionName, Title = "Sign in" }));

            BotCallbackHandler botCallbackHandler = async (turnContext, cancellationToken) =>
            {
                var dc = await dialogs.CreateContextAsync(turnContext, cancellationToken);

                var results = await dc.ContinueDialogAsync(cancellationToken);
                if (results.Status == DialogTurnStatus.Empty)
                {
                    await dc.PromptAsync("OAuthPrompt", new PromptOptions(), cancellationToken: cancellationToken);
                }
                else if (results.Status == DialogTurnStatus.Complete)
                {
                    if (results.Result is TokenResponse)
                    {
                        await turnContext.SendActivityAsync(MessageFactory.Text("Logged in."), cancellationToken);
                    }
                    else
                    {
                        await turnContext.SendActivityAsync(MessageFactory.Text("Failed."), cancellationToken);
                    }
                }
            };

            await new TestFlow(adapter, botCallbackHandler)
            .Send("hello")
            .AssertReply(activity =>
            {
                Assert.AreEqual(1, ((Activity)activity).Attachments.Count);
                Assert.AreEqual(OAuthCard.ContentType, ((Activity)activity).Attachments[0].ContentType);

                Assert.AreEqual(InputHints.AcceptingInput, ((Activity)activity).InputHint);

                // Add a magic code to the adapter
                adapter.AddUserToken(connectionName, activity.ChannelId, activity.Recipient.Id, token, magicCode);
            })
            .Send(magicCode)
            .AssertReply("Logged in.")
            .StartTestAsync();
        }

        [TestMethod]
        public async Task OAuthPromptTimesOut_Message()
        {
            await PromptTimeoutEndsDialogTest(MessageFactory.Text("hi"));
        }

        [TestMethod]
        public async Task OAuthPromptTimesOut_TokenResponseEvent()
        {
            var activity = new Activity() { Type = ActivityTypes.Event, Name = SignInConstants.TokenResponseEventName };
            activity.Value = JObject.FromObject(new TokenResponse(Channels.Msteams, "connectionName", "token", null));
            await PromptTimeoutEndsDialogTest(activity);
        }

        [TestMethod]
        public async Task OAuthPromptTimesOut_VerifyStateOperation()
        {
            var activity = new Activity() { Type = ActivityTypes.Invoke, Name = SignInConstants.VerifyStateOperationName };
            activity.Value = JObject.FromObject(new { state = "888999" });

            await PromptTimeoutEndsDialogTest(activity);
        }
        
        [TestMethod]
        public async Task OAuthPromptTimesOut_TokenExchangeOperation()
        {
            var activity = new Activity() { Type = ActivityTypes.Invoke, Name = SignInConstants.TokenExchangeOperationName };

            var connectionName = "myConnection";
            var exchangeToken = "exch123";
            var token = "abc123";

            activity.Value = JObject.FromObject(new TokenExchangeInvokeRequest()
            {
                ConnectionName = connectionName,
                Token = exchangeToken
            });

            await PromptTimeoutEndsDialogTest(activity);
        }

        public async Task OAuthPromptEndOnInvalidMessageSetting()
        {
            var convoState = new ConversationState(new MemoryStorage());
            var dialogState = convoState.CreateProperty<DialogState>("dialogState");

            var adapter = new TestAdapter()
                .Use(new AutoSaveStateMiddleware(convoState));

            var connectionName = "myConnection";

            // Create new DialogSet.
            var dialogs = new DialogSet(dialogState);
            dialogs.Add(new OAuthPrompt("OAuthPrompt", new OAuthPromptSettings() { Text = "Please sign in", ConnectionName = connectionName, Title = "Sign in", EndOnInvalidMessage = true }));

            BotCallbackHandler botCallbackHandler = async (turnContext, cancellationToken) =>
            {
                var dc = await dialogs.CreateContextAsync(turnContext, cancellationToken);

                var results = await dc.ContinueDialogAsync(cancellationToken);
                if (results.Status == DialogTurnStatus.Empty)
                {
                    await dc.PromptAsync("OAuthPrompt", new PromptOptions(), cancellationToken: cancellationToken);
                }
                else if (results.Status == DialogTurnStatus.Waiting)
                {
                    throw new InvalidOperationException("Test OAuthPromptEndOnInvalidMessageSetting expected DialogTurnStatus.Complete");
                }
                else if (results.Status == DialogTurnStatus.Complete)
                {
                    if (results.Result is TokenResponse)
                    {
                        await turnContext.SendActivityAsync(MessageFactory.Text("Logged in."), cancellationToken);
                    }
                    else
                    {
                        await turnContext.SendActivityAsync(MessageFactory.Text("Ended."), cancellationToken);
                    }
                }
            };

            await new TestFlow(adapter, botCallbackHandler)
            .Send("hello")
            .AssertReply(activity =>
            {
                Assert.AreEqual(1, ((Activity)activity).Attachments.Count);
                Assert.AreEqual(OAuthCard.ContentType, ((Activity)activity).Attachments[0].ContentType);
            })
            .Send("blah")
            .AssertReply("Ended.")
            .StartTestAsync();
        }

        [TestMethod]
        public async Task OAuthPromptDoesNotDetectCodeInBeginDialog()
        {
            var convoState = new ConversationState(new MemoryStorage());
            var dialogState = convoState.CreateProperty<DialogState>("dialogState");

            var adapter = new TestAdapter()
                .Use(new AutoSaveStateMiddleware(convoState));

            var connectionName = "myConnection";
            var token = "abc123";
            var magicCode = "888999";

            // Create new DialogSet
            var dialogs = new DialogSet(dialogState);
            dialogs.Add(new OAuthPrompt("OAuthPrompt", new OAuthPromptSettings() { Text = "Please sign in", ConnectionName = connectionName, Title = "Sign in" }));

            BotCallbackHandler botCallbackHandler = async (turnContext, cancellationToken) =>
            {
                // Add a magic code to the adapter preemptively so that we can test if the message that triggers BeginDialogAsync uses magic code detection
                adapter.AddUserToken(connectionName, turnContext.Activity.ChannelId, turnContext.Activity.From.Id, token, magicCode);

                var dc = await dialogs.CreateContextAsync(turnContext, cancellationToken);

                var results = await dc.ContinueDialogAsync(cancellationToken);

                if (results.Status == DialogTurnStatus.Empty)
                {
                    // If magicCode is detected when prompting, this will end the dialog and return the token in tokenResult
                    var tokenResult = await dc.PromptAsync("OAuthPrompt", new PromptOptions(), cancellationToken: cancellationToken);
                    if (tokenResult.Result is TokenResponse)
                    {
                        Assert.Fail();
                    }
                }
            };

            // Call BeginDialogAsync by sending the magic code as the first message. It SHOULD respond with an OAuthPrompt since we haven't authenticated yet
            await new TestFlow(adapter, botCallbackHandler)
            .Send(magicCode)
            .AssertReply(activity =>
            {
                Assert.AreEqual(1, ((Activity)activity).Attachments.Count);
                Assert.AreEqual(OAuthCard.ContentType, ((Activity)activity).Attachments[0].ContentType);

                Assert.AreEqual(InputHints.AcceptingInput, ((Activity)activity).InputHint);
            })
            .StartTestAsync();
        }

        [TestMethod]
        public async Task OAuthPromptWithTokenExchangeInvoke()
        {
            var convoState = new ConversationState(new MemoryStorage());
            var dialogState = convoState.CreateProperty<DialogState>("dialogState");

            var adapter = new TestAdapter()
                .Use(new AutoSaveStateMiddleware(convoState));

            var connectionName = "myConnection";
            var exchangeToken = "exch123";
            var token = "abc123";

            // Create new DialogSet.
            var dialogs = new DialogSet(dialogState);
            dialogs.Add(new OAuthPrompt("OAuthPrompt", new OAuthPromptSettings() { Text = "Please sign in", ConnectionName = connectionName, Title = "Sign in" }));

            BotCallbackHandler botCallbackHandler = async (turnContext, cancellationToken) =>
            {
                var dc = await dialogs.CreateContextAsync(turnContext, cancellationToken);

                var results = await dc.ContinueDialogAsync(cancellationToken);
                if (results.Status == DialogTurnStatus.Empty)
                {
                    await dc.PromptAsync("OAuthPrompt", new PromptOptions(), cancellationToken: cancellationToken);
                }
                else if (results.Status == DialogTurnStatus.Complete)
                {
                    if (results.Result is TokenResponse)
                    {
                        await turnContext.SendActivityAsync(MessageFactory.Text("Logged in."), cancellationToken);
                    }
                    else
                    {
                        await turnContext.SendActivityAsync(MessageFactory.Text("Failed."), cancellationToken);
                    }
                }
            };

            await new TestFlow(adapter, botCallbackHandler)
            .Send("hello")
            .AssertReply(activity =>
            {
                Assert.AreEqual(1, ((Activity)activity).Attachments.Count);
                Assert.AreEqual(OAuthCard.ContentType, ((Activity)activity).Attachments[0].ContentType);
                Assert.AreEqual(InputHints.AcceptingInput, ((Activity)activity).InputHint);

                // Add an exchangable token to the adapter
                adapter.AddExchangeableToken(connectionName, activity.ChannelId, activity.Recipient.Id, exchangeToken, token);
            })
            .Send(new Activity()
            {
                Type = ActivityTypes.Invoke,
                Name = SignInConstants.TokenExchangeOperationName,
                Value = JObject.FromObject(new TokenExchangeInvokeRequest()
                {
                    ConnectionName = connectionName,
                    Token = exchangeToken
                })
            })
            .AssertReply(a =>
            {
                Assert.AreEqual("invokeResponse", a.Type);
                var response = ((Activity)a).Value as InvokeResponse;
                Assert.IsNotNull(response);
                Assert.AreEqual(200, response.Status);
                var body = response.Body as TokenExchangeInvokeResponse;
                Assert.AreEqual(connectionName, body.ConnectionName);
                Assert.IsNull(body.FailureDetail);
            })
            .AssertReply("Logged in.")
            .StartTestAsync();
        }

        [TestMethod]
        public async Task OAuthPromptWithTokenExchangeFail()
        {
            var convoState = new ConversationState(new MemoryStorage());
            var dialogState = convoState.CreateProperty<DialogState>("dialogState");

            var adapter = new TestAdapter()
                .Use(new AutoSaveStateMiddleware(convoState));

            var connectionName = "myConnection";
            var exchangeToken = "exch123";

            // Create new DialogSet.
            var dialogs = new DialogSet(dialogState);
            dialogs.Add(new OAuthPrompt("OAuthPrompt", new OAuthPromptSettings() { Text = "Please sign in", ConnectionName = connectionName, Title = "Sign in" }));

            BotCallbackHandler botCallbackHandler = async (turnContext, cancellationToken) =>
            {
                var dc = await dialogs.CreateContextAsync(turnContext, cancellationToken);

                var results = await dc.ContinueDialogAsync(cancellationToken);
                if (results.Status == DialogTurnStatus.Empty)
                {
                    await dc.PromptAsync("OAuthPrompt", new PromptOptions(), cancellationToken: cancellationToken);
                }
                else if (results.Status == DialogTurnStatus.Complete)
                {
                    if (results.Result is TokenResponse)
                    {
                        await turnContext.SendActivityAsync(MessageFactory.Text("Logged in."), cancellationToken);
                    }
                    else
                    {
                        await turnContext.SendActivityAsync(MessageFactory.Text("Failed."), cancellationToken);
                    }
                }
            };

            await new TestFlow(adapter, botCallbackHandler)
            .Send("hello")
            .AssertReply(activity =>
            {
                Assert.AreEqual(1, ((Activity)activity).Attachments.Count);
                Assert.AreEqual(OAuthCard.ContentType, ((Activity)activity).Attachments[0].ContentType);
                Assert.AreEqual(InputHints.AcceptingInput, ((Activity)activity).InputHint);

                // No exchangable token is added to the adapter
            })
            .Send(new Activity()
            {
                Type = ActivityTypes.Invoke,
                Name = SignInConstants.TokenExchangeOperationName,
                Value = JObject.FromObject(new TokenExchangeInvokeRequest()
                {
                    ConnectionName = connectionName,
                    Token = exchangeToken
                })
            })
            .AssertReply(a =>
            {
                Assert.AreEqual("invokeResponse", a.Type);
                var response = ((Activity)a).Value as InvokeResponse;
                Assert.IsNotNull(response);
                Assert.AreEqual(412, response.Status);
                var body = response.Body as TokenExchangeInvokeResponse;
                Assert.AreEqual(connectionName, body.ConnectionName);
                Assert.IsNotNull(body.FailureDetail);
            })
            .StartTestAsync();
        }

        [TestMethod]
        public async Task OAuthPromptWithTokenExchangeNoBodyFails()
        {
            var convoState = new ConversationState(new MemoryStorage());
            var dialogState = convoState.CreateProperty<DialogState>("dialogState");

            var adapter = new TestAdapter()
                .Use(new AutoSaveStateMiddleware(convoState));

            var connectionName = "myConnection";

            // Create new DialogSet.
            var dialogs = new DialogSet(dialogState);
            dialogs.Add(new OAuthPrompt("OAuthPrompt", new OAuthPromptSettings() { Text = "Please sign in", ConnectionName = connectionName, Title = "Sign in" }));

            BotCallbackHandler botCallbackHandler = async (turnContext, cancellationToken) =>
            {
                var dc = await dialogs.CreateContextAsync(turnContext, cancellationToken);

                var results = await dc.ContinueDialogAsync(cancellationToken);
                if (results.Status == DialogTurnStatus.Empty)
                {
                    await dc.PromptAsync("OAuthPrompt", new PromptOptions(), cancellationToken: cancellationToken);
                }
                else if (results.Status == DialogTurnStatus.Complete)
                {
                    if (results.Result is TokenResponse)
                    {
                        await turnContext.SendActivityAsync(MessageFactory.Text("Logged in."), cancellationToken);
                    }
                    else
                    {
                        await turnContext.SendActivityAsync(MessageFactory.Text("Failed."), cancellationToken);
                    }
                }
            };

            await new TestFlow(adapter, botCallbackHandler)
            .Send("hello")
            .AssertReply(activity =>
            {
                Assert.AreEqual(1, ((Activity)activity).Attachments.Count);
                Assert.AreEqual(OAuthCard.ContentType, ((Activity)activity).Attachments[0].ContentType);
                Assert.AreEqual(InputHints.AcceptingInput, ((Activity)activity).InputHint);

                // No exchangable token is added to the adapter
            })
            .Send(new Activity()
            {
                Type = ActivityTypes.Invoke,
                Name = SignInConstants.TokenExchangeOperationName,

                // send no body
            })
            .AssertReply(a =>
            {
                Assert.AreEqual("invokeResponse", a.Type);
                var response = ((Activity)a).Value as InvokeResponse;
                Assert.IsNotNull(response);
                Assert.AreEqual(400, response.Status);
                var body = response.Body as TokenExchangeInvokeResponse;
                Assert.AreEqual(connectionName, body.ConnectionName);
                Assert.IsNotNull(body.FailureDetail);
            })
            .StartTestAsync();
        }

        [TestMethod]
        public async Task OAuthPromptWithTokenExchangeWrongConnectionNameFail()
        {
            var convoState = new ConversationState(new MemoryStorage());
            var dialogState = convoState.CreateProperty<DialogState>("dialogState");

            var adapter = new TestAdapter()
                .Use(new AutoSaveStateMiddleware(convoState));

            var connectionName = "myConnection";
            var exchangeToken = "exch123";

            // Create new DialogSet.
            var dialogs = new DialogSet(dialogState);
            dialogs.Add(new OAuthPrompt("OAuthPrompt", new OAuthPromptSettings() { Text = "Please sign in", ConnectionName = connectionName, Title = "Sign in" }));

            BotCallbackHandler botCallbackHandler = async (turnContext, cancellationToken) =>
            {
                var dc = await dialogs.CreateContextAsync(turnContext, cancellationToken);

                var results = await dc.ContinueDialogAsync(cancellationToken);
                if (results.Status == DialogTurnStatus.Empty)
                {
                    await dc.PromptAsync("OAuthPrompt", new PromptOptions(), cancellationToken: cancellationToken);
                }
                else if (results.Status == DialogTurnStatus.Complete)
                {
                    if (results.Result is TokenResponse)
                    {
                        await turnContext.SendActivityAsync(MessageFactory.Text("Logged in."), cancellationToken);
                    }
                    else
                    {
                        await turnContext.SendActivityAsync(MessageFactory.Text("Failed."), cancellationToken);
                    }
                }
            };

            await new TestFlow(adapter, botCallbackHandler)
            .Send("hello")
            .AssertReply(activity =>
            {
                Assert.AreEqual(1, ((Activity)activity).Attachments.Count);
                Assert.AreEqual(OAuthCard.ContentType, ((Activity)activity).Attachments[0].ContentType);
                Assert.AreEqual(InputHints.AcceptingInput, ((Activity)activity).InputHint);

                // No exchangable token is added to the adapter
            })
            .Send(new Activity()
            {
                Type = ActivityTypes.Invoke,
                Name = SignInConstants.TokenExchangeOperationName,
                Value = JObject.FromObject(new TokenExchangeInvokeRequest()
                {
                    ConnectionName = "beepboop",
                    Token = exchangeToken
                })
            })
            .AssertReply(a =>
            {
                Assert.AreEqual("invokeResponse", a.Type);
                var response = ((Activity)a).Value as InvokeResponse;
                Assert.IsNotNull(response);
                Assert.AreEqual(400, response.Status);
                var body = response.Body as TokenExchangeInvokeResponse;
                Assert.AreEqual(connectionName, body.ConnectionName);
                Assert.IsNotNull(body.FailureDetail);
            })
            .StartTestAsync();
        }

        [TestMethod]
        public async Task TestAdapterTokenExchange()
        {
            var convoState = new ConversationState(new MemoryStorage());

            var adapter = new TestAdapter()
                .Use(new AutoSaveStateMiddleware(convoState));

            var connectionName = "myConnection";
            var exchangeToken = "exch123";
            var token = "abc123";

            BotCallbackHandler botCallbackHandler = async (turnContext, cancellationToken) =>
            {
                var userId = "fred";
                adapter.AddExchangeableToken(connectionName, turnContext.Activity.ChannelId, userId, exchangeToken, token);

                // Positive case: Token
                var result = await adapter.ExchangeTokenAsync(turnContext, connectionName, userId, new TokenExchangeRequest() { Token = exchangeToken });
                Assert.IsNotNull(result);
                Assert.AreEqual(token, result.Token);
                Assert.AreEqual(connectionName, result.ConnectionName);

                // Positive case: URI
                result = await adapter.ExchangeTokenAsync(turnContext, connectionName, userId, new TokenExchangeRequest() { Uri = exchangeToken });
                Assert.IsNotNull(result);
                Assert.AreEqual(token, result.Token);
                Assert.AreEqual(connectionName, result.ConnectionName);

                // Negative case: Token
                result = await adapter.ExchangeTokenAsync(turnContext, connectionName, userId, new TokenExchangeRequest() { Token = "beeboop" });
                Assert.IsNull(result);

                // Negative case: URI
                result = await adapter.ExchangeTokenAsync(turnContext, connectionName, userId, new TokenExchangeRequest() { Uri = "beeboop" });
                Assert.IsNull(result);
            };

            await new TestFlow(adapter, botCallbackHandler)
            .Send("hello")
            .StartTestAsync();
        }

        private async Task OAuthPrompt(IStorage storage)
        {
            var convoState = new ConversationState(storage);
            var dialogState = convoState.CreateProperty<DialogState>("dialogState");

            var adapter = new TestAdapter()
                .Use(new AutoSaveStateMiddleware(convoState));

            var connectionName = "myConnection";
            var token = "abc123";

            // Create new DialogSet.
            var dialogs = new DialogSet(dialogState);
            dialogs.Add(new OAuthPrompt("OAuthPrompt", new OAuthPromptSettings() { Text = "Please sign in", ConnectionName = connectionName, Title = "Sign in" }));

            BotCallbackHandler botCallbackHandler = async (turnContext, cancellationToken) =>
            {
                var dc = await dialogs.CreateContextAsync(turnContext, cancellationToken);

                var results = await dc.ContinueDialogAsync(cancellationToken);
                if (results.Status == DialogTurnStatus.Empty)
                {
                    await dc.PromptAsync("OAuthPrompt", new PromptOptions(), cancellationToken: cancellationToken);
                }
                else if (results.Status == DialogTurnStatus.Complete)
                {
                    if (results.Result is TokenResponse)
                    {
                        await turnContext.SendActivityAsync(MessageFactory.Text("Logged in."), cancellationToken);
                    }
                    else
                    {
                        await turnContext.SendActivityAsync(MessageFactory.Text("Failed."), cancellationToken);
                    }
                }
            };

            await new TestFlow(adapter, botCallbackHandler)
            .Send("hello")
            .AssertReply(activity =>
            {
                Assert.AreEqual(1, ((Activity)activity).Attachments.Count);
                Assert.AreEqual(OAuthCard.ContentType, ((Activity)activity).Attachments[0].ContentType);

                Assert.AreEqual(InputHints.AcceptingInput, ((Activity)activity).InputHint);

                // Prepare an EventActivity with a TokenResponse and send it to the botCallbackHandler
                var eventActivity = CreateEventResponse(adapter, activity, connectionName, token);
                var ctx = new TurnContext(adapter, (Activity)eventActivity);
                botCallbackHandler(ctx, CancellationToken.None);
            })
            .AssertReply("Logged in.")
            .StartTestAsync();
        }

        private async Task PromptTimeoutEndsDialogTest(IActivity oauthPromptActivity)
        {
            var convoState = new ConversationState(new MemoryStorage());
            var dialogState = convoState.CreateProperty<DialogState>("dialogState");

            var adapter = new TestAdapter()
                .Use(new AutoSaveStateMiddleware(convoState));

            var connectionName = "myConnection";
            var exchangeToken = "exch123";
            var magicCode = "888999";
            var token = "abc123";

            // Create new DialogSet.
            var dialogs = new DialogSet(dialogState);
            
            // Set timeout to zero, so the prompt will end immediately.
            dialogs.Add(new OAuthPrompt("OAuthPrompt", new OAuthPromptSettings() { Text = "Please sign in", ConnectionName = connectionName, Title = "Sign in", Timeout = 0 }));

            BotCallbackHandler botCallbackHandler = async (turnContext, cancellationToken) =>
            {
                var dc = await dialogs.CreateContextAsync(turnContext, cancellationToken);

                var results = await dc.ContinueDialogAsync(cancellationToken);
                if (results.Status == DialogTurnStatus.Empty)
                {
                    await dc.PromptAsync("OAuthPrompt", new PromptOptions(), cancellationToken: cancellationToken);
                }
                else if (results.Status == DialogTurnStatus.Complete)
                {
                    // If the TokenResponse comes back, the timeout did not occur.
                    if (results.Result is TokenResponse)
                    {
                        await turnContext.SendActivityAsync("failed", cancellationToken: cancellationToken);
                    }
                    else
                    {
                        await turnContext.SendActivityAsync("ended", cancellationToken: cancellationToken);
                    }
                }
            };

            await new TestFlow(adapter, botCallbackHandler)
            .Send("hello")
            .AssertReply(activity =>
            {
                Assert.AreEqual(1, ((Activity)activity).Attachments.Count);
                Assert.AreEqual(OAuthCard.ContentType, ((Activity)activity).Attachments[0].ContentType);

                // Add a magic code to the adapter
                adapter.AddUserToken(connectionName, activity.ChannelId, activity.Recipient.Id, token, magicCode);

                // Add an exchangable token to the adapter
                adapter.AddExchangeableToken(connectionName, activity.ChannelId, activity.Recipient.Id, exchangeToken, token);
            })
            .Send(oauthPromptActivity)
            .AssertReply("ended")
            .StartTestAsync();
        }

        private Activity CreateEventResponse(TestAdapter adapter, IActivity activity, string connectionName, string token)
        {
            // add the token to the TestAdapter
            adapter.AddUserToken(connectionName, activity.ChannelId, activity.Recipient.Id, token);

            // send an event TokenResponse activity to the botCallback handler
            var eventActivity = ((Activity)activity).CreateReply();
            eventActivity.Type = ActivityTypes.Event;
            var from = eventActivity.From;
            eventActivity.From = eventActivity.Recipient;
            eventActivity.Recipient = from;
            eventActivity.Name = SignInConstants.TokenResponseEventName;
            eventActivity.Value = JObject.FromObject(new TokenResponse()
            {
                ConnectionName = connectionName,
                Token = token,
            });

            return eventActivity;
        }
    }
}
