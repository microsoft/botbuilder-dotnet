// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Runtime.Builders.Middleware
{
    /// <summary>
    /// Defines an implementation of <see cref="IMiddlewareBuilder"/> that returns an instance
    /// of <see cref="RemoveRecipientMentionMiddlewareBuilder"/>.
    /// </summary>
    [JsonObject]
    public class RemoveRecipientMentionMiddlewareBuilder : IMiddleware, IMiddlewareBuilder
    {
        /// <summary>
        /// Class identifier.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "Microsoft.RemoveRecipientMentionMiddleware";

        /// <summary>
        /// Builds an instance of type <see cref="RemoveRecipientMentionMiddlewareBuilder"/>.
        /// </summary>
        /// <param name="services">
        /// Provider containing all services registered with the application's service collection.
        /// </param>
        /// <param name="configuration">Application configuration.</param>
        /// <returns>An instance of type <see cref="RemoveRecipientMentionMiddlewareBuilder"/>.</returns>
        public IMiddleware Build(IServiceProvider services, IConfiguration configuration)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            return this;
        }

        /// <summary>
        /// Processes an incoming activity.
        /// </summary>
        /// <param name="turnContext">The context object for this turn.</param>
        /// <param name="next">The delegate to call to continue the bot middleware pipeline.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <remarks>Spawns a thread that sends the periodic typing activities until the turn ends.
        /// </remarks>
        /// <seealso cref="ITurnContext"/>
        /// <seealso cref="Bot.Schema.IActivity"/>
        public async Task OnTurnAsync(ITurnContext turnContext, NextDelegate next, CancellationToken cancellationToken)
        {
            if (turnContext?.Activity?.Type == ActivityTypes.Message)
            {
                turnContext.Activity.RemoveRecipientMention();
            }

            await next(cancellationToken).ConfigureAwait(false);
        }
    }
}
