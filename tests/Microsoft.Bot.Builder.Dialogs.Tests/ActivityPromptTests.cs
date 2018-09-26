// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Dialogs.Tests
{
    [TestClass]
    public class ActivityPromptTests
    {
        private async Task<bool> _validator(PromptValidatorContext<Activity> promptContext, CancellationToken cancellationToken)
        {
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

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ActivityPromptWithEmptyIdShouldFail()
        {
            var emptyId = "";
            var textPrompt = new EventActivityPrompt(emptyId, _validator);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ActivityPromptWithNullIdShouldFail()
        {
            var nullId = "";
            nullId = null;
            var textPrompt = new EventActivityPrompt(nullId, _validator);
        }
        
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ActivityPromptWithNullValidatorShouldFail()
        {
            var validator = (PromptValidator<Activity>)_validator;
            validator = null;
            var textPrompt = new EventActivityPrompt("EventActivityPrompt", validator);
        }

        [TestMethod]
        public async Task BasicActivityPrompt()
        {
            var convoState = new ConversationState(new MemoryStorage());
            var dialogState = convoState.CreateProperty<DialogState>("dialogState");

            var adapter = new TestAdapter()
                .Use(new AutoSaveStateMiddleware(convoState));

            // Create new DialogSet.
            var dialogs = new DialogSet(dialogState);

            // Create and add custom activity prompt to DialogSet.
            var eventPrompt = new EventActivityPrompt("EventActivityPrompt", _validator);
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

        [TestMethod]
        public async Task RetryAttachmentPrompt()
        {
            var convoState = new ConversationState(new MemoryStorage());
            var dialogState = convoState.CreateProperty<DialogState>("dialogState");

            var adapter = new TestAdapter()
                .Use(new AutoSaveStateMiddleware(convoState));

            var dialogs = new DialogSet(dialogState);

            var eventPrompt = new EventActivityPrompt("EventActivityPrompt", _validator);
            dialogs.Add(eventPrompt);

            var eventActivity = new Activity { Type = ActivityTypes.Event, Value = 2 };

            await new TestFlow(adapter, async (turnContext, cancellationToken) =>
            {
                var dc = await dialogs.CreateContextAsync(turnContext, cancellationToken);
                var results = await dc.ContinueDialogAsync(cancellationToken);
                if (results.Status == DialogTurnStatus.Empty)
                {
                    var options = new PromptOptions { Prompt = new Activity { Type = ActivityTypes.Message, Text = "please send an event." } };
                    await dc.PromptAsync("EventActivityPrompt", options);
                }
                else if (results.Status == DialogTurnStatus.Complete)
                {
                    var content = (Activity)results.Result;
                    await turnContext.SendActivityAsync(content, cancellationToken);
                }
            })
            .Send("hello")
            .AssertReply("please send an event.")
            .Send("hello again")
            .AssertReply("Please send an 'event'-type Activity with a value of 2.")
            .Send(eventActivity)
            .AssertReply("2")
            .StartTestAsync();
        }

        [TestMethod]
        public async Task ActivityPromptShouldReturnDialogEndOfTurnIfValidationFailed()
        {
            var convoState = new ConversationState(new MemoryStorage());
            var dialogState = convoState.CreateProperty<DialogState>("dialogState");

            var adapter = new TestAdapter()
                .Use(new AutoSaveStateMiddleware(convoState));

            var dialogs = new DialogSet(dialogState);

            PromptValidator<Activity> validator = async (prompt, cancellationToken) =>
            {
                return false;
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
                    var options = new PromptOptions { Prompt = new Activity { Type = ActivityTypes.Message, Text = "please send an event." } };
                    await dc.PromptAsync("EventActivityPrompt", options);
                }
                else if (results.Status == DialogTurnStatus.Complete)
                {
                    var content = (Activity)results.Result;
                    await turnContext.SendActivityAsync(content, cancellationToken);
                } else if (results.Status == DialogTurnStatus.Waiting)
                {
                    await turnContext.SendActivityAsync("Test complete.");
                }
            })
            .Send("hello")
            .AssertReply("please send an event.")
            .Send("test")
            .AssertReply("Test complete.")
            .StartTestAsync();
        }

        [TestMethod]
        public async Task ActivityPromptResumeDialogShouldReturnDialogEndOfTurn()
        {
            var convoState = new ConversationState(new MemoryStorage());
            var dialogState = convoState.CreateProperty<DialogState>("dialogState");

            var adapter = new TestAdapter()
                .Use(new AutoSaveStateMiddleware(convoState));

            var dialogs = new DialogSet(dialogState);

            PromptValidator<Activity> validator = async (prompt, cancellationToken) =>
            {
                return false;
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
                    var options = new PromptOptions { Prompt = new Activity { Type = ActivityTypes.Message, Text = "please send an event." } };
                    await dc.PromptAsync("EventActivityPrompt", options);
                }
                var secondResults = await eventPrompt.ResumeDialogAsync(dc, DialogReason.NextCalled);

                if (secondResults.Status == DialogTurnStatus.Waiting)
                {
                    await turnContext.SendActivityAsync("Test complete.");
                }
            })
            .Send("hello")
            .AssertReply("please send an event.")
            .AssertReply("please send an event.")
            .AssertReply("Test complete.")
            .StartTestAsync();
        }
    }

    public class EventActivityPrompt : ActivityPrompt
    {
        public EventActivityPrompt(string dialogId, PromptValidator<Activity> validator)
            : base(dialogId, validator)
        {
        }
    }
}
