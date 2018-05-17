// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Core.Extensions;
using Microsoft.Bot.Builder.Core.Extensions.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Prompts.Tests
{
    [TestClass]
    [TestCategory("Prompts")]
    [TestCategory("Text Prompts")]
    public class TextPromptTests
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        public async Task SimpleRecognize()
        {
            var activities = TranscriptUtilities.GetFromTestContext(TestContext);

            TestAdapter adapter = new TestAdapter()
                .Use(new ConversationState<PromptState>(new MemoryStorage()));

            var flow = new TestFlow(adapter, MyTestPrompt);

            await flow.Test(activities).StartTest();
        }

        [TestMethod]
        public async Task MinLenghtViaCustomValidator_Fail()
        {
            var activities = TranscriptUtilities.GetFromTestContext(TestContext);

            TestAdapter adapter = new TestAdapter()
                .Use(new ConversationState<PromptState>(new MemoryStorage()));

            var flow = new TestFlow(adapter, LengthCheckPromptTest);

            await flow.Test(activities).StartTest();
        }

        [TestMethod]
        public async Task MinLenghtViaCustomValidator_Pass()
        {
            var activities = TranscriptUtilities.GetFromTestContext(TestContext);

            TestAdapter adapter = new TestAdapter()
                .Use(new ConversationState<PromptState>(new MemoryStorage()));

            var flow = new TestFlow(adapter, LengthCheckPromptTest);

            await flow.Test(activities).StartTest();
        }


        public async Task MyTestPrompt(ITurnContext context)
        {
            var conversationState = context.GetConversationState<PromptState>();
            TextPrompt askForName = new TextPrompt();
            if (conversationState.Topic != "textPromptTest")
            {
                conversationState.Topic = "textPromptTest";                
                await askForName.Prompt(context, "Your Name:");
            }
            else
            {
                var textResult = await askForName.Recognize(context); 
                if (textResult.Succeeded())
                {
                    await context.SendActivity(textResult.Value);
                }
                else
                {
                    await context.SendActivity(textResult.Status.ToString()); 
                }
            }
        }

        private class PromptState
        {
            public string Topic { get; set; }
        }

        public async Task LengthCheckPromptTest(ITurnContext context)
        {
            var conversationState = context.GetConversationState<PromptState>();
            TextPrompt askForName = new TextPrompt(MinLengthValidator);
            if (conversationState.Topic != "textPromptTest")
            {
                conversationState.Topic = "textPromptTest";
                await askForName.Prompt(context, "Your Name:");
            }
            else
            {
                var textResult = await askForName.Recognize(context);
                if (textResult.Succeeded())
                {
                    await context.SendActivity(textResult.Value);
                }
                else
                {
                    await context.SendActivity(textResult.Status.ToString());
                }
            }
        }

        public async Task MinLengthValidator(ITurnContext context, TextResult textResult)
        {
            if (textResult.Value.Length <= 5)
                textResult.Status = PromptStatus.TooSmall;
        }
    }
}