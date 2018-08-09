// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Dialogs.Tests
{
    [TestClass]
    public class AttachmentPromptTests
    {

        [TestMethod]
        public async Task BasicAttachmentPrompt()
        {
            ConversationState convoState = new ConversationState(new MemoryStorage());
            var dialogState = convoState.CreateProperty<DialogState>("dialogState");

            TestAdapter adapter = new TestAdapter()
                .Use(convoState);

            // Create new DialogSet.
            DialogSet dialogs = new DialogSet(dialogState);

            // Create and add attachment prompt to DialogSet.
            var attachmentPrompt = new AttachmentPrompt("AttachmentPrompt");
            dialogs.Add(attachmentPrompt);

            // Create mock attachment for testing.
            var attachment = new Attachment { Content = "some content", ContentType = "text/plain" };

            // Create incoming activity with attachment.
            var activityWithAttachment = new Activity { Type = ActivityTypes.Message, Attachments = new List<Attachment> { attachment } };

            await new TestFlow(adapter, async (turnContext, cancellationToken) =>
            {
                var dc = await dialogs.CreateContextAsync(turnContext);

                var results = await dc.ContinueAsync();
                if (!turnContext.Responded && !results.HasActive && !results.HasResult)
                {
                    var options = new PromptOptions { Prompt = new Activity { Type = ActivityTypes.Message, Text = "please add an attachment." } };
                    await dc.PromptAsync("AttachmentPrompt", options);
                }
                else if (!results.HasActive && results.HasResult)
                {
                    var attachments = results.Result as List<Attachment>;
                    var content = (string)attachments[0].Content;
                    await turnContext.SendActivityAsync(content);

                }
            })
            .Send("hello")
            .AssertReply("please add an attachment.")
            .Send(activityWithAttachment)
            .AssertReply("some content")
            .StartTestAsync();
        }

        [TestMethod]
        public async Task RetryAttachmentPrompt()
        {
            ConversationState convoState = new ConversationState(new MemoryStorage());
            var dialogState = convoState.CreateProperty<DialogState>("dialogState");

            TestAdapter adapter = new TestAdapter()
                .Use(convoState);

            // Create new DialogSet.
            DialogSet dialogs = new DialogSet(dialogState);

            dialogs.Add(new AttachmentPrompt("AttachmentPrompt"));

            dialogs.Add(new WaterfallDialog("AttachmentDialog", new WaterfallStep[]
                    {
                        async (dc, step) =>
                        {
                            return await dc.PromptAsync("AttachmentPrompt", "please add an attachment.");
                        },
                        async (dc, step) =>
                        {
                            var results = step.Result as List<Attachment>;
                            var reply = (string)results[0].Content;
                            await dc.Context.SendActivityAsync(reply);
                            return await dc.EndAsync();
                        }
                    }
                ));
            // Create mock attachment for testing.
            var attachment = new Attachment { Content = "some content", ContentType = "text/plain" };

            // Create incoming activity with attachment.
            var activityWithAttachment = MessageFactory.Attachment(attachment);

            await new TestFlow(adapter, async (turnContext, cancellationToken) =>
            {
                var dc = await dialogs.CreateContextAsync(turnContext);
                await dc.ContinueAsync();
                if (!turnContext.Responded)
                {
                    await dc.BeginAsync("AttachmentDialog");
                }
            })
            .Send("hello")
            .AssertReply("please add an attachment.")
            .Send("hello again")
            .AssertReply("please add an attachment.")
            .Send(activityWithAttachment)
            .AssertReply("some content")
            .StartTestAsync();
        }
    }
}
