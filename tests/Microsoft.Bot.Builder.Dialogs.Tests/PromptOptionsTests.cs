// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Schema;
using Xunit;

namespace Microsoft.Bot.Builder.Dialogs.Tests
{
    public class PromptOptionsTests
    {
        private const string OptionsTestFieldValue = "Test field value";

        [Fact]
        public async Task OverriddenOptionsRetainValuesThroughTypeSerialization_Test()
        {
            var convoState = new ConversationState(new MemoryStorage());
            var dialogState = convoState.CreateProperty<DialogState>("dialogState");

            var adapter = new TestAdapter()
                .Use(new AutoSaveStateMiddleware(convoState));

            // Create new DialogSet.
            var dialogs = new DialogSet(dialogState);

            PromptValidator<Activity> validator = async (promptContext, cancellationToken) => 
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
            };

            // Create and add custom activity prompt to DialogSet.
            var eventPrompt = new PromptOptionsTestPrompt("PromptOptionsTestPrompt", validator);
            dialogs.Add(eventPrompt);

            // Create mock Activity for testing.
            var eventActivity = new Activity { Type = ActivityTypes.Event, Value = 2 };

            await new TestFlow(adapter, async (turnContext, cancellationToken) =>
            {
                var dc = await dialogs.CreateContextAsync(turnContext, cancellationToken);

                var results = await dc.ContinueDialogAsync(cancellationToken);
                if (results.Status == DialogTurnStatus.Empty)
                {
                    var options = new TestOptions { TestField = OptionsTestFieldValue, Prompt = new Activity { Type = ActivityTypes.Message, Text = "please send an event." } };
                    await dc.PromptAsync("PromptOptionsTestPrompt", options, cancellationToken);
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

#pragma warning disable SA1402 // File may only contain a single type
        public class PromptOptionsTestPrompt : ActivityPrompt
        {
            private const string PersistedOptions = "options";

            public PromptOptionsTestPrompt(string dialogId, PromptValidator<Activity> validator)
                : base(dialogId, validator)
            {
            }

            public override Task<DialogTurnResult> ContinueDialogAsync(DialogContext dc, CancellationToken cancellationToken = default)
            {
                // The oder the deserialization of types below matters
                var asPromptOptions = dc.ActiveDialog.State.MapValueTo<PromptOptions>(PersistedOptions);

                Assert.IsType<PromptOptions>(asPromptOptions);
                Assert.True(asPromptOptions.Properties.ContainsKey("TestField"));

                var asTestOptions = dc.ActiveDialog.State.MapValueTo<TestOptions>(PersistedOptions);
                Assert.IsType<TestOptions>(asTestOptions);
                Assert.Equal(asTestOptions.TestField, OptionsTestFieldValue);

                var asPromptOptions2 = dc.ActiveDialog.State.MapValueTo<PromptOptions>(PersistedOptions);
                Assert.IsType<TestOptions>(asPromptOptions2);

                return base.ContinueDialogAsync(dc, cancellationToken);
            }
        }

        public class TestOptions : PromptOptions
#pragma warning restore SA1402 // File may only contain a single type
        {
            public string TestField { get; set; }
        }
    }
}
