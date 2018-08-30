// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Recognizers.Text;

namespace Microsoft.Bot.Builder.TestBot
{
    public class TestBot : DialogBotBase
    {
        const string RootDialogName = "test-waterfall";

        public TestBot(TestBotAccessors accessors)
            : base(accessors, RootDialogName)
        {
            // add the various named dialogs that can be used
            Dialogs.Add(CreateWaterfall());
            Dialogs.Add(new NumberPrompt<int>("number", defaultLocale: Culture.English));
        }

        private static WaterfallDialog CreateWaterfall()
        {
            return new WaterfallDialog(RootDialogName, new WaterfallStep[] {
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
