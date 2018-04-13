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
    public class WaterfallNestedBot : IBot
    {
        private DialogSet _dialogs;

        public WaterfallNestedBot()
        {
            _dialogs = new DialogSet();
            _dialogs.Add("test-waterfall-a", Create_Waterfall1());
            _dialogs.Add("test-waterfall-b", Create_Waterfall2());
            _dialogs.Add("test-waterfall-c", Create_Waterfall3());
        }

        public async Task OnTurn(ITurnContext turnContext)
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
                                await dc.Begin("test-waterfall-a");
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

        private static WaterfallStep[] Create_Waterfall1()
        {
            return new WaterfallStep[] {
                Waterfall1_Step1,
                Waterfall1_Step2
            };
        }
        private static WaterfallStep[] Create_Waterfall2()
        {
            return new WaterfallStep[] {
                Waterfall2_Step1,
                Waterfall2_Step2
            };
        }

        private static WaterfallStep[] Create_Waterfall3()
        {
            return new WaterfallStep[] {
                Waterfall3_Step1,
                Waterfall3_Step2
            };
        }

        private static async Task Waterfall1_Step1(DialogContext dc, object args, SkipStepFunction next)
        {
            await dc.Context.SendActivity("step1");
            await dc.Begin("test-waterfall-b");
        }
        private static async Task Waterfall1_Step2(DialogContext dc, object args, SkipStepFunction next)
        {
            await dc.Context.SendActivity("step2");
            await dc.Begin("test-waterfall-c");
        }

        private static async Task Waterfall2_Step1(DialogContext dc, object args, SkipStepFunction next)
        {
            await dc.Context.SendActivity("step1.1");
        }
        private static async Task Waterfall2_Step2(DialogContext dc, object args, SkipStepFunction next)
        {
            await dc.Context.SendActivity("step1.2");
        }

        private static async Task Waterfall3_Step1(DialogContext dc, object args, SkipStepFunction next)
        {
            await dc.Context.SendActivity("step2.1");
        }
        private static async Task Waterfall3_Step2(DialogContext dc, object args, SkipStepFunction next)
        {
            await dc.Context.SendActivity("step2.2");
            await dc.End("All Done!");
        }
    }
}
