// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Core.Extensions;
using Microsoft.Bot.Builder.Testing;
using Microsoft.Bot.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Prompts.Tests
{
    [TestClass]
    [TestCategory("Prompts")]
    [TestCategory("Attachment Prompts")]
    public class AttachmentPromptTests
    {
        [TestMethod]
        public async Task AttachmentPrompt_ShouldSendPrompt()
        {
            await new TestFlow(new TestAdapter(), async (context) =>
            {
                var attachmentPrompt = new AttachmentPrompt();
                await attachmentPrompt.Prompt(context, "please add an attachment.");
            })
            .Send("hello")
            .AssertReply("please add an attachment.")
            .StartTest();
        }

        [TestMethod]
        public async Task AttachmentPrompt_ShouldRecognizeAttachment()
        {
            var adapter = new TestAdapter()
                .Use(new ConversationState<TestState>(new MemoryStorage()));

            var attachment = new Attachment { Content = "some content", ContentType = "text/plain" };
            var activityWithAttachment = MessageFactory.Attachment(attachment);

            await new TestFlow(adapter, async (context) =>
            {
                var state = ConversationState<TestState>.Get(context);

                var attachmentPrompt = new AttachmentPrompt();
                if (!state.InPrompt)
                {
                    state.InPrompt = true;
                    await attachmentPrompt.Prompt(context, "please add an attachment.");
                }
                else
                {
                    var attachmentResult = await attachmentPrompt.Recognize(context);
                    if (attachmentResult.Succeeded())
                    {
                        var reply = (string)attachmentResult.Attachments.First().Content;
                        await context.SendActivity(reply);
                    }
                    else
                        await context.SendActivity(attachmentResult.Status.ToString());
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
