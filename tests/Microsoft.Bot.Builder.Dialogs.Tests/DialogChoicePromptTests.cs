// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Recognizers.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Dialogs.Tests
{
    [TestClass]
    public class DialogChoicePromptTests
    {
        private List<string> colorChoices = new List<string> { "red", "green", "blue" };

        [TestMethod]
        public async Task BasicChoicePrompt()
        {
            var dialogs = new DialogSet();

            dialogs.Add("test-prompt", new ChoicePrompt(Culture.English) { Style = ListStyle.Inline });

            var promptOptions = new ChoicePromptOptions
            {
                Choices = new List<Choice>
                {
                    new Choice { Value = "red" },
                    new Choice { Value = "green" },
                    new Choice { Value = "blue" },
                }
            };

            dialogs.Add("test",
                new WaterfallStep[]
                {
                    async (dc, args, next) =>
                    {
                        await dc.PromptAsync("test-prompt", "favorite color?", promptOptions);
                    },
                    async (dc, args, next) =>
                    {
                        var choiceResult = (ChoiceResult)args;
                        await dc.Context.SendActivityAsync($"Bot received the choice '{choiceResult.Value.Value}'.");
                        await dc.EndAsync();
                    }
                }
            );

            ConversationState convoState = new ConversationState(new MemoryStorage());
            var testProperty = convoState.CreateProperty<Dictionary<string, object>>("test");

            TestAdapter adapter = new TestAdapter()
                .Use(convoState);

            await new TestFlow(adapter, async (turnContext, cancellationToken) =>
            {
                var state = await testProperty.GetAsync(turnContext, () => new Dictionary<string, object>());
                var dc = dialogs.CreateContext(turnContext, state);

                await dc.ContinueAsync();

                if (!turnContext.Responded)
                {
                    await dc.BeginAsync("test");
                }
            })
            .Send("hello")
            .AssertReply("favorite color? (1) red, (2) green, or (3) blue")
            .Send("green")
            .AssertReply("Bot received the choice 'green'.")
            .StartTestAsync();
        }
    }
}
