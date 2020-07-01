// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Integration.AspNet.WebApi
{
    /// <summary>
    /// Set the ARRAffinity into the turn state.
    /// </summary>
    /// <remarks>
    /// We have established a persistent connection so we neeed to pass the ARRAffinity for all requests that 
    /// come through this connection to make it available upstream.
    /// </remarks>
    internal class BotAffinity : IBot
    {
        private string _arrAffinity;
        private IBot _bot;

        public BotAffinity(string arrAffinity, IBot bot)
        {
            _arrAffinity = arrAffinity;
            _bot = bot;
        }

        public Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default)
        {
            turnContext.TurnState.Set("ARRAffinity", _arrAffinity);
            return _bot.OnTurnAsync(turnContext, cancellationToken);
        }
    }
}
