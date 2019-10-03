// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Builder.StreamingExtensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace SimpleChildBot
{
    public class AdapterWithErrorHandler : BotFrameworkHttpAdapter
    {
        public AdapterWithErrorHandler(IConfiguration configuration, ILogger<BotFrameworkHttpAdapter> logger)
            : base(null, null, null)
        {
            OnTurnError = async (turnContext, exception) =>
            {
                // Log any leaked exception from the application.
                logger.LogError($"Exception caught : {exception.Message}");

                // Send a catch-all apology to the user.
                await turnContext.SendActivityAsync($"Skill Error, it looks like something went wrong.\r\n{exception}");
            };
        }
    }
}
