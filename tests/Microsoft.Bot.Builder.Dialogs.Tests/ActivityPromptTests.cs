// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Dialogs.Tests
{
    [TestClass]
    public class ActivityPromptTests
    {

        [TestMethod]
        public async Task BasicActivityPrompt()
        {
            ConversationState convoState = new ConversationState(new MemoryStorage());
            var dialogState = convoState.CreateProperty<DialogState>("dialogState");

            TestAdapter adapter = new TestAdapter()
                .Use(convoState);

            // Create new DialogSet.
            DialogSet dialogs = new DialogSet(dialogState);

            // Create and add custom activity prompt to DialogSet.
            var eventPrompt = new EventActivityPrompt("EventActivityPrompt", async (context, promptContext) =>
            {
                var activity = (Activity)promptContext.Recognized.Value;
                if (activity.Type == ActivityTypes.Event)
                {
                    if ((int)activity.Value == 2)
                    {
                        promptContext.End(activity.Value);
                    }
                }
                else
                {
                    await context.SendActivityAsync("Please send an 'event'-type Activity with a value of 2.");
                }
            });
            dialogs.Add(eventPrompt);

            // Create mock Activity for testing.
            var eventActivity = new Activity { Type = ActivityTypes.Event, Value = 2 };

            await new TestFlow(adapter, async (turnContext, cancellationToken) =>
            {
                var dc = await dialogs.CreateContextAsync(turnContext);

                var results = await dc.ContinueAsync();
                if (!turnContext.Responded && !results.HasActive && !results.HasResult)
                {
                    var options = new PromptOptions { Prompt = new Activity { Type = ActivityTypes.Message, Text = "please send an event." } };
                    await dc.PromptAsync("EventActivityPrompt", options);
                }
                else if (!results.HasActive && results.HasResult)
                {
                    var content = results.Result.ToString();
                    await turnContext.SendActivityAsync(content);
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
            ConversationState convoState = new ConversationState(new MemoryStorage());
            var dialogState = convoState.CreateProperty<DialogState>("dialogState");

            TestAdapter adapter = new TestAdapter()
                .Use(convoState);

            DialogSet dialogs = new DialogSet(dialogState);

            var eventPrompt = new EventActivityPrompt("EventActivityPrompt", async (context, promptContext) =>
            {
                var activity = (Activity)promptContext.Recognized.Value;
                if (activity.Type == ActivityTypes.Event)
                {
                    if ((int)activity.Value == 2)
                    {
                        promptContext.End(activity.Value);
                    }
                }
                else
                {
                    await context.SendActivityAsync("Please send an 'event'-type Activity with a value of 2.");
                }
            });
            dialogs.Add(eventPrompt);

            var eventActivity = new Activity { Type = ActivityTypes.Event, Value = 2 };

            await new TestFlow(adapter, async (turnContext, cancellationToken) =>
            {
                var dc = await dialogs.CreateContextAsync(turnContext);
                var results = await dc.ContinueAsync();
                if (!turnContext.Responded && !results.HasActive && !results.HasResult)
                {
                    var options = new PromptOptions { Prompt = new Activity { Type = ActivityTypes.Message, Text = "please send an event." } };
                    await dc.PromptAsync("EventActivityPrompt", options);
                }
                else if (!results.HasActive && results.HasResult)
                {
                    var content = results.Result.ToString();
                    await turnContext.SendActivityAsync(content);
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
    }

    public class EventActivityPrompt : ActivityPrompt
    {
        public EventActivityPrompt(string dialogId, PromptValidator<Activity> validator)
            : base(dialogId, validator)
        {
        }
    }
}
