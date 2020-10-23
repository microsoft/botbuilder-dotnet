// Copyright(c) Microsoft Corporation.All rights reserved.
// Licensed under the MIT License.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Skills;
using Microsoft.Bot.Schema;
using Moq;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Testing
{
    /// <summary>
    /// Middleware to add a mocked <see cref="SkillConversationIdFactoryBase"/> to the <see cref="ITurnContext.TurnState"/>.
    /// </summary>
    public class SetSkillConversationIdFactoryBaseMiddleware : IMiddleware
    {
        /// <inheritdoc/>
        public async Task OnTurnAsync(ITurnContext turnContext, NextDelegate next, CancellationToken cancellationToken = default)
        {
            if (turnContext.Activity.Type == ActivityTypes.Message)
            {
                var mockSkillConversationIdFactoryBase = new Mock<SkillConversationIdFactoryBase>();
                mockSkillConversationIdFactoryBase.SetupAllProperties();
                turnContext.TurnState.Add(mockSkillConversationIdFactoryBase.Object);
            }

            await next(cancellationToken).ConfigureAwait(false);
        }
    }
}
