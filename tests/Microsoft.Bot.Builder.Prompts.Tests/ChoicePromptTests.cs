// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Core.Extensions;
using Microsoft.Bot.Builder.Testing;
using Microsoft.Bot.Schema;
using Microsoft.Recognizers.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static Microsoft.Bot.Builder.Prompts.PromptValidatorEx;

namespace Microsoft.Bot.Builder.Prompts.Tests
{
    [TestClass]
    [TestCategory("Prompts")]
    [TestCategory("Choice Prompts")]
    public class ChoicePromptTests
    {
        private List<string> colorChoices = new List<string> { "red", "green", "blue" };

        private Action<IActivity> StartsWithValidator(string expected)
        {
            return activity =>
            {
                Assert.IsInstanceOfType(activity, typeof(IMessageActivity));
                var msg = (IMessageActivity)activity;
                Assert.IsTrue(msg.Text.StartsWith(expected));
            };
        }

        private Action<IActivity> SuggestedActionsValidator(string expectedText, SuggestedActions expectedSuggestedActions)
        {
            return activity =>
            {
                Assert.IsInstanceOfType(activity, typeof(IMessageActivity));
                var msg = (IMessageActivity)activity;
                Assert.AreEqual(expectedText, msg.Text);
                Assert.AreEqual(expectedSuggestedActions.Actions.Count, msg.SuggestedActions.Actions.Count);
                for (int i = 0; i < expectedSuggestedActions.Actions.Count; i++)
                {
                    Assert.AreEqual(expectedSuggestedActions.Actions[i].Type, msg.SuggestedActions.Actions[i].Type);
                    Assert.AreEqual(expectedSuggestedActions.Actions[i].Value, msg.SuggestedActions.Actions[i].Value);
                    Assert.AreEqual(expectedSuggestedActions.Actions[i].Title, msg.SuggestedActions.Actions[i].Title);
                }
            };
        }

        private Action<IActivity> SpeakValidator(string expectedText, string expectedSpeak)
        {
            return activity =>
            {
                Assert.IsInstanceOfType(activity, typeof(IMessageActivity));
                var msg = (IMessageActivity)activity;
                Assert.AreEqual(expectedText, msg.Text);
                Assert.AreEqual(expectedSpeak, msg.Speak);
            };
        }

        [TestMethod]
        public async Task ShouldSendPrompt()
        {
            await new TestFlow(new TestAdapter(), async (context) =>
            {
                var choicePrompt = new ChoicePrompt(Culture.English);
                await choicePrompt.Prompt(context, colorChoices, "favorite color?");
            })
            .Send("hello")
            .AssertReply(StartsWithValidator("favorite color?"))
            .StartTest();
        }

        [TestMethod]
        public async Task ShouldSendPromptAsAnInlineList()
        {
            await new TestFlow(new TestAdapter(), async (context) =>
            {
                var choicePrompt = new ChoicePrompt(Culture.English);
                choicePrompt.Style = ListStyle.Inline;
                await choicePrompt.Prompt(context, colorChoices, "favorite color?");
            })
            .Send("hello")
            .AssertReply("favorite color? (1) red, (2) green, or (3) blue")
            .StartTest();
        }

        [TestMethod]
        public async Task ShouldSendPromptAsANumberedList()
        {
            await new TestFlow(new TestAdapter(), async (context) =>
            {
                var choicePrompt = new ChoicePrompt(Culture.English);
                choicePrompt.Style = ListStyle.List;
                await choicePrompt.Prompt(context, colorChoices, "favorite color?");
            })
            .Send("hello")
            .AssertReply("favorite color?\n\n   1. red\n   2. green\n   3. blue")
            .StartTest();
        }

        [TestMethod]
        public async Task ShouldSendPromptUsingSuggestedActions()
        {
            await new TestFlow(new TestAdapter(), async (context) =>
            {
                var choicePrompt = new ChoicePrompt(Culture.English);
                choicePrompt.Style = ListStyle.SuggestedAction;
                await choicePrompt.Prompt(context, colorChoices, "favorite color?");
            })
            .Send("hello")
            .AssertReply(SuggestedActionsValidator("favorite color?",
                new SuggestedActions
                {
                    Actions = new List<CardAction>
                    {
                        new CardAction { Type="imBack", Value="red", Title="red" },
                        new CardAction { Type="imBack", Value="green", Title="green" },
                        new CardAction { Type="imBack", Value="blue", Title="blue" },
                    }
                }))
            .StartTest();
        }

        [TestMethod]
        public async Task ShouldSendPromptWithoutAddingAList()
        {
            await new TestFlow(new TestAdapter(), async (context) =>
            {
                var choicePrompt = new ChoicePrompt(Culture.English);
                choicePrompt.Style = ListStyle.None;
                await choicePrompt.Prompt(context, colorChoices, "favorite color?");
            })
            .Send("hello")
            .AssertReply("favorite color?")
            .StartTest();
        }

        [TestMethod]
        public async Task ShouldSendPromptWithoutAddingAListButAddingSsml()
        {
            await new TestFlow(new TestAdapter(), async (context) =>
            {
                var choicePrompt = new ChoicePrompt(Culture.English);
                choicePrompt.Style = ListStyle.None;
                await choicePrompt.Prompt(context, colorChoices, "favorite color?", "spoken prompt");
            })
            .Send("hello")
            .AssertReply(SpeakValidator("favorite color?", "spoken prompt"))
            .StartTest();
        }

