// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Recognizers.Text;

namespace Microsoft.Bot.Builder.TestBot
{
    public class TestBot : IBot
    {
        private TestBotAccessors _accessors;

        public TestBot(TestBotAccessors accessors)
        {
            _accessors = accessors;
        }

        public async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (turnContext.Activity.Type == ActivityTypes.Message)
            {
                // create the DialogSet 
                await _accessors.ConversationDialogState.GetAsync(turnContext, () => new DialogState());
                var dialogs = new DialogSet(_accessors.ConversationDialogState);

                dialogs.Add(CreateWaterfall());
                dialogs.Add(new NumberPrompt<int>("number", defaultLocale: Culture.English));

                // run the DialogSet
                var dc = await dialogs.CreateContextAsync(turnContext);

                var results = await dc.ContinueAsync();
                if (!turnContext.Responded && !results.HasActive && !results.HasResult)
                {
                    await dc.BeginAsync("test-waterfall");
                }
                else if (!results.HasActive && results.HasResult)
                {
                    var value = (int)results.Result;
                    await turnContext.SendActivityAsync($"Bot received the number '{value}'.");
                }
            }
        }

        private static WaterfallDialog CreateWaterfall()
        {
            return new WaterfallDialog("test-waterfall", new WaterfallStep[] {
                WaterfallStep1,
                WaterfallStep2,
            });
        }

        private static async Task<DialogTurnResult> WaterfallStep1(DialogContext dc, WaterfallStepContext stepContext)
        {
            return await dc.PromptAsync("number", new PromptOptions { Prompt = MessageFactory.Text("Enter a number.") });
        }
        private static async Task<DialogTurnResult> WaterfallStep2(DialogContext dc, WaterfallStepContext stepContext)
        {
            if (stepContext.Values != null)
            {
                var numberResult = (int)stepContext.Result;
                await dc.Context.SendActivityAsync($"Thanks for '{numberResult}'");
            }
            return await dc.PromptAsync("number", new PromptOptions { Prompt = MessageFactory.Text("Enter another number.") });
        }
    }
}
