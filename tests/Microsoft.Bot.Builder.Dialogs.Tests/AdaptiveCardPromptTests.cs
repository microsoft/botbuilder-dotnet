// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
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
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AdaptiveCardPromptWithEmptyIdShouldFail()
        {
            var emptyId = string.Empty;
            var textPrompt = new AdaptiveCardPrompt(emptyId);
        }

        [TestMethod]
        public async Task ShouldCallAdaptiveCardPromptUsingDcPrompt()
        {
            var prompt = new AdaptiveCardPrompt("prompt");
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
                    var options = new PromptOptions { Prompt = new Activity { Attachments = GetAttachmentsWithCard() } };
                    await dc.PromptAsync("prompt", options, cancellationToken);
                }
                else if (results.Status == DialogTurnStatus.Complete)
                {
                    var content = results.Result;
                    await turnContext.SendActivityAsync($"You said: {content.ToString()}");
                }
            })
            .Send("hello")
            .AssertReply(
                (activity) =>
                {
                    AssertActivityHasCard(activity);
                    UpdateSimulatedInputWithPromptId(simulatedInput, prompt);
                })
            .Send(simulatedInput)

            // MUST use lambda because test is async and simulatedInput changes after prompt displayed. Ref won't work because async.
            .AssertReply((activity) =>
            {
                Assert.AreEqual($"You said: {simulatedInput.Value.ToString()}", (activity as Activity).Text);
            })
            .StartTestAsync();
        }

        [TestMethod]
        public async Task ShouldCallAdaptiveCardPromptUsingDcBeginDialog()
        {
            var prompt = new AdaptiveCardPrompt("prompt");
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
                    var options = new PromptOptions { Prompt = new Activity { Attachments = GetAttachmentsWithCard() } };
                    await dc.BeginDialogAsync("prompt", options, cancellationToken);
                }
                else if (results.Status == DialogTurnStatus.Complete)
                {
                    var content = results.Result;
                    await turnContext.SendActivityAsync($"You said: {content.ToString()}");
                }
            })
            .Send("hello")
            .AssertReply(
                (activity) =>
                {
                    AssertActivityHasCard(activity);
                    UpdateSimulatedInputWithPromptId(simulatedInput, prompt);
                })
            .Send(simulatedInput)

            // MUST use lambda because test is async and simulatedInput changes after prompt displayed. Ref won't work because async.
            .AssertReply((activity) =>
            {
                Assert.AreEqual($"You said: {simulatedInput.Value.ToString()}", (activity as Activity).Text);
            })
            .StartTestAsync();
        }

        [TestMethod]
        public async Task ShouldCreateANewPromptIdForEachOnPromptCall()
        {
            var prompt = new AdaptiveCardPrompt("prompt");
            var simulatedInput = GetSimulatedInput();
            var usedIds = new HashSet<string>();

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
                    var options = new PromptOptions { Prompt = new Activity { Attachments = GetAttachmentsWithCard() } };
                    await dc.PromptAsync("prompt", options, cancellationToken);
                }
                else if (results.Status == DialogTurnStatus.Complete)
                {
                    var content = results.Result;
                    await turnContext.SendActivityAsync(GetPromptIdFromObject(content));
                }
            })
            .Send("hello")
            .AssertReply(
                (activity) =>
                {
                    AssertActivityHasCard(activity);
                    UpdateSimulatedInputWithPromptId(simulatedInput, prompt);
                })
            .Send(simulatedInput)

            // MUST use lambda because test is async and simulatedInput changes after prompt displayed. Ref won't work because async.
            .AssertReply((activity) =>
            {
                var promptId = (activity as Activity).Text;
                Assert.IsFalse(usedIds.Contains(promptId));
                usedIds.Add(promptId);
            })
            .Send("hello")
            .AssertReply(
                (activity) =>
                {
                    AssertActivityHasCard(activity);
                    UpdateSimulatedInputWithPromptId(simulatedInput, prompt);
                })
            .Send(simulatedInput)

            // MUST use lambda because test is async and simulatedInput changes after prompt displayed. Ref won't work because async.
            .AssertReply((activity) =>
            {
                var promptId = (activity as Activity).Text;
                Assert.IsFalse(usedIds.Contains(promptId));
            })
            .StartTestAsync();
        }

        [TestMethod]
        public async Task ShouldUseRetryPromptIfGivenAndAttemptsBeforeCardRedisplayedAllows()
        {
            var prompt = new AdaptiveCardPrompt("prompt", null, new AdaptiveCardPromptSettings() { AttemptsBeforeCardRedisplayed = 1 });

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
                    var options = new PromptOptions
                    {
                        Prompt = new Activity { Attachments = GetAttachmentsWithCard() },
                        RetryPrompt = new Activity { Text = "RETRY", Attachments = GetAttachmentsWithCard() },
                    };
                    await dc.PromptAsync("prompt", options, cancellationToken);
                }
            })
            .Send("hello")
            .AssertReply(
                (activity) =>
                {
                    AssertActivityHasCard(activity);
                })
            .Send(GetInvalidSimulatedInput())
            .AssertReply("Please fill out the Adaptive Card")

            // Must be lambda to avoid type check. RetryPrompt type is null
            .AssertReply((activity) => Assert.AreEqual("RETRY", (activity as Activity).Text))
            .StartTestAsync();
        }

        [TestMethod]
        public async Task ShouldAllowForCustomPromptIdThatDoesntChangeOnReprompt()
        {
            var customId = "custom";
            var prompt = new AdaptiveCardPrompt("prompt", null, new AdaptiveCardPromptSettings() { AttemptsBeforeCardRedisplayed = 1, PromptId = customId });
            var simulatedInput = GetSimulatedInput();
            UpdateSimulatedInputWithPromptId(simulatedInput, null, customId);

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
                    var options = new PromptOptions
                    {
                        Prompt = new Activity { Attachments = GetAttachmentsWithCard() },
                    };
                    await dc.PromptAsync("prompt", options, cancellationToken);
                }
                else if (results.Status == DialogTurnStatus.Complete)
                {
                    var content = results.Result;
                    await turnContext.SendActivityAsync(GetPromptIdFromObject(content));
                }
            })
            .Send("hello")
            .AssertReply(
                (activity) =>
                {
                    AssertActivityHasCard(activity);
                })
            .Send(simulatedInput)
            .AssertReply(customId)
            .StartTestAsync();
        }

        [TestMethod]
        public async Task PromptCanBeTextOnlyActivityIfCardPassedInConstructor()
        {
            var prompt = new AdaptiveCardPrompt("prompt", null, new AdaptiveCardPromptSettings() { Card = GetCard() });

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
                    var options = new PromptOptions
                    {
                        Prompt = MessageFactory.Text("STRING"),
                    };
                    await dc.PromptAsync("prompt", options, cancellationToken);
                }
                else if (results.Status == DialogTurnStatus.Complete)
                {
                    var content = results.Result;
                    await turnContext.SendActivityAsync(GetPromptIdFromObject(content));
                }
            })
            .Send("hello")
            .AssertReply("STRING")
            .StartTestAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(NullReferenceException))]
        public async Task ShouldThrowIfNoAttachmentPassedInConstructorOrSet()
        {
            var prompt = new AdaptiveCardPrompt("prompt");

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
                    var options = new PromptOptions
                    {
                        Prompt = MessageFactory.Text("STRING"),
                    };
                    await dc.PromptAsync("prompt", options, cancellationToken);
                }
            })
            .Send("hello")
            .StartTestAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task ShouldThrowIfCardIsNotAValidAdaptiveCard()
        {
            var card = GetCard();
            card.ContentType = "InvalidCard";
            var prompt = new AdaptiveCardPrompt("prompt", null, new AdaptiveCardPromptSettings() { Card = card });

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
                    var options = new PromptOptions
                    {
                        Prompt = MessageFactory.Text("STRING"),
                    };
                    await dc.PromptAsync("prompt", options, cancellationToken);
                }
            })
            .Send("hello")
            .StartTestAsync();
        }

        [TestMethod]
        public async Task ShouldAcceptACustomValidatorThatHandlesValidContext()
        {
            var usedValidator = false;

            Task<bool> Validator(PromptValidatorContext<object> promptContext, CancellationToken cancellationToken)
            {
                usedValidator = true;
                return Task.FromResult(true);
            }

            var prompt = new AdaptiveCardPrompt("prompt", Validator);
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
                    var options = new PromptOptions { Prompt = new Activity { Attachments = GetAttachmentsWithCard() } };
                    await dc.PromptAsync("prompt", options, cancellationToken);
                }
                else if (results.Status == DialogTurnStatus.Complete)
                {
                    var content = results.Result;
                    await turnContext.SendActivityAsync($"You said: {content.ToString()}");
                }
            })
            .Send("hello")
            .AssertReply(
                (activity) =>
                {
                    AssertActivityHasCard(activity);
                    UpdateSimulatedInputWithPromptId(simulatedInput, prompt);
                })
            .Send(simulatedInput)

            // MUST use lambda because test is async and simulatedInput changes after prompt displayed. Ref won't work because async.
            .AssertReply((activity) =>
            {
                Assert.AreEqual($"You said: {simulatedInput.Value.ToString()}", (activity as Activity).Text);
            })
            .StartTestAsync();
            Assert.IsTrue(usedValidator);
        }

        [TestMethod]
        public async Task ShouldAcceptACustomValidatorThatHandlesInalidContext()
        {
            var usedValidator = false;

            async Task<bool> Validator(PromptValidatorContext<object> promptContext, CancellationToken cancellationToken)
            {
                usedValidator = true;
                await promptContext.Context.SendActivityAsync("FAILED");
                return false;
            }

            var prompt = new AdaptiveCardPrompt("prompt", Validator);
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
                    var options = new PromptOptions { Prompt = new Activity { Attachments = GetAttachmentsWithCard() } };
                    await dc.PromptAsync("prompt", options, cancellationToken);
                }
                else if (results.Status == DialogTurnStatus.Complete)
                {
                    var content = results.Result;
                    await turnContext.SendActivityAsync($"You said: {content.ToString()}");
                }
            })
            .Send("hello")
            .AssertReply(
                (activity) =>
                {
                    AssertActivityHasCard(activity);
                    UpdateSimulatedInputWithPromptId(simulatedInput, prompt);
                })
            .Send(simulatedInput)
            .AssertReply("FAILED")
            .StartTestAsync();
            Assert.IsTrue(usedValidator);
        }

        [TestMethod]
        public async Task ShouldTrackTheNumberOfAttempts()
        {
            var attempts = 0;

            Task<bool> Validator(PromptValidatorContext<object> promptContext, CancellationToken cancellationToken)
            {
                attempts = promptContext.AttemptCount;
                return Task.FromResult(false);
            }

            var prompt = new AdaptiveCardPrompt("prompt", Validator, new AdaptiveCardPromptSettings() { AttemptsBeforeCardRedisplayed = 99 });
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
                    var options = new PromptOptions { Prompt = new Activity { Attachments = GetAttachmentsWithCard() } };
                    await dc.PromptAsync("prompt", options, cancellationToken);
                }
                else if (results.Status == DialogTurnStatus.Waiting)
                {
                    await turnContext.SendActivityAsync("Invalid Response");
                }
            })
            .Send("hello")
            .AssertReply(
                (activity) =>
                {
                    AssertActivityHasCard(activity);
                    UpdateSimulatedInputWithPromptId(simulatedInput, prompt);
                })
            .Send(simulatedInput)
            .AssertReply("Invalid Response")
            .Send(simulatedInput)
            .AssertReply("Invalid Response")
            .Send(simulatedInput)
            .AssertReply("Invalid Response")
            .StartTestAsync();
            Assert.AreEqual(3, attempts);
        }

        [TestMethod]
        public async Task ShouldRecognizeCardInput()
        {
            var usedValidator = false;

            Task<bool> Validator(PromptValidatorContext<object> promptContext, CancellationToken cancellationToken)
            {
                usedValidator = true;
                Assert.IsTrue(promptContext.Recognized.Succeeded);
                return Task.FromResult(true);
            }

            var prompt = new AdaptiveCardPrompt("prompt", Validator);
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
                    var options = new PromptOptions { Prompt = new Activity { Attachments = GetAttachmentsWithCard() } };
                    await dc.PromptAsync("prompt", options, cancellationToken);
                }
                else if (results.Status == DialogTurnStatus.Complete)
                {
                    var content = results.Result;
                    await turnContext.SendActivityAsync($"You said: {content.ToString()}");
                }
            })
            .Send("hello")
            .AssertReply(
                (activity) =>
                {
                    AssertActivityHasCard(activity);
                    UpdateSimulatedInputWithPromptId(simulatedInput, prompt);
                    Assert.IsTrue(simulatedInput.Value != null && string.IsNullOrEmpty(simulatedInput.Text));
                })
            .Send(simulatedInput)
            .AssertReply((activity) =>
            {
                Assert.AreEqual($"You said: {simulatedInput.Value.ToString()}", (activity as Activity).Text);
            })
            .StartTestAsync();
            Assert.IsTrue(usedValidator);
        }

        [TestMethod]
        public async Task ShouldNotRecognizeTextInputAndShouldDisplayCustomInputFailMessage()
        {
            // Note: Validator isn't used if !recognized.succeeded
            var failMessage = "Test input fail message";
            var prompt = new AdaptiveCardPrompt("prompt", null, new AdaptiveCardPromptSettings() { InputFailMessage = failMessage });
            var invalidSimulatedInput = GetInvalidSimulatedInput();

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
                    var options = new PromptOptions { Prompt = new Activity { Attachments = GetAttachmentsWithCard() } };
                    await dc.PromptAsync("prompt", options, cancellationToken);
                }
            })
            .Send("hello")
            .AssertReply(
                (activity) =>
                {
                    AssertActivityHasCard(activity);
                    UpdateSimulatedInputWithPromptId(invalidSimulatedInput, prompt);
                })
            .Send(invalidSimulatedInput)
            .AssertReply(failMessage)
            .StartTestAsync();
        }

        [TestMethod]
        public async Task ShouldNotSuccessfullyRecognizeIfInputComesFromCardWithWrongId()
        {
            // Note: Validator isn't used if !recognized.succeeded
            var prompt = new AdaptiveCardPrompt("prompt");
            var simulatedInput = GetSimulatedInput();
            UpdateSimulatedInputWithPromptId(simulatedInput, null, "wrongId");

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
                    var options = new PromptOptions { Prompt = new Activity { Attachments = GetAttachmentsWithCard() } };
                    await dc.PromptAsync("prompt", options, cancellationToken);
                }
                else if (results.Status == DialogTurnStatus.Waiting)
                {
                    await turnContext.SendActivityAsync("Invalid Response");
                }
            })
            .Send("hello")
            .AssertReply(
                (activity) =>
                {
                    AssertActivityHasCard(activity);
                })
            .Send(simulatedInput)
            .AssertReply("Invalid Response")
            .StartTestAsync();
        }

        [TestMethod]
        public async Task ShouldNotSuccessfullyRecognizeAndShouldUseCustomMissingIdsMessage()
        {
            // Note: Validator isn't used if !recognized.succeeded
            var missingIdsMessage = "Test Missing Ids";
            var prompt = new AdaptiveCardPrompt("prompt", null, new AdaptiveCardPromptSettings()
            {
                MissingRequiredInputsMessage = missingIdsMessage,
                RequiredInputIds = new List<string>() { "test1", "test2", "test3" },
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
                    var options = new PromptOptions { Prompt = new Activity { Attachments = GetAttachmentsWithCard() } };
                    await dc.PromptAsync("prompt", options, cancellationToken);
                }
            })
            .Send("hello")
            .AssertReply(
                (activity) =>
                {
                    AssertActivityHasCard(activity);
                    UpdateSimulatedInputWithPromptId(simulatedInput, prompt);
                })
            .Send(simulatedInput)
            .AssertReply($"{missingIdsMessage}: test1, test2, test3")
            .StartTestAsync();
        }

        [TestMethod]
        public async Task ShouldSuccessfullyRecognizeIfAllRequiredIdsSupplied()
        {
            // Note: Validator isn't used if !recognized.succeeded
            var missingIdsMessage = "Test Missing Ids";
            var simulatedInput = GetSimulatedInput();
            var prompt = new AdaptiveCardPrompt("prompt", null, new AdaptiveCardPromptSettings()
            {
                MissingRequiredInputsMessage = missingIdsMessage,
                RequiredInputIds = (simulatedInput.Value as JObject).Properties().Select(p => p.Name).ToList(),
            });

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
                    var options = new PromptOptions { Prompt = new Activity { Attachments = GetAttachmentsWithCard() } };
                    await dc.PromptAsync("prompt", options, cancellationToken);
                }
                else if (results.Status == DialogTurnStatus.Complete)
                {
                    var content = results.Result;
                    await turnContext.SendActivityAsync($"You said: {content.ToString()}");
                }
            })
            .Send("hello")
            .AssertReply(
                (activity) =>
                {
                    AssertActivityHasCard(activity);
                    UpdateSimulatedInputWithPromptId(simulatedInput, prompt);
                })
            .Send(simulatedInput)

            // MUST use lambda because test is async and simulatedInput changes after prompt displayed. Ref won't work because async.
            .AssertReply((activity) =>
            {
                Assert.AreEqual($"You said: {simulatedInput.Value.ToString()}", (activity as Activity).Text);
            })
            .StartTestAsync();
        }

        [TestMethod]
        public async Task ShouldRedisplayCardOnlyWhenAttemptCountDivisibleByAttemptsBeforeCardRedisplayed()
        {
            var prompt = new AdaptiveCardPrompt("prompt", null, new AdaptiveCardPromptSettings() { AttemptsBeforeCardRedisplayed = 5 });

            var simulatedInput = GetSimulatedInput();
            UpdateSimulatedInputWithPromptId(simulatedInput, null, "invalidId");

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
                    var options = new PromptOptions { Prompt = new Activity { Attachments = GetAttachmentsWithCard() } };
                    await dc.PromptAsync("prompt", options, cancellationToken);
                }
                else if (results.Status == DialogTurnStatus.Waiting)
                {
                    await turnContext.SendActivityAsync("Invalid Response");
                }
            })
            .Send("hello")
            .AssertReply(
                (activity) =>
                {
                    AssertActivityHasCard(activity);
                })
            .Send(simulatedInput)
            .AssertReply("Invalid Response")
            .Send(simulatedInput)
            .AssertReply("Invalid Response")
            .Send(simulatedInput)
            .AssertReply("Invalid Response")
            .Send(simulatedInput)
            .AssertReply("Invalid Response")
            .Send(simulatedInput)
            .AssertReply(
                (activity) =>
                {
                    AssertActivityHasCard(activity);
                })
            .StartTestAsync();
        }

        [TestMethod]
        public async Task ShouldAppropriatelyAddPromptIdToCardInAllNestedJsonOccurrences()
        {
            // Assert card doesn't already have promptIds
            var cardBefore = JObject.FromObject(GetCard().Content);
            Assert.IsTrue(string.IsNullOrEmpty(cardBefore["selectAction"]["data"]?.ToString()) || string.IsNullOrEmpty(cardBefore["selectAction"]["data"]["promptId"]?.ToString()));
            Assert.IsTrue(string.IsNullOrEmpty(cardBefore["actions"][0]["data"]?.ToString()) || string.IsNullOrEmpty(cardBefore["actions"][0]["data"]["promptId"]?.ToString()));
            Assert.IsTrue(string.IsNullOrEmpty(cardBefore["actions"][1]["card"]["actions"][0]["data"]?.ToString()) || string.IsNullOrEmpty(cardBefore["actions"][1]["card"]["actions"][0]["data"]["promptId"]?.ToString()));
            Assert.IsTrue(string.IsNullOrEmpty(cardBefore["actions"][2]["card"]["actions"][0]["data"]?.ToString()) || string.IsNullOrEmpty(cardBefore["actions"][2]["card"]["actions"][0]["data"]["promptId"]?.ToString()));
            Assert.IsTrue(string.IsNullOrEmpty(cardBefore["actions"][3]["card"]["actions"][0]["data"]?.ToString()) || string.IsNullOrEmpty(cardBefore["actions"][3]["card"]["actions"][0]["data"]["promptId"]?.ToString()));

            var prompt = new AdaptiveCardPrompt("prompt");

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
                    var options = new PromptOptions { Prompt = new Activity { Attachments = GetAttachmentsWithCard() } };
                    await dc.PromptAsync("prompt", options, cancellationToken);
                }
                else if (results.Status == DialogTurnStatus.Waiting)
                {
                    await turnContext.SendActivityAsync("Invalid Response");
                }
            })
            .Send("hello")
            .AssertReply(
                (activity) =>
                {
                    AssertActivityHasCard(activity);
                    var expectedId = prompt.PromptId;
                    var cardAfter = (activity as Activity).Attachments[0].Content as JObject;
                    Assert.AreEqual(expectedId, cardAfter["selectAction"]["data"]["promptId"].ToString());
                    Assert.AreEqual(expectedId, cardAfter["actions"][0]["data"]["promptId"].ToString());
                    Assert.AreEqual(expectedId, cardAfter["actions"][1]["card"]["actions"][0]["data"]["promptId"].ToString());
                    Assert.AreEqual(expectedId, cardAfter["actions"][2]["card"]["actions"][0]["data"]["promptId"].ToString());
                    Assert.AreEqual(expectedId, cardAfter["actions"][3]["card"]["actions"][0]["data"]["promptId"].ToString());
                })
            .StartTestAsync();
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
            }),
        };

        private Activity GetInvalidSimulatedInput() => new Activity()
        {
            Type = ActivityTypes.Message,
            Text = "Invalid Adaptive Card Input",
        };

        private void UpdateSimulatedInputWithPromptId(Activity simulatedInput, AdaptiveCardPrompt prompt = null, string promptId = null)
        {
            var jsonValue = JObject.FromObject(simulatedInput.Value ?? new { });

            if (prompt != null)
            {
                jsonValue["promptId"] = prompt.PromptId;
            }
            else if (promptId != null)
            {
                jsonValue["promptId"] = promptId;
            }
            else { throw new Exception("Must provide either prompt or promptId"); }

            simulatedInput.Value = JsonConvert.DeserializeObject(jsonValue.ToString());
        }

        private string GetPromptIdFromObject(object obj) => JObject.FromObject(obj)["promptId"].ToString();

        private void AssertActivityHasCard(IActivity activity) => Assert.AreEqual("application/vnd.microsoft.card.adaptive", (activity as Activity).Attachments[0].ContentType);
    }
}
