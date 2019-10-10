// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Microsoft.Bot.Builder.Integration;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Builder.Integration.AspNet.Core.Skills;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DialogRootBot
{
    public class AdapterWithErrorHandler : BotFrameworkHttpAdapter
    {
        public AdapterWithErrorHandler(IConfiguration configuration, ILogger<BotFrameworkHttpAdapter> logger)
            : base(configuration, logger)
        {
            // TODO: Gabo, think if this should be moved somewhere else.
            var section = configuration.GetSection($"Skills");
            var skills = section?.Get<SkillOptions[]>();
            if (skills != null)
            {
                this.UseSkills(new Uri(configuration["SkillsCallbackEndpoint"]), skills);
            }

            OnTurnError = async (turnContext, exception) =>
            {
                // Log any leaked exception from the application.
                logger.LogError($"Exception caught : {exception.Message}");

                // Send a catch-all apology to the user.
                await turnContext.SendActivityAsync($"Sorry, it looks like something went wrong. \r\n{exception}");
            };
        }
    }
}
