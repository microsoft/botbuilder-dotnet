// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;

namespace AspNetCore_EchoBot_With_AppInsights
{
    public class MyAppInsightsBot : IBot
    {
        private readonly MyAppInsightsBotAccessors _stateAccessors;

        public MyAppInsightsBot(MyAppInsightsBotAccessors accessors)
        {
            _stateAccessors = accessors ?? throw new ArgumentNullException(nameof(accessors));
        }
        /// <summary>
        /// Every Conversation turn for our EchoBot will call this method. In here
        /// the bot checks the Activty type to verify it's a message, bumps the 
        /// turn conversation 'Turn' count, and then echoes the users typing
        /// back to them. 
        /// </summary>
        /// <param name="context">Turn scoped context containing all the data needed
        /// for processing this conversation turn. </param>        
        public async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            // This bot is only handling Messages
            if (turnContext.Activity.Type == ActivityTypes.Message)
            {
                // Get the conversation state from the turn context that holds a simple counter
                var state = await _stateAccessors.CounterState.GetAsync(turnContext, () => new CounterState());

                // Bump the turn count. 
                state.TurnCount++;

                // Echo back to the user whatever they typed.
                await turnContext.SendActivityAsync($"Turn {state.TurnCount}: You sent '{turnContext.Activity.Text}'");
            }
        }
    }
}
