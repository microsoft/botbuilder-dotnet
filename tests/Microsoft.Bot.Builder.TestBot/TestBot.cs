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
                // using store accessors to get dialog state
                await _accessors.ConversationDialogState.GetAsync(turnContext, () => new DialogState());

                // create the DialogSet from current dialog state
                var dialogs = new DialogSet(_accessors.ConversationDialogState);

                // each OnTurn potantially represent a new instanse of the bot, thus we need to recreate the set
                // deifne dialogs and related 
                dialogs.Add(CreateWaterfall());
                dialogs.Add(new NumberPrompt<int>("number", defaultLocale: Culture.English));

                // run the DialogSet - let the framework identify the current state of the dialog from 
                // the dialog stack and figure out what (if any) is the active dialog
                var dc = await dialogs.CreateContextAsync(turnContext);
                var results = await dc.ContinueAsync();

                // HasActive = true if there is an active dialog on the dialogstack
                // HasResults = true if the dialog just completed and the final  result can be retrived
                // if both are false this indicates a new dialog needs to start
                if( !results.HasActive && !results.HasResult)
                {
                    await dc.BeginAsync("test-waterfall");

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
