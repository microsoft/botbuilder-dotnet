// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;

namespace Microsoft.BotBuilderSamples.DialogRootBot.Dialogs
{
<<<<<<< HEAD
=======
    /// <summary>
    /// A simple waterfall dialog used to test triggering tangents from <see cref="MainDialog"/>.
    /// </summary>
>>>>>>> f127fca9b2eef1fe51f52bbfb2fbbab8a10fc0e8
    public class TangentDialog : ComponentDialog
    {
        public TangentDialog(string dialogId = nameof(TangentDialog))
            : base(dialogId)
        {
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            var waterfallSteps = new WaterfallStep[]
            {
                Step1Async,
<<<<<<< HEAD
                Step2Async
=======
                Step2Async,
                EndStepAsync
>>>>>>> f127fca9b2eef1fe51f52bbfb2fbbab8a10fc0e8
            };
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));

            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> Step1Async(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
<<<<<<< HEAD
            var promptMessage = MessageFactory.Text("Tangent step 1 of 2", InputHints.ExpectingInput);
=======
            var messageText = "Tangent step 1 of 2, say something.";
            var promptMessage = MessageFactory.Text(messageText, messageText, InputHints.ExpectingInput);
>>>>>>> f127fca9b2eef1fe51f52bbfb2fbbab8a10fc0e8
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);
        }

        private async Task<DialogTurnResult> Step2Async(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
<<<<<<< HEAD
            var promptMessage = MessageFactory.Text("Tangent step 2 of 2", InputHints.ExpectingInput);
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);
        }
=======
            var messageText = "Tangent step 2 of 2, say something.";
            var promptMessage = MessageFactory.Text(messageText, messageText, InputHints.ExpectingInput);
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);
        }

        private async Task<DialogTurnResult> EndStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
        }
>>>>>>> f127fca9b2eef1fe51f52bbfb2fbbab8a10fc0e8
    }
}
