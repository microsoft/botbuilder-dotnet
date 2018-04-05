// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Core.Extensions;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Prompts;
using Microsoft.Bot.Schema;
using Microsoft.Recognizers.Text;

namespace Microsoft.Bot.Samples.Dialog.Prompts
{
    public class PromptBot : IBot
    {
        private DialogSet _dialogs;

        public PromptBot()
        {
            _dialogs = new DialogSet();
            _dialogs.Add("number", new Builder.Dialogs.Prompts.NumberPrompt<int>(Culture.English));
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
                                var numberResult = (NumberResult<int>)dialogResult.Result;
                                await turnContext.SendActivity($"Bot received the number '{numberResult.Value}'.");
                            }
                            else
                            {
                                await dc.Prompt("number", "Enter a number.");
                            }
                        }

                        break;

                    case ActivityTypes.ConversationUpdate:
                        foreach (var newMember in turnContext.Activity.MembersAdded)
                        {
                            if (newMember.Id != turnContext.Activity.Recipient.Id)
                            {
                                await turnContext.SendActivity("Hello and welcome to the prompt bot.");
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
