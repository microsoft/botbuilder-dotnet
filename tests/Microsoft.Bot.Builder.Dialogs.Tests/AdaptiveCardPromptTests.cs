// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Dialogs.Tests
{
    [TestClass]
    public class AdaptiveCardPromptTests
    {
        private readonly string customPromptId = "custom";

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AdaptiveCardPromptWithoutSettingsShouldFail()
        {
            new AdaptiveCardPrompt("prompt", null);
        }
        
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AdaptiveCardPromptWithoutCardShouldFail()
        {
            new AdaptiveCardPrompt("prompt", new AdaptiveCardPromptSettings());
        }

        [TestMethod]
        [ExpectedException(typeof(NullReferenceException), "No Adaptive Card provided. Include in the constructor or PromptOptions.Prompt.Attachments")]
        public void AdaptiveCardPromptWithBlankCardShouldFail()
        {
            new AdaptiveCardPrompt("prompt", new AdaptiveCardPromptSettings() { Card = new Attachment() });
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void AdaptiveCardPromptWithoutValidCardShouldFail()
        {
            new AdaptiveCardPrompt("prompt", new AdaptiveCardPromptSettings()
            {
                Card = new Attachment()
                {
                    Content = GetCard().Content,
                    ContentType = "invalidcard"
                }
            });
        }

        [TestMethod]
        public async Task RecognizesInputWithCustomPromptIdAndCorrectInput()
        {
            var usedValidator = false;
            var prompt = new AdaptiveCardPrompt(
                "prompt", 
                new AdaptiveCardPromptSettings()
                {
                    PromptId = customPromptId,
                    Card = GetCard(),
                },
                (context, cancel) =>
                {
                    Assert.IsTrue(context.Recognized.Succeeded);
                    Assert.AreEqual(context.Recognized.Value.Error, AdaptiveCardPromptErrors.None);
                    usedValidator = true;
                    return Task.FromResult(context.Recognized.Succeeded);
                });
            var simulatedInput = GetSimulatedInput();

            var convoState = new ConversationState(new MemoryStorage());
            var dialogState = convoState.CreateProperty<DialogState>("dialogState");

            var adapter = new TestAdapter()
                .Use(new AutoSaveStateMiddleware(convoState));

            // Create new DialogSet.
            var dialogs = new DialogSet(dialogState);

            // Create and add custom activity prompt to DialogSet.
            dialogs.Add(prompt);

            // Create mock Activity for testing.
            await new TestFlow(adapter, async (turnContext, cancellationToken) =>
            {
                var dc = await dialogs.CreateContextAsync(turnContext, cancellationToken);

                var results = await dc.ContinueDialogAsync(cancellationToken);
                if (results.Status == DialogTurnStatus.Empty)
                {
                    await dc.PromptAsync("prompt", new PromptOptions());
                }
                else if (results.Status == DialogTurnStatus.Complete)
                {
                    var content = (results.Result as AdaptiveCardPromptResult).Data;
                    await turnContext.SendActivityAsync($"You said: {content.ToString()}");
                }
            })
            .Send("hello")
            .AssertReply(
                (activity) =>
                {
                    var card = JObject.FromObject((activity as Activity).Attachments[0].Content);
                    Assert.AreEqual(card["selectAction"]["data"]["promptId"], customPromptId);
                })
            .Send(simulatedInput)
            .StartTestAsync();
            Assert.IsTrue(usedValidator);
        }

        [TestMethod]
        public async Task PresentsPromptIdErrorWhenInputPromptIdDoesNotMatch()
        {
            var usedValidator = false;
            var prompt = new AdaptiveCardPrompt(
                "prompt",
                new AdaptiveCardPromptSettings()
                {
                    PromptId = "differentPromptId",
                    Card = GetCard(),
                },
                (context, cancel) =>
                {
                    Assert.IsFalse(context.Recognized.Succeeded);
                    Assert.AreEqual(context.Recognized.Value.Error, AdaptiveCardPromptErrors.UserInputDoesNotMatchCardId);
                    usedValidator = true;
                    return Task.FromResult(context.Recognized.Succeeded);
                });
            var simulatedInput = GetSimulatedInput();

            var convoState = new ConversationState(new MemoryStorage());
            var dialogState = convoState.CreateProperty<DialogState>("dialogState");

            var adapter = new TestAdapter()
                .Use(new AutoSaveStateMiddleware(convoState));

            // Create new DialogSet.
            var dialogs = new DialogSet(dialogState);

            // Create and add custom activity prompt to DialogSet.
            dialogs.Add(prompt);

            // Create mock Activity for testing.
            await new TestFlow(adapter, async (turnContext, cancellationToken) =>
            {
                var dc = await dialogs.CreateContextAsync(turnContext, cancellationToken);

                var results = await dc.ContinueDialogAsync(cancellationToken);
                if (results.Status == DialogTurnStatus.Empty)
                {
                    await dc.PromptAsync("prompt", new PromptOptions());
                }
                else if (results.Status == DialogTurnStatus.Complete)
                {
                    var content = (results.Result as AdaptiveCardPromptResult).Data;
                    await turnContext.SendActivityAsync($"You said: {content.ToString()}");
                }
            })
            .Send("hello")
            .AssertReply(
                (activity) =>
                {
                    var card = JObject.FromObject((activity as Activity).Attachments[0].Content);
                    Assert.AreEqual(card["selectAction"]["data"]["promptId"], customPromptId);
                })
            .Send(simulatedInput)
            .AssertReply(
                (activity) =>
                {
                    var card = JObject.FromObject((activity as Activity).Attachments[0].Content);
                    Assert.AreEqual(card["selectAction"]["data"]["promptId"], customPromptId);
                })
            .StartTestAsync();
            Assert.IsTrue(usedValidator);
        }

        [TestMethod]
        public async Task RecognizesIfAllRequiredIdsSupplied()
        {
            var usedValidator = false;
            var prompt = new AdaptiveCardPrompt(
                "prompt",
                new AdaptiveCardPromptSettings()
                {
                    Card = GetCard(),
                    RequiredInputIds = new string[]
                    {
                        "foodChoice",
                        "steakOther",
                        "steakTemp",
                    }
                },
                (context, cancel) =>
                {
                    Assert.IsTrue(context.Recognized.Succeeded);
                    Assert.AreEqual(context.Recognized.Value.Error, AdaptiveCardPromptErrors.None);
                    usedValidator = true;
                    return Task.FromResult(context.Recognized.Succeeded);
                });
            var simulatedInput = GetSimulatedInput();

            var convoState = new ConversationState(new MemoryStorage());
            var dialogState = convoState.CreateProperty<DialogState>("dialogState");

            var adapter = new TestAdapter()
                .Use(new AutoSaveStateMiddleware(convoState));

            // Create new DialogSet.
            var dialogs = new DialogSet(dialogState);

            // Create and add custom activity prompt to DialogSet.
            dialogs.Add(prompt);

            // Create mock Activity for testing.
            await new TestFlow(adapter, async (turnContext, cancellationToken) =>
            {
                var dc = await dialogs.CreateContextAsync(turnContext, cancellationToken);

                var results = await dc.ContinueDialogAsync(cancellationToken);
                if (results.Status == DialogTurnStatus.Empty)
                {
                    await dc.PromptAsync("prompt", new PromptOptions());
                }
                else if (results.Status == DialogTurnStatus.Complete)
                {
                    var content = (results.Result as AdaptiveCardPromptResult).Data;
                    await turnContext.SendActivityAsync($"You said: {content.ToString()}");
                }
            })
            .Send("hello")
            .AssertReply((activity) => { AssertActivityHasCard(activity); })
            .Send(simulatedInput)
            .StartTestAsync();
            Assert.IsTrue(usedValidator);
        }

        [TestMethod]
        public async Task PresentsMissingIdsErrorWhenInputIsMissingIds()
        {
            var usedValidator = false;
            var prompt = new AdaptiveCardPrompt(
                "prompt",
                new AdaptiveCardPromptSettings()
                {
                    Card = GetCard(),
                    RequiredInputIds = new string[]
                    {
                        "test1",
                        "test2",
                        "test3",
                    }
                },
                async (context, cancel) =>
                {
                    Assert.IsFalse(context.Recognized.Succeeded);
                    Assert.AreEqual(context.Recognized.Value.Error, AdaptiveCardPromptErrors.MissingRequiredIds);
                    await context.Context.SendActivityAsync($"test inputs missing: {string.Join(", ", context.Recognized.Value.MissingIds)}");
                    usedValidator = true;
                    return context.Recognized.Succeeded;
                });
            var simulatedInput = GetSimulatedInput();

            var convoState = new ConversationState(new MemoryStorage());
            var dialogState = convoState.CreateProperty<DialogState>("dialogState");

            var adapter = new TestAdapter()
                .Use(new AutoSaveStateMiddleware(convoState));

            // Create new DialogSet.
            var dialogs = new DialogSet(dialogState);

            // Create and add custom activity prompt to DialogSet.
            dialogs.Add(prompt);

            // Create mock Activity for testing.
            await new TestFlow(adapter, async (turnContext, cancellationToken) =>
            {
                var dc = await dialogs.CreateContextAsync(turnContext, cancellationToken);

                var results = await dc.ContinueDialogAsync(cancellationToken);
                if (results.Status == DialogTurnStatus.Empty)
                {
                    await dc.PromptAsync("prompt", new PromptOptions());
                }
                else if (results.Status == DialogTurnStatus.Complete)
                {
                    var content = results.Result.GetType().GetProperty("Data");
                    await turnContext.SendActivityAsync($"You said: {content.ToString()}");
                }
            })
            .Send("hello")
            .AssertReply((activity) => { AssertActivityHasCard(activity); })
            .Send(simulatedInput)
            .AssertReply("test inputs missing: test1, test2, test3")
            .StartTestAsync();
            Assert.IsTrue(usedValidator);
        }

        [TestMethod]
        public async Task RecognizesValidCardInput()
        {
            var usedValidator = false;
            var prompt = new AdaptiveCardPrompt(
                "prompt",
                new AdaptiveCardPromptSettings()
                {
                    Card = GetCard(),
                },
                (context, cancel) =>
                {
                    Assert.IsTrue(context.Recognized.Succeeded);
                    usedValidator = true;
                    return Task.FromResult(context.Recognized.Succeeded);
                });
            var simulatedInput = GetSimulatedInput();

            var convoState = new ConversationState(new MemoryStorage());
            var dialogState = convoState.CreateProperty<DialogState>("dialogState");

            var adapter = new TestAdapter()
                .Use(new AutoSaveStateMiddleware(convoState));

            // Create new DialogSet.
            var dialogs = new DialogSet(dialogState);

            // Create and add custom activity prompt to DialogSet.
            dialogs.Add(prompt);

            // Create mock Activity for testing.
            await new TestFlow(adapter, async (turnContext, cancellationToken) =>
            {
                var dc = await dialogs.CreateContextAsync(turnContext, cancellationToken);

                var results = await dc.ContinueDialogAsync(cancellationToken);
                if (results.Status == DialogTurnStatus.Empty)
                {
                    await dc.PromptAsync("prompt", new PromptOptions());
                }
                else if (results.Status == DialogTurnStatus.Complete)
                {
                    var content = (results.Result as AdaptiveCardPromptResult).Data;
                    await turnContext.SendActivityAsync($"You said: {content.ToString()}");
                }
            })
            .Send("hello")
            .AssertReply((activity) => { AssertActivityHasCard(activity); })
            .Send(simulatedInput)
            .AssertReply($"You said: {simulatedInput.Value.ToString()}")
            .StartTestAsync();
            Assert.IsTrue(usedValidator);
        }

        [TestMethod]
        public async Task PresentsTextInputErrorWhenUserInputsViaTextNotCard()
        {
            var usedValidator = false;
            var prompt = new AdaptiveCardPrompt(
                "prompt",
                new AdaptiveCardPromptSettings()
                {
                    Card = GetCard(),
                },
                (context, cancel) =>
                {
                    Assert.IsFalse(context.Recognized.Succeeded);
                    Assert.AreEqual(context.Recognized.Value.Error, AdaptiveCardPromptErrors.UserUsedTextInput);
                    usedValidator = true;
                    return Task.FromResult(context.Recognized.Succeeded);
                });
            var simulatedInput = GetSimulatedInput();

            var convoState = new ConversationState(new MemoryStorage());
            var dialogState = convoState.CreateProperty<DialogState>("dialogState");

            var adapter = new TestAdapter()
                .Use(new AutoSaveStateMiddleware(convoState));

            // Create new DialogSet.
            var dialogs = new DialogSet(dialogState);

            // Create and add custom activity prompt to DialogSet.
            dialogs.Add(prompt);

            // Create mock Activity for testing.
            await new TestFlow(adapter, async (turnContext, cancellationToken) =>
            {
                var dc = await dialogs.CreateContextAsync(turnContext, cancellationToken);

                var results = await dc.ContinueDialogAsync(cancellationToken);
                if (results.Status == DialogTurnStatus.Empty)
                {
                    await dc.PromptAsync("prompt", new PromptOptions());
                }
                else if (results.Status == DialogTurnStatus.Complete)
                {
                    var content = (results.Result as AdaptiveCardPromptResult).Data;
                    await turnContext.SendActivityAsync($"You said: {content.ToString()}");
                }
            })
            .Send("hello")
            .AssertReply((activity) => { AssertActivityHasCard(activity); })
            .Send("This is not valid card input")
            .AssertReply((activity) => { AssertActivityHasCard(activity); })
            .StartTestAsync();
            Assert.IsTrue(usedValidator);
        }

        [TestMethod]
        public async Task CallsUsingBeginDialog()
        {
            var prompt = new AdaptiveCardPrompt("prompt", new AdaptiveCardPromptSettings() { Card = GetCard() });
            var simulatedInput = GetSimulatedInput();

            var convoState = new ConversationState(new MemoryStorage());
            var dialogState = convoState.CreateProperty<DialogState>("dialogState");

            var adapter = new TestAdapter()
                .Use(new AutoSaveStateMiddleware(convoState));

            // Create new DialogSet.
            var dialogs = new DialogSet(dialogState);

            // Create and add custom activity prompt to DialogSet.
            dialogs.Add(prompt);

            // Create mock Activity for testing.
            await new TestFlow(adapter, async (turnContext, cancellationToken) =>
            {
                var dc = await dialogs.CreateContextAsync(turnContext, cancellationToken);

                var results = await dc.ContinueDialogAsync(cancellationToken);
                if (results.Status == DialogTurnStatus.Empty)
                {
                    await dc.BeginDialogAsync("prompt", new PromptOptions());
                }
                else if (results.Status == DialogTurnStatus.Complete)
                {
                    var content = (results.Result as AdaptiveCardPromptResult).Data;
                    await turnContext.SendActivityAsync($"You said: {content.ToString()}");
                }
            })
            .Send("hello")
            .AssertReply((activity) => { AssertActivityHasCard(activity); })
            .Send(simulatedInput)
            .AssertReply($"You said: {simulatedInput.Value.ToString()}")
            .StartTestAsync();
        }

        [TestMethod]
        public async Task CallsUsingPrompt()
        {
            var prompt = new AdaptiveCardPrompt("prompt", new AdaptiveCardPromptSettings() { Card = GetCard() });
            var simulatedInput = GetSimulatedInput();

            var convoState = new ConversationState(new MemoryStorage());
            var dialogState = convoState.CreateProperty<DialogState>("dialogState");

            var adapter = new TestAdapter()
                .Use(new AutoSaveStateMiddleware(convoState));

            // Create new DialogSet.
            var dialogs = new DialogSet(dialogState);

            // Create and add custom activity prompt to DialogSet.
            dialogs.Add(prompt);

            // Create mock Activity for testing.
            await new TestFlow(adapter, async (turnContext, cancellationToken) =>
            {
                var dc = await dialogs.CreateContextAsync(turnContext, cancellationToken);

                var results = await dc.ContinueDialogAsync(cancellationToken);
                if (results.Status == DialogTurnStatus.Empty)
                {
                    await dc.PromptAsync("prompt", new PromptOptions());
                }
                else if (results.Status == DialogTurnStatus.Complete)
                {
                    var content = (results.Result as AdaptiveCardPromptResult).Data;
                    await turnContext.SendActivityAsync($"You said: {content.ToString()}");
                }
            })
            .Send("hello")
            .AssertReply((activity) => { AssertActivityHasCard(activity); })
            .Send(simulatedInput)
            .AssertReply($"You said: {simulatedInput.Value.ToString()}")
            .StartTestAsync();
        }

        [TestMethod]
        public async Task UsesRetryPromptOnRetries()
        {
            var prompt = new AdaptiveCardPrompt("prompt", new AdaptiveCardPromptSettings() { Card = GetCard() });
            var simulatedInput = GetSimulatedInput();

            var convoState = new ConversationState(new MemoryStorage());
            var dialogState = convoState.CreateProperty<DialogState>("dialogState");

            var adapter = new TestAdapter()
                .Use(new AutoSaveStateMiddleware(convoState));

            // Create new DialogSet.
            var dialogs = new DialogSet(dialogState);

            // Create and add custom activity prompt to DialogSet.
            dialogs.Add(prompt);

            // Create mock Activity for testing.
            await new TestFlow(adapter, async (turnContext, cancellationToken) =>
            {
                var dc = await dialogs.CreateContextAsync(turnContext, cancellationToken);

                var results = await dc.ContinueDialogAsync(cancellationToken);
                if (results.Status == DialogTurnStatus.Empty)
                {
                    await dc.BeginDialogAsync("prompt", new PromptOptions()
                    {
                        RetryPrompt = MessageFactory.Text("RETRY")
                    });
                }
                else if (results.Status == DialogTurnStatus.Complete)
                {
                    var content = (results.Result as AdaptiveCardPromptResult).Data;
                    await turnContext.SendActivityAsync($"You said: {content.ToString()}");
                }
            })
            .Send("hello")
            .AssertReply((activity) => { AssertActivityHasCard(activity); })
            .Send("This is not a valid response")
            .AssertReply("RETRY")
            .StartTestAsync();
        }

        [TestMethod]
        public async Task DoesNotOverwriteDevProvidedAttachments()
        {
            var prompt = new AdaptiveCardPrompt("prompt", new AdaptiveCardPromptSettings() { Card = GetCard() });
            var simulatedInput = GetSimulatedInput();

            var convoState = new ConversationState(new MemoryStorage());
            var dialogState = convoState.CreateProperty<DialogState>("dialogState");

            var adapter = new TestAdapter()
                .Use(new AutoSaveStateMiddleware(convoState));

            // Create new DialogSet.
            var dialogs = new DialogSet(dialogState);

            // Create and add custom activity prompt to DialogSet.
            dialogs.Add(prompt);

            // Create mock Activity for testing.
            await new TestFlow(adapter, async (turnContext, cancellationToken) =>
            {
                var dc = await dialogs.CreateContextAsync(turnContext, cancellationToken);

                var results = await dc.ContinueDialogAsync(cancellationToken);
                if (results.Status == DialogTurnStatus.Empty)
                {
                    var promptWithAttachments = MessageFactory.Text(string.Empty);
                    promptWithAttachments.Attachments = new List<Attachment>
                    {
                        new Attachment() { Content = "a" },
                        new Attachment() { Content = "b" },
                        new Attachment() { Content = "c" },
                    };
                    await dc.BeginDialogAsync("prompt", new PromptOptions()
                    {
                        Prompt = promptWithAttachments
                    });
                }
                else if (results.Status == DialogTurnStatus.Complete)
                {
                    var content = (results.Result as AdaptiveCardPromptResult).Data;
                    await turnContext.SendActivityAsync($"You said: {content.ToString()}");
                }
            })
            .Send("hello")
            .AssertReply((activity) => 
            {
                Assert.AreEqual((activity as Activity).Attachments[0].Content, "a");
                Assert.AreEqual((activity as Activity).Attachments[1].Content, "b");
                Assert.AreEqual((activity as Activity).Attachments[2].Content, "c");
                Assert.AreEqual((activity as Activity).Attachments[3].ContentType, "application/vnd.microsoft.card.adaptive");
            })
            .StartTestAsync();
        }

        [TestMethod]
        public async Task TracksTheNumberOfAttempts()
        {
            var attempts = 0;
            var prompt = new AdaptiveCardPrompt(
                "prompt",
                new AdaptiveCardPromptSettings()
                {
                    Card = GetCard(),
                },
                (context, cancel) =>
                {
                    attempts = int.Parse(context.State["AttemptCount"].ToString());
                    return Task.FromResult(false);
                });
            var simulatedInput = GetSimulatedInput();

            var convoState = new ConversationState(new MemoryStorage());
            var dialogState = convoState.CreateProperty<DialogState>("dialogState");

            var adapter = new TestAdapter()
                .Use(new AutoSaveStateMiddleware(convoState));

            // Create new DialogSet.
            var dialogs = new DialogSet(dialogState);

            // Create and add custom activity prompt to DialogSet.
            dialogs.Add(prompt);

            // Create mock Activity for testing.
            await new TestFlow(adapter, async (turnContext, cancellationToken) =>
            {
                var dc = await dialogs.CreateContextAsync(turnContext, cancellationToken);

                var results = await dc.ContinueDialogAsync(cancellationToken);
                if (results.Status == DialogTurnStatus.Empty)
                {
                    await dc.PromptAsync("prompt", new PromptOptions());
                }
                else if (results.Status == DialogTurnStatus.Complete)
                {
                    var content = (results.Result as AdaptiveCardPromptResult).Data;
                    await turnContext.SendActivityAsync($"You said: {content.ToString()}");
                }
            })
            .Send("hello")
            .AssertReply((activity) => 
            { 
                AssertActivityHasCard(activity);
            })
            .Send(simulatedInput)
            .AssertReply((activity) =>
            {
                AssertActivityHasCard(activity);
                Assert.AreEqual(attempts, 1);
            })
            .Send(simulatedInput)
            .AssertReply((activity) =>
            {
                AssertActivityHasCard(activity);
                Assert.AreEqual(attempts, 2);
            })
            .Send(simulatedInput)
            .AssertReply((activity) =>
            {
                AssertActivityHasCard(activity);
                Assert.AreEqual(attempts, 3);
            })
            .StartTestAsync();
        }

        [TestMethod]
        public async Task AcceptsCustomValidatorAndCallsItIfRecognizedSucceeded()
        {
            var usedValidator = false;
            var prompt = new AdaptiveCardPrompt(
                "prompt",
                new AdaptiveCardPromptSettings()
                {
                    Card = GetCard(),
                },
                (context, cancel) =>
                {
                    Assert.IsTrue(context.Recognized.Succeeded);
                    usedValidator = true;
                    return Task.FromResult(true);
                });
            var simulatedInput = GetSimulatedInput();

            var convoState = new ConversationState(new MemoryStorage());
            var dialogState = convoState.CreateProperty<DialogState>("dialogState");

            var adapter = new TestAdapter()
                .Use(new AutoSaveStateMiddleware(convoState));

            // Create new DialogSet.
            var dialogs = new DialogSet(dialogState);

            // Create and add custom activity prompt to DialogSet.
            dialogs.Add(prompt);

            // Create mock Activity for testing.
            await new TestFlow(adapter, async (turnContext, cancellationToken) =>
            {
                var dc = await dialogs.CreateContextAsync(turnContext, cancellationToken);

                var results = await dc.ContinueDialogAsync(cancellationToken);
                if (results.Status == DialogTurnStatus.Empty)
                {
                    await dc.PromptAsync("prompt", new PromptOptions());
                }
                else if (results.Status == DialogTurnStatus.Complete)
                {
                    var content = (results.Result as AdaptiveCardPromptResult).Data;
                    await turnContext.SendActivityAsync($"You said: {content.ToString()}");
                }
            })
            .Send("hello")
            .AssertReply((activity) => { AssertActivityHasCard(activity); })
            .Send(simulatedInput)
            .StartTestAsync();
            Assert.IsTrue(usedValidator);
        }

        [TestMethod]
        public async Task AcceptsCustomValidatorAndCallsItIfNotRecognizedSucceeded()
        {
            var usedValidator = false;
            var prompt = new AdaptiveCardPrompt(
                "prompt",
                new AdaptiveCardPromptSettings()
                {
                    Card = GetCard(),
                },
                (context, cancel) =>
                {
                    Assert.IsFalse(context.Recognized.Succeeded);
                    usedValidator = true;
                    return Task.FromResult(false);
                });
            var simulatedInput = GetSimulatedInput();

            var convoState = new ConversationState(new MemoryStorage());
            var dialogState = convoState.CreateProperty<DialogState>("dialogState");

            var adapter = new TestAdapter()
                .Use(new AutoSaveStateMiddleware(convoState));

            // Create new DialogSet.
            var dialogs = new DialogSet(dialogState);

            // Create and add custom activity prompt to DialogSet.
            dialogs.Add(prompt);

            // Create mock Activity for testing.
            await new TestFlow(adapter, async (turnContext, cancellationToken) =>
            {
                var dc = await dialogs.CreateContextAsync(turnContext, cancellationToken);

                var results = await dc.ContinueDialogAsync(cancellationToken);
                if (results.Status == DialogTurnStatus.Empty)
                {
                    await dc.PromptAsync("prompt", new PromptOptions());
                }
                else if (results.Status == DialogTurnStatus.Complete)
                {
                    var content = (results.Result as AdaptiveCardPromptResult).Data;
                    await turnContext.SendActivityAsync($"You said: {content.ToString()}");
                }
            })
            .Send("hello")
            .AssertReply((activity) => { AssertActivityHasCard(activity); })
            .Send("this is not valid input")
            .StartTestAsync();
            Assert.IsTrue(usedValidator);
        }

        [TestMethod]
        public async Task DoesNotRepromptIfNotRecognizedSucceededButValidatorTrue()
        {
            var usedValidator = false;
            var prompt = new AdaptiveCardPrompt(
                "prompt",
                new AdaptiveCardPromptSettings()
                {
                    Card = GetCard(),
                },
                (context, cancel) =>
                {
                    Assert.IsFalse(context.Recognized.Succeeded);
                    usedValidator = true;
                    return Task.FromResult(true);
                });
            var simulatedInput = GetSimulatedInput();

            var convoState = new ConversationState(new MemoryStorage());
            var dialogState = convoState.CreateProperty<DialogState>("dialogState");

            var adapter = new TestAdapter()
                .Use(new AutoSaveStateMiddleware(convoState));

            // Create new DialogSet.
            var dialogs = new DialogSet(dialogState);

            // Create and add custom activity prompt to DialogSet.
            dialogs.Add(prompt);

            // Create mock Activity for testing.
            await new TestFlow(adapter, async (turnContext, cancellationToken) =>
            {
                var dc = await dialogs.CreateContextAsync(turnContext, cancellationToken);

                var results = await dc.ContinueDialogAsync(cancellationToken);
                if (results.Status == DialogTurnStatus.Empty)
                {
                    await dc.PromptAsync("prompt", new PromptOptions());
                }
                else if (results.Status == DialogTurnStatus.Complete)
                {
                    var content = (results.Result as AdaptiveCardPromptResult).Data;
                    await turnContext.SendActivityAsync("Validator passed");
                }
            })
            .Send("hello")
            .AssertReply((activity) => { AssertActivityHasCard(activity); })
            .Send("this is not valid input")
            .AssertReply("Validator passed")
            .StartTestAsync();
            Assert.IsTrue(usedValidator);
        }

        private Attachment GetCard()
        {
            var cardPath = Path.Combine("../../../", "adaptiveCard.json");
            var cardJson = File.ReadAllText(cardPath);
            var cardAttachment = new Attachment()
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = JsonConvert.DeserializeObject(cardJson),
            };
            return cardAttachment;
        }

        private List<Attachment> GetAttachmentsWithCard() => new List<Attachment>() { GetCard() };

        private Activity GetSimulatedInput() => new Activity()
        {
            Type = ActivityTypes.Message,
            Value = JObject.FromObject(new
            {
                foodChoice = "Steak",
                steakOther = "some details",
                steakTemp = "rare",
                promptId = customPromptId,
            }),
        };

        private void AssertActivityHasCard(IActivity activity) => Assert.AreEqual("application/vnd.microsoft.card.adaptive", (activity as Activity).Attachments[0].ContentType);
    }
}
