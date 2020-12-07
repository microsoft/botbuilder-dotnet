// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Runtime.Builders.Handlers
{
    /// <summary>
    /// When added, this configures an OnTurnError implementation for your bot adapter.
    /// You can toggle whether to log the exception and send a trace activity with the stack trace, as well
    /// as provide LG templates for the logged exception message and the bot response to the user.
    /// </summary>
    public class OnTurnErrorHandlerBuilder : IOnTurnErrorHandlerBuilder
    {
        [JsonProperty("$kind")]
        public const string Kind = "Microsoft.OnTurnErrorHandler";

        public Func<ITurnContext, Exception, Task> Build(IServiceProvider services, IConfiguration configuration)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            var logger = services.GetService<ILogger<IBotFrameworkHttpAdapter>>() ?? new NullLogger<IBotFrameworkHttpAdapter>();
            var conversationState = services.GetService<ConversationState>();

            return async (turnContext, exception) =>
            {
                // Log any leaked exception from the application
                logger.LogError(exception, exception.Message);

                // Send the exception message to the user. Since the default behavior does not
                // send logs or trace activities, the bot appears hanging without any activity
                // to the user.
                await turnContext.SendActivityAsync(exception.Message).ConfigureAwait(false);

                if (conversationState != null)
                {
                    try
                    {
                        // Delete the conversationState for the current conversation to prevent the
                        // bot from getting stuck in a error-loop caused by being in a bad state.
                        await conversationState.DeleteAsync(turnContext).ConfigureAwait(false);
                    }
#pragma warning disable CA1031 // Do not catch general exception types (we just log the exception and continue)
                    catch (Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types
                    {
                        logger.LogError(ex, $"Exception caught on attempting to Delete ConversationState : {ex.Message}");
                    }
                }
            };
        }
    }
}
