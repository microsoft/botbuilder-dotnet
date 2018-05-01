// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Core.Extensions;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Samples.Dialog.Prompts
{
    public class ProfileControl : DialogContainer
    {
        public ProfileControl()
            : base("fillProfile")
        {
            Dialogs.Add("fillProfile", 
                new WaterfallStep[]
                {
                    async (dc, args, next) =>
                    {
                        dc.ActiveDialog.State = new Dictionary<string, object>();
                        await dc.Prompt("textPrompt", "What's your name?");
                    },
                    async (dc, args, next) =>
                    {
                        dc.ActiveDialog.State["name"] = args["Value"];
                        await dc.Prompt("textPrompt", "What's your phone number?");
                    },
                    async (dc, args, next) =>
                    {
                        dc.ActiveDialog.State["phone"] = args["Value"];
                        await dc.End(dc.ActiveDialog.State);
                    }
                }
            );
            Dialogs.Add("textPrompt", new Builder.Dialogs.TextPrompt());
        }
    }

    public class DialogContainerBot : IBot
    {
        private DialogSet _dialogs;

        public DialogContainerBot()
        {
            _dialogs = new DialogSet();

            _dialogs.Add("getProfile", new ProfileControl());
            _dialogs.Add("firstRun",
                new WaterfallStep[]
                {
                    async (dc, args, next) =>
                    {
                         await dc.Context.SendActivity("Welcome! We need to ask a few questions to get started.");
                         await dc.Begin("getProfile");
                    },
                    async (dc, args, next) =>
                    {
                        await dc.Context.SendActivity($"Thanks {args["name"]} I have your phone number as {args["phone"]}!");
                        await dc.End();
                    }
                }
            );
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
                            await dc.Begin("firstRun");
                        }

                        break;

                    case ActivityTypes.ConversationUpdate:
                        foreach (var newMember in turnContext.Activity.MembersAdded)
                        {
                            if (newMember.Id != turnContext.Activity.Recipient.Id)
                            {
                                await turnContext.SendActivity("Hello and welcome to the Composite Control bot.");
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
    }
}
