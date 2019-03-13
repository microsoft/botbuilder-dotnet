// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.TestBot.WebApi
{
    /// <summary>
    /// TestBot class.
    /// </summary>
    public class TestBot : IBot
    {
        /// <summary>
        /// OnTurnAsync class.
        /// </summary>
        /// <param name="turnContext">ITurnContext.</param>
        /// <param name="cancellationToken">CancellationToken.</param>
        /// <returns>Task.</returns>
        public async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (turnContext.Activity.Type == ActivityTypes.Message)
            {
                await turnContext.SendActivityAsync(MessageFactory.Text("hi"));
            }
        }
    }
}
