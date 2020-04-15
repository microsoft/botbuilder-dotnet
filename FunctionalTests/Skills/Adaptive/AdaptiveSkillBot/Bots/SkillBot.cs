// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;

namespace Microsoft.BotBuilderSamples.AdaptiveSkillBot.Bots
{
    public class SkillBot<T> : IBot
        where T : Dialog
    {
        private readonly ConversationState _conversationState;
        private readonly DialogManager _dialogManager;

        public SkillBot(ConversationState conversationState, T mainDialog)
        {
            _conversationState = conversationState;
            _dialogManager = new DialogManager(mainDialog);
        }

        public async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default)
        {
            await _dialogManager.OnTurnAsync(turnContext, cancellationToken);

            // Save any state changes that might have occured during the turn.
            await _conversationState.SaveChangesAsync(turnContext, false, cancellationToken);
        }
    }
}
