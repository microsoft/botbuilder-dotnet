// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Dialogs.Tests
{
    [TestClass]
    public class OAuthPromptTests
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void OAuthPromptWithEmptyIdShouldFail()
        {
            var emptyId = string.Empty;
            var confirmPrompt = new OAuthPrompt(emptyId, new OAuthPromptSettings());
        }

        [TestMethod]
        public async Task OAuthPrompt()
        {
            var convoState = new ConversationState(new MemoryStorage());
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
            eventActivity.Name = "tokens/response";
            eventActivity.Value = JObject.FromObject(new TokenResponse()
            {
                ConnectionName = connectionName,
                Token = token,
            });

            return eventActivity;
        }
    }
}
