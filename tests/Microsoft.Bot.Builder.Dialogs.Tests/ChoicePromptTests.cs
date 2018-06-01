// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Core.Extensions;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Schema;
using Microsoft.Recognizers.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static Microsoft.Bot.Builder.Dialogs.PromptValidatorEx;

namespace Microsoft.Bot.Builder.Dialogs.Tests
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
            TestAdapter adapter = new TestAdapter()
                .Use(new ConversationState<Dictionary<string, object>>(new MemoryStorage()));

            await new TestFlow(adapter, async (turnContext) =>
            {
                var state = ConversationState<Dictionary<string, object>>.Get(turnContext);
                var prompt = new ChoicePrompt(Culture.English);

                var dialogCompletion = await prompt.Continue(turnContext, state);
                if (!dialogCompletion.IsActive && !dialogCompletion.IsCompleted)
                {
                    await prompt.Begin(turnContext, state,
                        new ChoicePromptOptions
                        {
                            PromptString = "favorite color?",
                            Choices = ChoiceFactory.ToChoices(colorChoices)
                        });
                }
            })
            .Send("hello")
            .AssertReply(StartsWithValidator("favorite color?"))
            .StartTest();
        }

        [TestMethod]
        public async Task ShouldSendPromptAsAnInlineList()
        {
            TestAdapter adapter = new TestAdapter()
                .Use(new ConversationState<Dictionary<string, object>>(new MemoryStorage()));

            await new TestFlow(adapter, async (turnContext) =>
            {
                var state = ConversationState<Dictionary<string, object>>.Get(turnContext);
                var prompt = new ChoicePrompt(Culture.English);
                prompt.Style = ListStyle.Inline;

                var dialogCompletion = await prompt.Continue(turnContext, state);
                if (!dialogCompletion.IsActive && !dialogCompletion.IsCompleted)
                {
                    await prompt.Begin(turnContext, state,
                        new ChoicePromptOptions
                        {
                            PromptString = "favorite color?",
                            Choices = ChoiceFactory.ToChoices(colorChoices)
                        });
                }
            })
            .Send("hello")
            .AssertReply("favorite color? (1) red, (2) green, or (3) blue")
            .StartTest();
        }

        [TestMethod]
        public async Task ShouldSendPromptAsANumberedList()
        {
            TestAdapter adapter = new TestAdapter()
                .Use(new ConversationState<Dictionary<string, object>>(new MemoryStorage()));

            await new TestFlow(adapter, async (turnContext) =>
            {
                var state = ConversationState<Dictionary<string, object>>.Get(turnContext);
                var prompt = new ChoicePrompt(Culture.English);
                prompt.Style = ListStyle.List;

                var dialogCompletion = await prompt.Continue(turnContext, state);
                if (!dialogCompletion.IsActive && !dialogCompletion.IsCompleted)
                {
                    await prompt.Begin(turnContext, state,
                        new ChoicePromptOptions
                        {
                            PromptString = "favorite color?",
                            Choices = ChoiceFactory.ToChoices(colorChoices)
                        });
                }
            })
            .Send("hello")
            .AssertReply("favorite color?\n\n   1. red\n   2. green\n   3. blue")
            .StartTest();
        }

        [TestMethod]
        public async Task ShouldSendPromptUsingSuggestedActions()
        {
            TestAdapter adapter = new TestAdapter()
                .Use(new ConversationState<Dictionary<string, object>>(new MemoryStorage()));

            await new TestFlow(adapter, async (turnContext) =>
            {
                var state = ConversationState<Dictionary<string, object>>.Get(turnContext);
                var prompt = new ChoicePrompt(Culture.English);
                prompt.Style = ListStyle.SuggestedAction;

                var dialogCompletion = await prompt.Continue(turnContext, state);
                if (!dialogCompletion.IsActive && !dialogCompletion.IsCompleted)
                {
                    await prompt.Begin(turnContext, state,
                        new ChoicePromptOptions
                        {
                            PromptString = "favorite color?",
                            Choices = ChoiceFactory.ToChoices(colorChoices)
                        });
                }
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
            TestAdapter adapter = new TestAdapter()
                .Use(new ConversationState<Dictionary<string, object>>(new MemoryStorage()));

            await new TestFlow(adapter, async (turnContext) =>
            {
                var state = ConversationState<Dictionary<string, object>>.Get(turnContext);
                var prompt = new ChoicePrompt(Culture.English);
                prompt.Style = ListStyle.None;

                var dialogCompletion = await prompt.Continue(turnContext, state);
                if (!dialogCompletion.IsActive && !dialogCompletion.IsCompleted)
                {
                    await prompt.Begin(turnContext, state,
                        new ChoicePromptOptions
                        {
                            PromptString = "favorite color?",
                            Choices = ChoiceFactory.ToChoices(colorChoices)
                        });
                }
            })
            .Send("hello")
            .AssertReply("favorite color?")
            .StartTest();
        }

        [TestMethod]
        public async Task ShouldSendPromptWithoutAddingAListButAddingSsml()
        {
            TestAdapter adapter = new TestAdapter()
                .Use(new ConversationState<Dictionary<string, object>>(new MemoryStorage()));

            await new TestFlow(adapter, async (turnContext) =>
            {
                var state = ConversationState<Dictionary<string, object>>.Get(turnContext);
                var prompt = new ChoicePrompt(Culture.English);
                prompt.Style = ListStyle.None;

                var dialogCompletion = await prompt.Continue(turnContext, state);
                if (!dialogCompletion.IsActive && !dialogCompletion.IsCompleted)
                {
                    await prompt.Begin(turnContext, state,
                        new ChoicePromptOptions
                        {
                            PromptString = "favorite color?",
                            Speak = "spoken prompt",
                            Choices = ChoiceFactory.ToChoices(colorChoices)
                        });
                }
            })
            .Send("hello")
            .AssertReply(SpeakValidator("favorite color?", "spoken prompt"))
            .StartTest();
        }

        [TestMethod]
        public async Task ShouldSendActivityBasedPrompt()
        {
            TestAdapter adapter = new TestAdapter()
                .Use(new ConversationState<Dictionary<string, object>>(new MemoryStorage()));

            await new TestFlow(adapter, async (turnContext) =>
            {
                var state = ConversationState<Dictionary<string, object>>.Get(turnContext);
                var prompt = new ChoicePrompt(Culture.English);
                prompt.Style = ListStyle.None;

                var dialogCompletion = await prompt.Continue(turnContext, state);
                if (!dialogCompletion.IsActive && !dialogCompletion.IsCompleted)
                {
                    await prompt.Begin(turnContext, state,
                        new ChoicePromptOptions
                        {
                            PromptActivity = MessageFactory.Text("test"),
                            Choices = ChoiceFactory.ToChoices(colorChoices)
                        });
                }
            })
            .Send("hello")
            .AssertReply("test")
            .StartTest();
        }

        [TestMethod]
        public async Task ShouldSendActivityBasedPromptWithSsml()
        {
            TestAdapter adapter = new TestAdapter()
                .Use(new ConversationState<Dictionary<string, object>>(new MemoryStorage()));

            await new TestFlow(adapter, async (turnContext) =>
            {
                var state = ConversationState<Dictionary<string, object>>.Get(turnContext);
                var prompt = new ChoicePrompt(Culture.English);

                var dialogCompletion = await prompt.Continue(turnContext, state);
                if (!dialogCompletion.IsActive && !dialogCompletion.IsCompleted)
                {
                    await prompt.Begin(turnContext, state,
                        new ChoicePromptOptions
                        {
                            // TODO: the current model adds the Speak to the activity - that seem surprising (and unnecessary) 
                            PromptActivity = MessageFactory.Text("test"),
                            Speak = "spoken test"
                        });
                }
            })
            .Send("hello")
            .AssertReply(SpeakValidator("test", "spoken test"))
            .StartTest();
        }

        [TestMethod]
        public async Task ShouldRecognizeAChoice()
        {
            TestAdapter adapter = new TestAdapter()
                .Use(new ConversationState<Dictionary<string, object>>(new MemoryStorage()));

            await new TestFlow(adapter, async (turnContext) =>
            {
                var state = ConversationState<Dictionary<string, object>>.Get(turnContext);
                var prompt = new ChoicePrompt(Culture.English);
                prompt.Style = ListStyle.None;

                var dialogCompletion = await prompt.Continue(turnContext, state);
                if (!dialogCompletion.IsActive && !dialogCompletion.IsCompleted)
                {
                    await prompt.Begin(turnContext, state,
                        new ChoicePromptOptions
                        {
                            PromptString = "favorite color?",
                            Choices = ChoiceFactory.ToChoices(colorChoices)
                        });
                }
                else if (dialogCompletion.IsCompleted)
                {
                    var choiceResult = (ChoiceResult)dialogCompletion.Result;
                    await turnContext.SendActivity($"{choiceResult.Value.Value}");
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
            TestAdapter adapter = new TestAdapter()
                .Use(new ConversationState<Dictionary<string, object>>(new MemoryStorage()));

            await new TestFlow(adapter, async (turnContext) =>
            {
                var state = ConversationState<Dictionary<string, object>>.Get(turnContext);
                var prompt = new ChoicePrompt(Culture.English);
                prompt.Style = ListStyle.None;

                var dialogCompletion = await prompt.Continue(turnContext, state);
                if (!dialogCompletion.IsActive && !dialogCompletion.IsCompleted)
                {
                    await prompt.Begin(turnContext, state,
                        new ChoicePromptOptions
                        {
                            PromptString = "favorite color?",
                            Choices = ChoiceFactory.ToChoices(colorChoices)
                        });
                }
                // TODO: this is a very awkward way to check for failure in teh current model
                else if (dialogCompletion.IsActive && !dialogCompletion.IsCompleted)
                {
                    if (dialogCompletion.Result == null)
                    {
                        await turnContext.SendActivity("NotRecognized");
                    }
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
            TestAdapter adapter = new TestAdapter()
                .Use(new ConversationState<Dictionary<string, object>>(new MemoryStorage()));

            PromptValidator<ChoiceResult> validator = (ITurnContext context, ChoiceResult result) =>
            {
                // TODO: the current model has no way for this status to bubble up
                result.Status = "validation failed";
                result.Value = null;
                return Task.CompletedTask;
            };

            await new TestFlow(adapter, async (turnContext) =>
            {
                var state = ConversationState<Dictionary<string, object>>.Get(turnContext);
                var prompt = new ChoicePrompt(Culture.English, validator);
                prompt.Style = ListStyle.None;

                var dialogCompletion = await prompt.Continue(turnContext, state);
                if (!dialogCompletion.IsActive && !dialogCompletion.IsCompleted)
                {
                    await prompt.Begin(turnContext, state,
                        new ChoicePromptOptions
                        {
                            PromptString = "favorite color?",
                            Choices = ChoiceFactory.ToChoices(colorChoices)
                        });
                }
                else if (dialogCompletion.IsActive && !dialogCompletion.IsCompleted)
                {
                    if (dialogCompletion.Result == null)
                    {
                        await turnContext.SendActivity("validation failed");
                    }
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
            TestAdapter adapter = new TestAdapter()
                .Use(new ConversationState<Dictionary<string, object>>(new MemoryStorage()));

            PromptValidator<ChoiceResult> validator = (ITurnContext context, ChoiceResult result) =>
            {
                Assert.IsTrue(false);
                return Task.CompletedTask;
            };

            await new TestFlow(adapter, async (turnContext) =>
            {
                var state = ConversationState<Dictionary<string, object>>.Get(turnContext);
                var prompt = new ChoicePrompt(Culture.English, validator);
                prompt.Style = ListStyle.None;

                var dialogCompletion = await prompt.Continue(turnContext, state);
                if (!dialogCompletion.IsActive && !dialogCompletion.IsCompleted)
                {
                    await prompt.Begin(turnContext, state,
                        new ChoicePromptOptions
                        {
                            PromptString = "favorite color?",
                            Choices = ChoiceFactory.ToChoices(colorChoices)
                        });
                }
                else if (dialogCompletion.IsActive && !dialogCompletion.IsCompleted)
                {
                    if (dialogCompletion.Result == null)
                    {
                        await turnContext.SendActivity("NotRecognized");
                    }
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
