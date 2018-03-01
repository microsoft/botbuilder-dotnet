// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Middleware;
using Microsoft.Bot.Builder.Storage;
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
                .Use(new ConversationState<StoreItem>(new MemoryStorage()));

            await new TestFlow(adapter, MyTestPrompt)
                .Send("hello")
                .AssertReply("Your Name:")
                .Send("test test test")
                .AssertReply("Passed")
                .AssertReply("test test test")                
                .StartTest();
        }

        [TestMethod]
        public async Task MinLenghtViaCustomValidator_Fail()
        {
            TestAdapter adapter = new TestAdapter()
                .Use(new ConversationState<StoreItem>(new MemoryStorage()));

            await new TestFlow(adapter, LengthCheckPromptTest)
                .Send("hello")
                .AssertReply("Your Name:")
                .Send("1")
                .AssertReply("Failed")                
                .StartTest();
        }
        [TestMethod]
        public async Task MinLenghtViaCustomValidator_Pass()
        {
            TestAdapter adapter = new TestAdapter()
                .Use(new ConversationState<StoreItem>(new MemoryStorage()));

            await new TestFlow(adapter, LengthCheckPromptTest)
                .Send("hello")
                .AssertReply("Your Name:")
                .Send("123456")
                .AssertReply("Passed")
                .AssertReply("123456")
                .StartTest();
        }


        [TestMethod]
        public async Task FailOnWhitespace()
        {
            TestAdapter adapter = new TestAdapter()
                .Use(new ConversationState<StoreItem>(new MemoryStorage()));

            await new TestFlow(adapter, MyTestPrompt)
                .Send("hello")
                .AssertReply("Your Name:")
                .Send(" ")
                .AssertReply("Failed")                
                .StartTest();
        }

        public async Task MyTestPrompt(IBotContext context)
        {
            dynamic conversationState = ConversationState<StoreItem>.Get(context);
            TextPrompt askForName = new TextPrompt();
            if (conversationState["topic"] != "textPromptTest")
            {
                conversationState["topic"] = "textPromptTest";                
                await askForName.Prompt(context, "Your Name:");
            }
            else
            {
                var text = await askForName.Recognize(context); 
                if (text != null)
                {
                    context.Reply("Passed");
                    context.Reply(text);
                }
                else
                {
                    context.Reply("Failed"); 
                }
            }
        }

        public async Task LengthCheckPromptTest(IBotContext context)
        {
            dynamic conversationState = ConversationState<StoreItem>.Get(context);
            TextPrompt askForName = new TextPrompt(MinLengthValidator);
            if (conversationState["topic"] != "textPromptTest")
            {
                conversationState["topic"] = "textPromptTest";
                await askForName.Prompt(context, "Your Name:");
            }
            else
            {
                var text = await askForName.Recognize(context);
                if (text != null)
                {
                    context.Reply("Passed");
                    context.Reply(text);
                }
                else
                {
                    context.Reply("Failed");
                }
            }
        }

        public async Task<bool> MinLengthValidator(IBotContext context, string toValidate)
        {
            return toValidate.Length > 5; 
        }
    }
}