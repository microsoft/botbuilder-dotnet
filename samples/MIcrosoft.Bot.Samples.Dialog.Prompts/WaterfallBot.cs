// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Core.Extensions;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Samples.Dialog.Prompts
{
    public class WaterfallBot : IBot
    {
        private DialogSet _dialogs;

        public WaterfallBot()
        {
            _dialogs = new DialogSet();

            _dialogs.Add("waterfall", CreateWaterfall());
        }

        public async Task OnReceiveActivity(ITurnContext turnContext)
        {
            try
            {
                switch (turnContext.Activity.Type)
                {
                    case ActivityTypes.Message:
                        var state = ConversationState<ConversationData>.Get(turnContext);
                        var dc = _dialogs.CreateContext(turnContext, state);

                        await dc.Continue();
                        var dialogResult = dc.DialogResult;

                        if (!dialogResult.Active)
                        {
                            if (dialogResult.Result != null)
                            {
                                await turnContext.SendActivity($"Waterfall concluded with '{dialogResult.Result}'.");
                            }
                            else
                            {
                                await dc.Begin("waterfall");
                            }
                        }

                        break;

                    case ActivityTypes.ConversationUpdate:
                        foreach (var newMember in turnContext.Activity.MembersAdded)
                        {
                            if (newMember.Id != turnContext.Activity.Recipient.Id)
                            {
                                await turnContext.SendActivity("Hello and welcome to the waterfall bot.");
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
        }
        private async Task WaterfallStep2(DialogContext dc, object args, SkipStepFunction next)
        {
            await dc.Context.SendActivity("step2");
        }
        private async Task WaterfallStep3(DialogContext dc, object args, SkipStepFunction next)
        {
            await dc.Context.SendActivity("step3");
            await dc.End("All Done!");
        }
    }
}
