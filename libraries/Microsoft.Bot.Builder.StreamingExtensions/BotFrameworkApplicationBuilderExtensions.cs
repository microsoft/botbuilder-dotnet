// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;

namespace Microsoft.Bot.Builder.StreamingExtensions
{
    /// <summary>
    /// Maps various endpoint handlers for the registered bot into the request execution pipeline using the V4 protocol.
    /// </summary>
    public static class BotFrameworkApplicationBuilderExtensions
    {
        /// <summary>
        /// Maps various endpoint handlers for the registered bot into the request execution pipeline using the V4 protocol.
        /// </summary>
        /// <param name="applicationBuilder">The application builder that defines the bot's pipeline.<see cref="IApplicationBuilder"/>.</param>
        /// <param name="pipeName">The name of the named pipe to use when creating the server.</param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        public static IApplicationBuilder UseBotFrameworkNamedPipe(this IApplicationBuilder applicationBuilder, string pipeName = "bfv4.pipes")
        {
            if (applicationBuilder == null)
            {
                throw new ArgumentNullException(nameof(applicationBuilder));
            }

            var bot = applicationBuilder.ApplicationServices.GetService(typeof(IBot)) as IBot;
            _ = (applicationBuilder.ApplicationServices.GetService(typeof(IBotFrameworkHttpAdapter)) as DirectLineAdapter).AddNamedPipeConnection(pipeName, bot);

            return applicationBuilder;
        }
    }
}
