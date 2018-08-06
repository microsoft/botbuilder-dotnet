// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Linq;
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
            var testProperty = convoState.CreateProperty<Dictionary<string, object>>("test");

            TestAdapter adapter = new TestAdapter()
                .Use(convoState);

            var attachment = new Attachment { Content = "some content", ContentType = "text/plain" };
            var activityWithAttachment = MessageFactory.Attachment(attachment);

            await new TestFlow(adapter, async (turnContext, cancellationToken) =>
            {
                var state = await testProperty.GetAsync(turnContext, () => new Dictionary<string, object>());
                var prompt = new AttachmentPrompt();

                var dialogCompletion = await prompt.ContinueAsync(turnContext, state);
                if (!dialogCompletion.IsActive && !dialogCompletion.IsCompleted)
                {
                    await prompt.BeginAsync(turnContext, state, new PromptOptions { PromptString = "please add an attachment." });
                }
                else if (dialogCompletion.IsCompleted)
                {
                    var attachmentResult = (AttachmentResult)dialogCompletion.Result;
                    var reply = (string)attachmentResult.Attachments.First().Content;
                    await turnContext.SendActivityAsync(reply);
                }
            })
            .Send("hello")
            .AssertReply("please add an attachment.")
            .Send(activityWithAttachment)
            .AssertReply("some content")
            .StartTestAsync();
        }
    }
}
