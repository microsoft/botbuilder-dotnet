// Copyright(c) Microsoft Corporation.All rights reserved.
// Licensed under the MIT License.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Testing.Mocks;
using Microsoft.Bot.Builder.Skills;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Testing
{
    /// <summary>
    /// Middleware to add <see cref="MockSkillBotFrameworkClient"/> to the <see cref="ITurnContext.TurnState"/>.
    /// </summary>
    public class SetSkillBotFrameworkClientMiddleware : IMiddleware
    {
        /// <inheritdoc/>
        public async Task OnTurnAsync(ITurnContext turnContext, NextDelegate next, CancellationToken cancellationToken = default)
        {
            if (turnContext.Activity.Type == ActivityTypes.Message)
            {
                turnContext.TurnState.Add<BotFrameworkClient>(new MockSkillBotFrameworkClient());
            }

            await next(cancellationToken).ConfigureAwait(false);
        }
    }
}
