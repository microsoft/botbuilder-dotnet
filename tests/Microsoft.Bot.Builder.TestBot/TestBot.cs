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
        private DialogSet _dialogs;

        public TestBot(TestBotAccessors accessors)
        {
            // create the DialogSet from accessor
            _dialogs = new DialogSet(accessors.ConversationDialogState);

            // add the various named dialogs that can be used
            _dialogs.Add(CreateWaterfall());
            _dialogs.Add(new NumberPrompt<int>("number", defaultLocale: Culture.English));
        }

        public async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (turnContext.Activity.Type == ActivityTypes.Message)
            {
                // run the DialogSet - let the framework identify the current state of the dialog from 
                // the dialog stack and figure out what (if any) is the active dialog
                var dialogContext = await _dialogs.CreateContextAsync(turnContext);
                var results = await dialogContext.ContinueAsync();

                // HasActive = true if there is an active dialog on the dialogstack
                // HasResults = true if the dialog just completed and the final  result can be retrived
                // if both are false this indicates a new dialog needs to start
                if(!turnContext.Responded && !results.HasActive && !results.HasResult)
                {
                    await dialogContext.BeginAsync("test-waterfall");

                    // DO NOT start another dialog 
                    //await dc.BeginAsync("test-waterfall1");
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
            // this prompt will not continue until we receive a number
            return await dc.PromptAsync("number", new PromptOptions { Prompt = MessageFactory.Text("Enter a number.") });
        }
        private static async Task<DialogTurnResult> WaterfallStep2(DialogContext dc, WaterfallStepContext stepContext)
        {
            // step context represents values from previous (waterfall) step - in this case the first number
            if (stepContext.Values != null)
            {
                var numberResult = (int)stepContext.Result;
                await dc.Context.SendActivityAsync($"Thanks for '{numberResult}'");
            }
            return await dc.PromptAsync("number", new PromptOptions { Prompt = MessageFactory.Text("Enter another number.") });
        }
    }
}
