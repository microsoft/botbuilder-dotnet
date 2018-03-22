// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Core.Extensions;
using Microsoft.Bot.Builder.Core.State;
using Microsoft.Bot.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Prompts.Tests
{
    [TestClass]
    [TestCategory("Prompts")]
    [TestCategory("Text Prompts")]
    public class TextPromptTests
    {
        [TestMethod]
        public async Task SimpleRecognize()
        {
            TestAdapter adapter = new TestAdapter()
                .Use(new StateManagementMiddleware()
                        .UseDefaultStorageProvider(new MemoryStateStorageProvider())
                        .UseConversationState());

            await new TestFlow(adapter, MyTestPrompt)
                .Send("hello")
                .AssertReply("Your Name:")
                .Send("test test test")
                .AssertReply("test test test")                
                .StartTest();
        }

        [TestMethod]
        public async Task MinLenghtViaCustomValidator_Fail()
        {
            TestAdapter adapter = new TestAdapter()
                .Use(new StateManagementMiddleware()
                        .UseDefaultStorageProvider(new MemoryStateStorageProvider())
                        .UseConversationState());

            await new TestFlow(adapter, LengthCheckPromptTest)
                .Send("hello")
                .AssertReply("Your Name:")
                .Send("1")
                .AssertReply(PromptStatus.TooSmall.ToString())                
                .StartTest();
        }
        [TestMethod]
        public async Task MinLenghtViaCustomValidator_Pass()
        {
            TestAdapter adapter = new TestAdapter()
                .Use(new StateManagementMiddleware()
                        .UseDefaultStorageProvider(new MemoryStateStorageProvider())
                        .UseConversationState());

            await new TestFlow(adapter, LengthCheckPromptTest)
                .Send("hello")
                .AssertReply("Your Name:")
                .Send("123456")
                .AssertReply("123456")
                .StartTest();
        }

        public class MyTestPromptState
        {
            public string Topic { get; set; }
        }

        public async Task MyTestPrompt(ITurnContext context)
        {
            var conversationState = context.ConversationState();
            var state = await conversationState.Get<MyTestPromptState>() ?? new MyTestPromptState();

            TextPrompt askForName = new TextPrompt();
            if (state.Topic != "textPromptTest")
            {
                state.Topic = "textPromptTest";

                conversationState.Set(state);
                await conversationState.SaveChanges();

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

        public async Task LengthCheckPromptTest(ITurnContext context)
        {
            var conversationState = context.ConversationState();
            var state = await conversationState.Get<MyTestPromptState>() ?? new MyTestPromptState();
            TextPrompt askForName = new TextPrompt(MinLengthValidator);
            if (state.Topic != "textPromptTest")
            {
                state.Topic = "textPromptTest";

                conversationState.Set(state);
                await conversationState.SaveChanges();

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