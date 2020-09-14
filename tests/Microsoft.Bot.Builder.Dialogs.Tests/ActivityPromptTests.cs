// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Schema;
using Xunit;

namespace Microsoft.Bot.Builder.Dialogs.Tests
{
    public class ActivityPromptTests
    {
        [Fact]
        public void ActivityPromptWithEmptyIdShouldFail()
        {
            Assert.Throws<ArgumentNullException>(() => new EventActivityPrompt(string.Empty, Validator));
        }

        [Fact]
        public void ActivityPromptWithNullIdShouldFail()
        {
            Assert.Throws<ArgumentNullException>(() => new EventActivityPrompt(null, Validator));
        }

        [Fact]
        public void ActivityPromptWithNullValidatorShouldFail()
        {
            Assert.Throws<ArgumentNullException>(() => new EventActivityPrompt("EventActivityPrompt", null));
        }

        [Fact]
        public async Task BasicActivityPrompt()
        {
            var convoState = new ConversationState(new MemoryStorage());
            var dialogState = convoState.CreateProperty<DialogState>("dialogState");

            var adapter = new TestAdapter()
                .Use(new AutoSaveStateMiddleware(convoState));

            // Create new DialogSet.
            var dialogs = new DialogSet(dialogState);

            // Create and add custom activity prompt to DialogSet.
            var eventPrompt = new EventActivityPrompt("EventActivityPrompt", Validator);
            dialogs.Add(eventPrompt);

            // Create mock Activity for testing.
            var eventActivity = new Activity { Type = ActivityTypes.Event, Value = 2 };

            await new TestFlow(adapter, async (turnContext, cancellationToken) =>
            {
                var dc = await dialogs.CreateContextAsync(turnContext, cancellationToken);

                var results = await dc.ContinueDialogAsync(cancellationToken);
                if (results.Status == DialogTurnStatus.Empty)
                {
                    var options = new PromptOptions { Prompt = new Activity { Type = ActivityTypes.Message, Text = "please send an event." } };
                    await dc.PromptAsync("EventActivityPrompt", options, cancellationToken);
                }
                else if (results.Status == DialogTurnStatus.Complete)
                {
                    var content = (Activity)results.Result;
                    await turnContext.SendActivityAsync(content, cancellationToken);
                }
            })
            .Send("hello")
            .AssertReply("please send an event.")
            .Send(eventActivity)
            .AssertReply("2")
            .StartTestAsync();
        }

        [Fact]
        public async Task ActivityPromptShouldSendRetryPromptIfValidationFailed()
        {
            var convoState = new ConversationState(new MemoryStorage());
            var dialogState = convoState.CreateProperty<DialogState>("dialogState");

            var adapter = new TestAdapter()
                .Use(new AutoSaveStateMiddleware(convoState));

            var dialogs = new DialogSet(dialogState);

            PromptValidator<Activity> validator = (prompt, cancellationToken) =>
            {
                return Task.FromResult(false);
            };

            var eventPrompt = new EventActivityPrompt("EventActivityPrompt", validator);
            dialogs.Add(eventPrompt);

            var eventActivity = new Activity { Type = ActivityTypes.Event, Value = 2 };

            await new TestFlow(adapter, async (turnContext, cancellationToken) =>
            {
                var dc = await dialogs.CreateContextAsync(turnContext, cancellationToken);
                var results = await dc.ContinueDialogAsync(cancellationToken);
                if (results.Status == DialogTurnStatus.Empty)
                {
                    var options = new PromptOptions
                    {
                        Prompt = new Activity
                        {
                            Type = ActivityTypes.Message,
                            Text = "please send an event.",
                        },
                        RetryPrompt = new Activity
                        {
                            Type = ActivityTypes.Message,
                            Text = "Retrying - please send an event.",
                        },
                    };

                    await dc.PromptAsync("EventActivityPrompt", options);
                }
                else if (results.Status == DialogTurnStatus.Complete)
                {
                    var content = (Activity)results.Result;
                    await turnContext.SendActivityAsync(content, cancellationToken);
                }
                else if (results.Status == DialogTurnStatus.Waiting)
                {
                    await turnContext.SendActivityAsync("Test complete.");
                }
            })
            .Send("hello")
            .AssertReply("please send an event.")
            .Send("test")
            .AssertReply("Retrying - please send an event.")
            .StartTestAsync();
        }

