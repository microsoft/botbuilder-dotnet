// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Core.Extensions;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Prompts;
using Microsoft.Bot.Schema;
using Microsoft.Recognizers.Text;

namespace Microsoft.Bot.Samples.Dialog.Prompts
{
    public class AppBot : IBot
    {
        private DialogSet _dialogs;

        public AppBot()
        {
            _dialogs = new DialogSet();

            // Add prompts
            //_dialogs.Add("choicePrompt", new ChoicePrompt());
            _dialogs.Add("confirmPrompt", new ConfirmPrompt(Culture.English));
            //_dialogs.Add("datetimePrompt", new DatetimePrompt());
            _dialogs.Add("numberPrompt", new NumberPrompt<float>(Culture.English));
            _dialogs.Add("textPrompt", new TextPrompt());
            //_dialogs.Add("attachmentPrompt", new AttachmentPrompt());

            _dialogs.Add("mainMenu", new WaterfallStep[] {
                delegate (DialogContext dc, object args, SkipStepFunction next)
                {
                    return Task.FromResult("test1");
                },
                delegate (DialogContext dc, object args, SkipStepFunction next)
                {
                    return Task.FromResult("test2");
                }
            });
        }

        public async Task OnReceiveActivity(ITurnContext context)
        {
            try
            {
                switch (context.Activity.Type)
                {
                    case ActivityTypes.Message:

                        var state = ConversationState<ConversationData>.Get(context);

                        var dc = _dialogs.CreateContext(context, state);

                        var utterance = (context.Activity.Text ?? string.Empty).Trim().ToLower();
                        if (utterance == "menu" || utterance == "cancel")
                        {
                            dc.EndAll();
                        }

                        await dc.Continue();

                        if (!context.Responded)
                        {
                            await dc.Begin("mainMenu");
                        }

                        break;

                    case ActivityTypes.ConversationUpdate:
                        foreach (var newMember in context.Activity.MembersAdded)
                        {
                            if (newMember.Id != context.Activity.Recipient.Id)
                            {
                                await context.SendActivity("Hello and welcome to the prompt bot.");
                            }
                        }
                        break;
                }
            }
            catch (Exception e)
            {
                await context.SendActivity($"Exception: {e.Message}");
            }
        }
    }
}
