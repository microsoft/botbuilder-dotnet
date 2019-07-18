// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

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
        /// <param name="middlewareSet">The set of middleware the bot executes on each turn. <see cref="MiddlewareSet"/>.</param>
        /// <param name="onTurnError">Callback to execute when an error occurs while executing the pipeline.</param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        public static IApplicationBuilder UseBotFrameworkNamedPipe(this IApplicationBuilder applicationBuilder, IList<IMiddleware> middlewareSet = null, Func<ITurnContext, Exception, Task> onTurnError = null)
        {
            if (applicationBuilder == null)
            {
                throw new ArgumentNullException(nameof(applicationBuilder));
            }

            var connector = new NamedPipeConnector();
            connector.InitializeNamedPipeServer(applicationBuilder.ApplicationServices, middlewareSet, onTurnError);

            return applicationBuilder;
        }
    }
}