        [Fact]
        public async Task ActivityPromptResumeDialogShouldPromptNotRetry()
        {
            var convoState = new ConversationState(new MemoryStorage());
            var dialogState = convoState.CreateProperty<DialogState>("dialogState");

            var adapter = new TestAdapter()
                .Use(new AutoSaveStateMiddleware(convoState));

            var dialogs = new DialogSet(dialogState);
            var eventPrompt = new EventActivityPrompt("EventActivityPrompt", (prompt, cancellationToken) => Task.FromResult(false));

            dialogs.Add(eventPrompt);

            await new TestFlow(adapter, async (turnContext, cancellationToken) =>
            {
                var dc = await dialogs.CreateContextAsync(turnContext, cancellationToken);

                switch (turnContext.Activity.Text)
                {
                    case "begin":

                        var options = new PromptOptions
                        {
                            Prompt = new Activity
                            {
                                Type = ActivityTypes.Message,
                                Text = "please send an event.",
                            },
                            RetryPrompt = new Activity
                            {
                                Type = ActivityTypes.Message,
                                Text = "Retrying - please send an event.",
                            },
                        };

                        await dc.PromptAsync("EventActivityPrompt", options);

                        break;

                    case "continue":

                        await eventPrompt.ContinueDialogAsync(dc);

                        break;

                    case "resume":

                        await eventPrompt.ResumeDialogAsync(dc, DialogReason.NextCalled);

                        break;
                }
            })
            .Send("begin")
            .AssertReply("please send an event.")
            .Send("continue")
            .AssertReply("Retrying - please send an event.")
            .Send("resume")

            // 'ResumeDialogAsync' of ActivityPrompt does NOT cause a Retry
            .AssertReply("please send an event.")
            .StartTestAsync();
        }

        [Fact]
        public async Task OnPromptOverloadWithoutIsRetryParamReturnsBasicActivityPrompt()
        {
            var convoState = new ConversationState(new MemoryStorage());
            var dialogState = convoState.CreateProperty<DialogState>("dialogState");

            var adapter = new TestAdapter()
                .Use(new AutoSaveStateMiddleware(convoState));

            // Create new DialogSet.
            var dialogs = new DialogSet(dialogState);

            // Create and add custom activity prompt to DialogSet.
            var eventPrompt = new EventActivityWithoutRetryPrompt("EventActivityWithoutRetryPrompt", Validator);
            dialogs.Add(eventPrompt);

            // Create mock Activity for testing.
            var eventActivity = new Activity { Type = ActivityTypes.Event, Value = 2 };

            await new TestFlow(adapter, async (turnContext, cancellationToken) =>
            {
                var dc = await dialogs.CreateContextAsync(turnContext, cancellationToken);

                var results = await dc.ContinueDialogAsync(cancellationToken);
                if (results.Status == DialogTurnStatus.Empty)
                {
                    var options = new PromptOptions { Prompt = new Activity { Type = ActivityTypes.Message, Text = "please send an event." } };
                    await dc.PromptAsync("EventActivityWithoutRetryPrompt", options, cancellationToken);
                }
                else if (results.Status == DialogTurnStatus.Complete)
                {
                    var content = (Activity)results.Result;
                    await turnContext.SendActivityAsync(content, cancellationToken);
                }
            })
            .Send("hello")
            .AssertReply("please send an event.")
            .Send(eventActivity)
            .AssertReply("2")
            .StartTestAsync();
        }

        [Fact]
        public async Task OnPromptErrorsWithNullContext()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                var eventPrompt = new EventActivityPrompt("EventActivityPrompt", Validator);

                var options = new PromptOptions { Prompt = new Activity { Type = ActivityTypes.Message, Text = "please send an event." } };

                await eventPrompt.OnPromptNullContext(options);
            });
        }

        [Fact]
        public async Task OnPromptErrorsWithNullOptions()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                var convoState = new ConversationState(new MemoryStorage());
                var dialogState = convoState.CreateProperty<DialogState>("dialogState");

                var adapter = new TestAdapter()
                    .Use(new AutoSaveStateMiddleware(convoState));

                // Create new DialogSet.
                var dialogs = new DialogSet(dialogState);

                // Create and add custom activity prompt to DialogSet.
                var eventPrompt = new EventActivityPrompt("EventActivityPrompt", Validator);
                dialogs.Add(eventPrompt);

                await new TestFlow(adapter, async (turnContext, cancellationToken) =>
                {
                    var dc = await dialogs.CreateContextAsync(turnContext, cancellationToken);

                    await eventPrompt.OnPromptNullOptions(dc);
                })
                .Send("hello")
                .StartTestAsync();
            });
        }

        private async Task<bool> Validator(PromptValidatorContext<Activity> promptContext, CancellationToken cancellationToken)
        {
            Assert.True(promptContext.AttemptCount > 0);

            var activity = promptContext.Recognized.Value;
            if (activity.Type == ActivityTypes.Event)
            {
                if ((int)activity.Value == 2)
                {
                    promptContext.Recognized.Value = MessageFactory.Text(activity.Value.ToString());
                    return true;
                }
            }
            else
            {
                await promptContext.Context.SendActivityAsync("Please send an 'event'-type Activity with a value of 2.");
            }

            return false;
        }
    }
}
