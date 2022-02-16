// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Schema;
using Xunit;

namespace Microsoft.Bot.Builder.Dialogs.Tests
{
    public class AttachmentPromptTests
    {
        [Fact]
        public void AttachmentPromptWithEmptyIdShouldFail()
        {
            Assert.Throws<ArgumentNullException>(() => new AttachmentPrompt(string.Empty));
        }

        [Fact]
        public void AttachmentPromptWithNullIdShouldFail()
        {
            Assert.Throws<ArgumentNullException>(() => new AttachmentPrompt(null));
        }

        [Fact]
        public async Task BasicAttachmentPrompt()
        {
            var convoState = new ConversationState(new MemoryStorage());
            var dialogState = convoState.CreateProperty<DialogState>("dialogState");

            var adapter = new TestAdapter(TestAdapter.CreateConversation(nameof(BasicAttachmentPrompt)))
                .Use(new AutoSaveStateMiddleware(convoState))
                .Use(new TranscriptLoggerMiddleware(new TraceTranscriptLogger(traceActivity: false)));

            // Create new DialogSet.
            var dialogs = new DialogSet(dialogState);

            // Create and add attachment prompt to DialogSet.
            var attachmentPrompt = new AttachmentPrompt("AttachmentPrompt");
            dialogs.Add(attachmentPrompt);

            // Create mock attachment for testing.
            var attachment = new Attachment { Content = "some content", ContentType = "text/plain" };

            // Create incoming activity with attachment.
            var activityWithAttachment = new Activity { Type = ActivityTypes.Message };
            activityWithAttachment.Attachments.Add(attachment);

            await new TestFlow(adapter, async (turnContext, cancellationToken) =>
            {
                var dc = await dialogs.CreateContextAsync(turnContext, cancellationToken);

                var results = await dc.ContinueDialogAsync();
                if (results.Status == DialogTurnStatus.Empty)
                {
                    var options = new PromptOptions { Prompt = new Activity { Type = ActivityTypes.Message, Text = "please add an attachment." } };
                    await dc.PromptAsync("AttachmentPrompt", options, cancellationToken);
                }
                else if (results.Status == DialogTurnStatus.Complete)
                {
                    var attachments = results.Result as List<Attachment>;
                    var content = MessageFactory.Text((string)attachments[0].Content);
                    await turnContext.SendActivityAsync(content, cancellationToken);
                }
            })
            .Send("hello")
            .AssertReply("please add an attachment.")
            .Send(activityWithAttachment)
            .AssertReply("some content")
            .StartTestAsync();
        }

        [Fact]
        public async Task RetryAttachmentPrompt()
        {
            var convoState = new ConversationState(new MemoryStorage());
            var dialogState = convoState.CreateProperty<DialogState>("dialogState");

            var adapter = new TestAdapter(TestAdapter.CreateConversation(nameof(RetryAttachmentPrompt)))
                .Use(new AutoSaveStateMiddleware(convoState))
                .Use(new TranscriptLoggerMiddleware(new TraceTranscriptLogger(traceActivity: false)));

            // Create new DialogSet.
            var dialogs = new DialogSet(dialogState);

            dialogs.Add(new AttachmentPrompt("AttachmentPrompt"));

            // Create mock attachment for testing.
            var attachment = new Attachment { Content = "some content", ContentType = "text/plain" };

            // Create incoming activity with attachment.
            var activityWithAttachment = new Activity { Type = ActivityTypes.Message };
            activityWithAttachment.Attachments.Add(attachment);

            await new TestFlow(adapter, async (turnContext, cancellationToken) =>
            {
                var dc = await dialogs.CreateContextAsync(turnContext, cancellationToken);
                var results = await dc.ContinueDialogAsync(cancellationToken);
                if (results.Status == DialogTurnStatus.Empty)
                {
                    var options = new PromptOptions { Prompt = new Activity { Type = ActivityTypes.Message, Text = "please add an attachment." } };
                    await dc.PromptAsync("AttachmentPrompt", options, cancellationToken);
                }
                else if (results.Status == DialogTurnStatus.Complete)
                {
                    var attachments = results.Result as List<Attachment>;
                    var content = MessageFactory.Text((string)attachments[0].Content);
                    await turnContext.SendActivityAsync(content, cancellationToken);
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
