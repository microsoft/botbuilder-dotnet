// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Core.Extensions;
using Microsoft.Bot.Builder.Prompts;
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
                var dialogs = new DialogSet();
                dialogs.Add("test-prompt", new AttachmentPrompt());

                var state = ConversationState<Dictionary<string, object>>.Get(turnContext);
                var dc = dialogs.CreateContext(turnContext, state);

                await dc.Continue();
                var dialogResult = dc.DialogResult;

                if (!dialogResult.Active)
                {
                    if (dialogResult.Result != null)
                    {
                        var attachmentResult = (AttachmentResult)dialogResult.Result;
                        var reply = (string)attachmentResult.Attachments.First().Content;
                        await turnContext.SendActivity(reply);
                    }
                    else
                    {
                        await dc.Prompt("test-prompt", "please add an attachment.");
                    }
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
