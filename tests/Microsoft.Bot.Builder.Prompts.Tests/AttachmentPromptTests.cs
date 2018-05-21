// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Core.Extensions;
using Microsoft.Bot.Builder.Core.Extensions.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Prompts.Tests
{
    [TestClass]
    [TestCategory("Prompts")]
    [TestCategory("Attachment Prompts")]
    public class AttachmentPromptTests
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        public async Task AttachmentPrompt_ShouldSendPrompt()
        {
            var activities = TranscriptUtilities.GetFromTestContext(TestContext);

            var flow = new TestFlow(new TestAdapter(), async (context) =>
            {
                var attachmentPrompt = new AttachmentPrompt();
                await attachmentPrompt.Prompt(context, "please add an attachment.");
            });

            await flow.Test(activities).StartTest();
        }

        [TestMethod]
        public async Task AttachmentPrompt_ShouldRecognizeAttachment()
        {
            var activities = TranscriptUtilities.GetFromTestContext(TestContext);
            
            var adapter = new TestAdapter()
                .Use(new ConversationState<TestState>(new MemoryStorage()));

            var flow = new TestFlow(adapter, async (context) =>
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
                        var reply = attachmentResult.Attachments.First().Content as string;
                        await context.SendActivity(reply);
                    }
                    else
                        await context.SendActivity(attachmentResult.Status.ToString());
                }
            });

            await flow.Test(activities).StartTest();
        }
    }
}
