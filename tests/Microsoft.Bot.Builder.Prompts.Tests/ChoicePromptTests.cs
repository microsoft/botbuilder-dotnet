// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Core.Extensions;
using Microsoft.Bot.Builder.Core.Extensions.Tests;
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
        public TestContext TestContext { get; set; }

        private List<string> colorChoices = new List<string> { "red", "green", "blue" };
        
        [TestMethod]
        public async Task ShouldSendPrompt()
        {
            var activities = TranscriptUtilities.GetFromTestContext(TestContext);

            var flow = new TestFlow(new TestAdapter(), async (context) =>
            {
                var choicePrompt = new ChoicePrompt(Culture.English);
                await choicePrompt.Prompt(context, colorChoices, "favorite color?");
            });

            await flow.Test(activities, (expected, actual) => {
                Assert.IsTrue(actual.AsMessageActivity().Text.StartsWith(expected.AsMessageActivity().Text));
            }).StartTest();
        }

        [TestMethod]
        public async Task ShouldSendPromptAsAnInlineList()
        {
            var activities = TranscriptUtilities.GetFromTestContext(TestContext);

            var flow = new TestFlow(new TestAdapter(), async (context) =>
            {
                var choicePrompt = new ChoicePrompt(Culture.English)
                {
                    Style = ListStyle.Inline
                };
                await choicePrompt.Prompt(context, colorChoices, "favorite color?");
            });

            await flow.Test(activities).StartTest();
        }

        [TestMethod]
        public async Task ShouldSendPromptAsANumberedList()
        {
            var activities = TranscriptUtilities.GetFromTestContext(TestContext);

            var flow = new TestFlow(new TestAdapter(), async (context) =>
            {
                var choicePrompt = new ChoicePrompt(Culture.English)
                {
                    Style = ListStyle.List
                };
                await choicePrompt.Prompt(context, colorChoices, "favorite color?");
            });

            await flow.Test(activities).StartTest();
        }

        [TestMethod]
        public async Task ShouldSendPromptUsingSuggestedActions()
        {
            var activities = TranscriptUtilities.GetFromTestContext(TestContext);

            var flow = new TestFlow(new TestAdapter(), async (context) =>
            {
                var choicePrompt = new ChoicePrompt(Culture.English)
                {
                    Style = ListStyle.SuggestedAction
                };
                await choicePrompt.Prompt(context, colorChoices, "favorite color?");
            });

            await flow.Test(activities, (expected, actual) => {
                var expectedMessage = expected.AsMessageActivity();
                var actualMessage = actual.AsMessageActivity();
                Assert.AreEqual(expectedMessage.Text, actualMessage.Text);
                var actionPairs = expectedMessage.SuggestedActions.Actions.Zip(actualMessage.SuggestedActions.Actions, (a, b) => (a, b));
                foreach (var (expectedAction, actualAction) in actionPairs)
                {
                    Assert.AreEqual(expectedAction.Type, actualAction.Type);
                    Assert.AreEqual(expectedAction.Value, actualAction.Value);
                    Assert.AreEqual(expectedAction.Title, actualAction.Title);
                }
            }).StartTest();
        }

        [TestMethod]
        public async Task ShouldSendPromptWithoutAddingAList()
        {
            var activities = TranscriptUtilities.GetFromTestContext(TestContext);

            var flow = new TestFlow(new TestAdapter(), async (context) =>
            {
                var choicePrompt = new ChoicePrompt(Culture.English)
                {
                    Style = ListStyle.None
                };
                await choicePrompt.Prompt(context, colorChoices, "favorite color?");
            });

            await flow.Test(activities).StartTest();
        }

        [TestMethod]
        public async Task ShouldSendPromptWithoutAddingAListButAddingSsml()
        {
            var activities = TranscriptUtilities.GetFromTestContext(TestContext);

            var flow = new TestFlow(new TestAdapter(), async (context) =>
            {
                var choicePrompt = new ChoicePrompt(Culture.English)
                {
                    Style = ListStyle.None
                };
                await choicePrompt.Prompt(context, colorChoices, "favorite color?", "spoken prompt");
            });

            await flow.Test(activities, (expected, actual) => {
                var expectedMessage = expected.AsMessageActivity();
                var actualMessage = actual.AsMessageActivity();
                Assert.AreEqual(expectedMessage.Text, actualMessage.Text);
                Assert.AreEqual(expectedMessage.Speak, actualMessage.Speak);
            }).StartTest();
        }

        [TestMethod]
        public async Task ShouldSendActivityBasedPrompt()
        {
            var activities = TranscriptUtilities.GetFromTestContext(TestContext);

            var flow = new TestFlow(new TestAdapter(), async (context) =>
            {
                var choicePrompt = new ChoicePrompt(Culture.English);
                await choicePrompt.Prompt(context, MessageFactory.Text("test"));
            });

            await flow.Test(activities).StartTest();
        }

        [TestMethod]
        public async Task ShouldSendActivityBasedPromptWithSsml()
        {
            var activities = TranscriptUtilities.GetFromTestContext(TestContext);

            var flow = new TestFlow(new TestAdapter(), async (context) =>
            {
                var choicePrompt = new ChoicePrompt(Culture.English);
                await choicePrompt.Prompt(context, MessageFactory.Text("test"), "spoken test");
            });

            await flow.Test(activities, (expected, actual) => {
                var expectedMessage = expected.AsMessageActivity();
                var actualMessage = actual.AsMessageActivity();
                Assert.AreEqual(expectedMessage.Text, actualMessage.Text);
                Assert.AreEqual(expectedMessage.Speak, actualMessage.Speak);
            }).StartTest();
        }

        [TestMethod]
        public async Task ShouldRecognizeAChoice()
        {
            var activities = TranscriptUtilities.GetFromTestContext(TestContext);

            var adapter = new TestAdapter()
                .Use(new ConversationState<TestState>(new MemoryStorage()));

            var flow = new TestFlow(adapter, async (context) =>
            {
                var state = ConversationState<TestState>.Get(context);

                var choicePrompt = new ChoicePrompt(Culture.English)
                {
                    Style = ListStyle.None
                };
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
            });

            await flow.Test(activities).StartTest();
        }

        [TestMethod]
        public async Task ShouldNOTrecognizeOtherText()
        {
            var activities = TranscriptUtilities.GetFromTestContext(TestContext);

            var adapter = new TestAdapter()
                .Use(new ConversationState<TestState>(new MemoryStorage()));

            var flow = new TestFlow(adapter, async (context) =>
            {
                var state = ConversationState<TestState>.Get(context);

                var choicePrompt = new ChoicePrompt(Culture.English)
                {
                    Style = ListStyle.None
                };
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
            });

            await flow.Test(activities).StartTest();
        }

        [TestMethod]
        public async Task ShouldCallCustomValidator()
        {
            var activities = TranscriptUtilities.GetFromTestContext(TestContext);

            var adapter = new TestAdapter()
                .Use(new ConversationState<TestState>(new MemoryStorage()));

            PromptValidator<ChoiceResult> validator = (ITurnContext context, ChoiceResult result) =>
            {
                result.Status = "validation failed";
                result.Value = null;
                return Task.CompletedTask;
            };

            var flow = new TestFlow(adapter, async (context) =>
            {
                var state = ConversationState<TestState>.Get(context);

                var choicePrompt = new ChoicePrompt(Culture.English, validator)
                {
                    Style = ListStyle.None
                };
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
            });

            await flow.Test(activities).StartTest();
        }

        [TestMethod]
        public async Task ShouldHandleAnUndefinedRequest()
        {
            var activities = TranscriptUtilities.GetFromTestContext(TestContext);

            var adapter = new TestAdapter()
                .Use(new ConversationState<TestState>(new MemoryStorage()));

            PromptValidator<ChoiceResult> validator = (ITurnContext context, ChoiceResult result) =>
            {
                Assert.IsTrue(false);
                return Task.CompletedTask;
            };

            var flow = new TestFlow(adapter, async (context) =>
            {
                var state = ConversationState<TestState>.Get(context);

                var choicePrompt = new ChoicePrompt(Culture.English, validator)
                {
                    Style = ListStyle.None
                };
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
            });

            await flow.Test(activities).StartTest();
        }
    }
}
