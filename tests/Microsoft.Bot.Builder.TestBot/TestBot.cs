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
        private SemaphoreSlim _semaphore;

        public TestBot(TestBotAccessors accessors)
        {
            // create the DialogSet from accessor
            _dialogs = new DialogSet(accessors.ConversationDialogState);

            // a semaphore to serialize access to the bot state
            _semaphore = accessors.SemaphoreSlim;

            // add the various named dialogs that can be used
            _dialogs.Add(CreateWaterfall());
            _dialogs.Add(new NumberPrompt<int>("number", defaultLocale: Culture.English));
        }

        public async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            // We only want to pump one activity at a time through the state.
            // Note the state is shared across all instances of this IBot class so we
            // create the semaphore globally with the accessors.
            try
            {
                await _semaphore.WaitAsync();

                // run the DialogSet - let the framework identify the current state of the dialog from 
                // the dialog stack and figure out what (if any) is the active dialog
                var dialogContext = await _dialogs.CreateContextAsync(turnContext, cancellationToken);
                var results = await dialogContext.ContinueAsync(cancellationToken);

                // HasActive = true if there is an active dialog on the dialogstack
                // HasResults = true if the dialog just completed and the final  result can be retrived
                // if both are false this indicates a new dialog needs to start
                // an additional check for Responded stops a new waterfall from being automatically started over
                if (!turnContext.Responded && !results.HasActive && !results.HasResult)
                {
                    await dialogContext.BeginAsync("test-waterfall", null, cancellationToken);
                }
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private static WaterfallDialog CreateWaterfall()
        {
            return new WaterfallDialog("test-waterfall", new WaterfallStep[] {
                WaterfallStep1,
                WaterfallStep2,
                WaterfallStep3
            });
        }

        private static async Task<DialogTurnResult> WaterfallStep1(DialogContext dc, WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // we are only interested in Message activities - any other type of activity we will immediately complete teh waterfall
            if (dc.Context.Activity.Type != ActivityTypes.Message)
            {
                return await dc.EndAsync(cancellationToken);
            }

            // this prompt will not continue until we receive a number
            return await dc.PromptAsync("number", new PromptOptions { Prompt = MessageFactory.Text("Enter a number.") }, cancellationToken);
        }
        private static async Task<DialogTurnResult> WaterfallStep2(DialogContext dc, WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // step context represents values from previous (waterfall) step - in this case the first number
            if (stepContext.Values != null)
            {
                var numberResult = (int)stepContext.Result;
                await dc.Context.SendActivityAsync(MessageFactory.Text($"Thanks for '{numberResult}'"), cancellationToken);
            }
            return await dc.PromptAsync("number", new PromptOptions { Prompt = MessageFactory.Text("Enter another number.") }, cancellationToken);
        }
        private static async Task<DialogTurnResult> WaterfallStep3(DialogContext dc, WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var value = (int)stepContext.Result;
            await dc.Context.SendActivityAsync(MessageFactory.Text($"Bot received the number '{value}'."), cancellationToken);
            return await dc.EndAsync(cancellationToken);
        }
    }
}
