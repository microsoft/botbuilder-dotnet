// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Core.Extensions;
using Microsoft.Bot.Builder.Prompts;
using Microsoft.Bot.Builder.Prompts.Choices;
using Microsoft.Bot.Builder.Testing;
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
                        await dc.Prompt("test-prompt", "favorite color?", promptOptions);
                    },
                    async (dc, args, next) =>
                    {
                        var choiceResult = (ChoiceResult)args;
                        await dc.Context.SendActivity($"Bot received the choice '{choiceResult.Value.Value}'.");
                        await dc.End();
                    }
                }
            );

            TestAdapter adapter = new TestAdapter()
                .Use(new ConversationState<Dictionary<string, object>>(new MemoryStorage()));

            await new TestFlow(adapter, async (turnContext) =>
            {
                var state = ConversationState<Dictionary<string, object>>.Get(turnContext);
                var dc = dialogs.CreateContext(turnContext, state);

                await dc.Continue();

                if (!turnContext.Responded)
                {
                    await dc.Begin("test");
                }
            })
            .Send("hello")
            .AssertReply("favorite color? (1) red, (2) green, or (3) blue")
            .Send("green")
            .AssertReply("Bot received the choice 'green'.")
            .StartTest();
        }
    }
}
