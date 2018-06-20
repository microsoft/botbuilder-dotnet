// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Core.Extensions;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Recognizers.Text;

namespace Microsoft.Bot.Samples.Dialog.Prompts
{
    public class WaterfallAndPromptBot : IBot
    {
        private DialogSet _dialogs;

        public WaterfallAndPromptBot()
        {
            _dialogs = new DialogSet();

            _dialogs.Add("waterfall", CreateWaterfall());
            _dialogs.Add("number", new Builder.Dialogs.NumberPrompt<int>(Culture.English));
        }

        public async Task OnTurn(ITurnContext turnContext)
        {
            try
            {
                switch (turnContext.Activity.Type)
                {
                    case ActivityTypes.Message:
                        var state = ConversationState<Dictionary<string, object>>.Get(turnContext);
                        var dc = _dialogs.CreateContext(turnContext, state);

                        await dc.Continue();

                        if (!turnContext.Responded)
                        {
                            await dc.Begin("waterfall");
                        }
                        break;

                    case ActivityTypes.ConversationUpdate:
                        foreach (var newMember in turnContext.Activity.MembersAdded)
                        {
                            if (newMember.Id != turnContext.Activity.Recipient.Id)
                            {
                                await turnContext.SendActivity("Hello and welcome to the waterfall and prompt bot.");
                            }
                        }
                        break;
                }
            }
            catch (Exception e)
            {
                await turnContext.SendActivity($"Exception: {e.Message}");
            }
        }

        private WaterfallStep[] CreateWaterfall()
        {
            return new WaterfallStep[] {
                WaterfallStep1,
                WaterfallStep2,
                WaterfallStep3
            };
        }

        private async Task WaterfallStep1(DialogContext dc, object args, SkipStepFunction next)
        {
            await dc.Context.SendActivity("step1");
            await dc.Prompt("number", "Enter a number.", new PromptOptions { RetryPromptString = "It must be a number" });
        }
        private async Task WaterfallStep2(DialogContext dc, object args, SkipStepFunction next)
        {
            if (args != null)
            {
                var numberResult = (NumberResult<int>)args;
                await dc.Context.SendActivity($"Thanks for '{numberResult.Value}'");
            }
            await dc.Context.SendActivity("step2");
            await dc.Prompt("number", "Enter a number.", new PromptOptions { RetryPromptString = "It must be a number" });
        }
        private async Task WaterfallStep3(DialogContext dc, object args, SkipStepFunction next)
        {
            if (args != null)
            {
                var numberResult = (NumberResult<int>)args;
                await dc.Context.SendActivity($"Thanks for '{numberResult.Value}'");
            }
            await dc.Context.SendActivity("step3");
            await dc.End();
        }
    }
}
