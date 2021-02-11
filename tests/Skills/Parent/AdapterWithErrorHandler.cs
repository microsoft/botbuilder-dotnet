// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading.Tasks;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Microsoft.BotBuilderSamples
{
    public class AdapterWithErrorHandler : BotFrameworkHttpAdapter
    {
        public AdapterWithErrorHandler(IConfiguration configuration, ILogger<BotFrameworkHttpAdapter> logger)
            : base(configuration, logger)
        {
            OnTurnError = (turnContext, exception) =>
            {
                // Log any leaked exception from the application.
                logger.LogError($"Exception caught : {exception.Message}");
                return Task.CompletedTask;
            };
        }
    }
}
