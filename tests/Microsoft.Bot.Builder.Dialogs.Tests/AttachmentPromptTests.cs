// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Core.Extensions;
using Microsoft.Bot.Builder.Prompts;
using Microsoft.Bot.Builder.Testing;
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
            TestAdapter adapter = new TestAdapter()
                .Use(new ConversationState<Dictionary<string, object>>(new MemoryStorage()));

            var attachment = new Attachment { Content = "some content", ContentType = "text/plain" };
            var activityWithAttachment = MessageFactory.Attachment(attachment);

            await new TestFlow(adapter, async (turnContext) =>
            {
                var state = ConversationState<Dictionary<string, object>>.Get(turnContext);
                var prompt = new AttachmentPrompt();

                var dialogCompletion = await prompt.Continue(turnContext, state);
                if (!dialogCompletion.IsActive && !dialogCompletion.IsCompleted)
                {
                    await prompt.Begin(turnContext, state, new PromptOptions { PromptString = "please add an attachment." });
                }
                else if (dialogCompletion.IsCompleted)
                {
                    var attachmentResult = (AttachmentResult)dialogCompletion.Result;
                    var reply = (string)attachmentResult.Attachments.First().Content;
                    await turnContext.SendActivity(reply);
                }
            })
            .Send("hello")
            .AssertReply("please add an attachment.")
            .Send(activityWithAttachment)
            .AssertReply("some content")
            .StartTest();
        }
    }
}