        [TestMethod]
        public async Task ShouldSendActivityBasedPrompt()
        {
            await new TestFlow(new TestAdapter(), async (context) =>
            {
                var choicePrompt = new ChoicePrompt(Culture.English);
                await choicePrompt.Prompt(context, MessageFactory.Text("test"));
            })
            .Send("hello")
            .AssertReply("test")
            .StartTest();
        }

        [TestMethod]
        public async Task ShouldSendActivityBasedPromptWithSsml()
        {
            await new TestFlow(new TestAdapter(), async (context) =>
            {
                var choicePrompt = new ChoicePrompt(Culture.English);
                await choicePrompt.Prompt(context, MessageFactory.Text("test"), "spoken test");
            })
            .Send("hello")
            .AssertReply(SpeakValidator("test", "spoken test"))
            .StartTest();
        }

        [TestMethod]
        public async Task ShouldRecognizeAChoice()
        {
            var adapter = new TestAdapter()
                .Use(new ConversationState<TestState>(new MemoryStorage()));

            await new TestFlow(adapter, async (context) =>
            {
                var state = ConversationState<TestState>.Get(context);

                var choicePrompt = new ChoicePrompt(Culture.English);
                if (!state.InPrompt)
                {
                    state.InPrompt = true;
                    await choicePrompt.Prompt(context, colorChoices, "favorite color?");
                }
                else
                {
                    var choiceResult = await choicePrompt.Recognize(context, colorChoices);
                    if (choiceResult.Succeeded())
                    {
                        await context.SendActivity(choiceResult.Value.Value.ToString());
                    }
                    else
                        await context.SendActivity(choiceResult.Status.ToString());
                }
            })
            .Send("hello")
            .AssertReply(StartsWithValidator("favorite color?"))
            .Send("red")
            .AssertReply("red")
            .StartTest();
        }

        [TestMethod]
        public async Task ShouldNOTrecognizeOtherText()
        {
            var adapter = new TestAdapter()
                .Use(new ConversationState<TestState>(new MemoryStorage()));

            await new TestFlow(adapter, async (context) =>
            {
                var state = ConversationState<TestState>.Get(context);

                var choicePrompt = new ChoicePrompt(Culture.English);
                if (!state.InPrompt)
                {
                    state.InPrompt = true;
                    await choicePrompt.Prompt(context, colorChoices, "favorite color?");
                }
                else
                {
                    var choiceResult = await choicePrompt.Recognize(context, colorChoices);
                    if (choiceResult.Succeeded())
                    {
                        await context.SendActivity(choiceResult.Value.Value.ToString());
                    }
                    else
                        await context.SendActivity(choiceResult.Status);
                }
            })
            .Send("hello")
            .AssertReply(StartsWithValidator("favorite color?"))
            .Send("what was that?")
            .AssertReply("NotRecognized")
            .StartTest();
        }

        [TestMethod]
        public async Task ShouldCallCustomValidator()
        {
            var adapter = new TestAdapter()
                .Use(new ConversationState<TestState>(new MemoryStorage()));

            PromptValidator<ChoiceResult> validator = (ITurnContext context, ChoiceResult result) =>
            {
                result.Status = "validation failed";
                result.Value = null;
                return Task.CompletedTask;
            };

            await new TestFlow(adapter, async (context) =>
            {
                var state = ConversationState<TestState>.Get(context);

                var choicePrompt = new ChoicePrompt(Culture.English, validator);
                if (!state.InPrompt)
                {
                    state.InPrompt = true;
                    await choicePrompt.Prompt(context, colorChoices, "favorite color?");
                }
                else
                {
                    var choiceResult = await choicePrompt.Recognize(context, colorChoices);
                    if (choiceResult.Succeeded())
                    {
                        await context.SendActivity(choiceResult.Value.Value.ToString());
                    }
                    else
                        await context.SendActivity(choiceResult.Status);
                }
            })
            .Send("hello")
            .AssertReply(StartsWithValidator("favorite color?"))
            .Send("I'll take the red please.")
            .AssertReply("validation failed")
            .StartTest();
        }

        [TestMethod]
        public async Task ShouldHandleAnUndefinedRequest()
        {
            var adapter = new TestAdapter()
                .Use(new ConversationState<TestState>(new MemoryStorage()));

            PromptValidator<ChoiceResult> validator = (ITurnContext context, ChoiceResult result) =>
            {
                Assert.IsTrue(false);
                return Task.CompletedTask;
            };

            await new TestFlow(adapter, async (context) =>
            {
                var state = ConversationState<TestState>.Get(context);

                var choicePrompt = new ChoicePrompt(Culture.English, validator);
                if (!state.InPrompt)
                {
                    state.InPrompt = true;
                    await choicePrompt.Prompt(context, colorChoices, "favorite color?");
                }
                else
                {
                    var choiceResult = await choicePrompt.Recognize(context, colorChoices);
                    if (choiceResult.Succeeded())
                    {
                        await context.SendActivity(choiceResult.Value.Value.ToString());
                    }
                    else
                        await context.SendActivity(choiceResult.Status);
                }
            })
            .Send("hello")
            .AssertReply(StartsWithValidator("favorite color?"))
            .Send("value shouldn't have been recognized.")
            .AssertReply("NotRecognized")
            .StartTest();
        }
    }
}
