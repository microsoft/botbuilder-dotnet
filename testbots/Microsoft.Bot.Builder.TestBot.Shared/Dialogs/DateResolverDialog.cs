// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Recognizers.Text.DataTypes.TimexExpression;

namespace Microsoft.Bot.Builder.TestBot.Shared.Dialogs
{
    public class DateResolverDialog : CancelAndHelpDialog
    {
        public DateResolverDialog(string id = null)
            : base(id ?? nameof(DateResolverDialog))
        {
            AddDialog(new DateTimePrompt(nameof(DateTimePrompt), DateTimePromptValidator));
            var steps = new WaterfallStep[]
            {
                InitialActionAsync,
                FinalActionAsync,
            };
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), steps));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        private static Task<bool> DateTimePromptValidator(PromptValidatorContext<IList<DateTimeResolution>> promptContext, CancellationToken cancellationToken)
        {
            if (promptContext.Recognized.Succeeded)
            {
                // This value will be a TIMEX. And we are only interested in a Date so grab the first result and drop the Time part.
                // TIMEX is a format that represents DateTime expressions that include some ambiguity. e.g. missing a Year.
                var timex = promptContext.Recognized.Value[0].Timex.Split('T')[0];

                // If this is a definite Date including year, month and day we are good otherwise reprompt.
                // A better solution might be to let the user know what part is actually missing.
                var isDefinite = new TimexProperty(timex).Types.Contains(Constants.TimexTypes.Definite);

                return Task.FromResult(isDefinite);
            }

            return Task.FromResult(false);
        }

        private async Task<DialogTurnResult> InitialActionAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var timex = (string)stepContext.Options;

            var promptMsg = "When would you like to travel?";
            var repromptMsg = "I'm sorry, to make your booking please enter a full travel date including Day Month and Year.";

            if (timex == null)
            {
                // We were not given any date at all so prompt the user.
                return await stepContext.PromptAsync(
                    nameof(DateTimePrompt),
                    new PromptOptions
                    {
                        Prompt = MessageFactory.Text(promptMsg),
                        RetryPrompt = MessageFactory.Text(repromptMsg),
                    }, cancellationToken);
            }

            // We have a Date we just need to check it is unambiguous.
            var timexProperty = new TimexProperty(timex);
            if (!timexProperty.Types.Contains(Constants.TimexTypes.Definite))
            {
                // This is essentially a "reprompt" of the data we were given up front.
                return await stepContext.PromptAsync(
                    nameof(DateTimePrompt),
                    new PromptOptions
                    {
                        Prompt = MessageFactory.Text(repromptMsg),
                    }, cancellationToken);
            }

            return await stepContext.NextAsync(new List<DateTimeResolution> { new DateTimeResolution { Timex = timex } }, cancellationToken);
        }

        private async Task<DialogTurnResult> FinalActionAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var timex = ((List<DateTimeResolution>)stepContext.Result)[0].Timex;
            return await stepContext.EndDialogAsync(timex, cancellationToken);
        }
    }
}
