// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Extensions.Logging;

namespace Microsoft.Bot.Builder.StreamingExtensions
{
    /// <summary>
    /// Maps various endpoint handlers for the registered bot into the request execution pipeline using the V4 protocol.
    /// </summary>
    public static class BotFrameworkApplicationBuilderExtensions
    {
        /// <summary>
        /// Maps various endpoint handlers for the registered bot into the request execution pipeline using the V4 protocol.
        /// Throws <see cref="ArgumentNullException"/> if application is null.
        /// </summary>
        /// <param name="applicationBuilder">The application builder that defines the bot's pipeline.<see cref="IApplicationBuilder"/>.</param>
        /// <param name="onTurnError">Optional function to perform on turn errors.</param>
        /// <param name="pipeName">The name of the named pipe to use when creating the server.</param>
        /// <param name="logger">The ILogger implementation this adapter should use.</param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        public static IApplicationBuilder UseBotFrameworkNamedPipe(this IApplicationBuilder applicationBuilder, Func<ITurnContext, Exception, Task> onTurnError, string pipeName = null, ILogger<BotFrameworkHttpAdapter> logger = null)
        {
            if (applicationBuilder == null)
            {
                throw new ArgumentNullException(nameof(applicationBuilder));
            }

            var bot = applicationBuilder.ApplicationServices.GetService(typeof(IBot)) as IBot;
            (applicationBuilder.ApplicationServices.GetService(typeof(IBotFrameworkHttpAdapter)) as DirectLineAdapter).CreateAdapterListeningOnNamedPipe(onTurnError, bot, pipeName, logger);

            return applicationBuilder;
        }
    }
}
