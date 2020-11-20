// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Microsoft.Bot.Builder.Runtime.Builders.Transcripts;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Runtime.Builders.Middleware
{
    /// <summary>
    /// Defines an implementation of <see cref="IMiddlewareBuilder"/> that returns an instance
    /// of <see cref="TranscriptLoggerMiddleware"/>.
    /// </summary>
    [JsonObject]
    public class TranscriptLoggerMiddlewareBuilder : IMiddlewareBuilder
    {
        /// <summary>
        /// Class identifier.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "Microsoft.TranscriptLoggerMiddleware";

        /// <summary>
        /// Gets or sets the <see cref="ITranscriptLoggerBuilder"/> instance used to construct the underlying
        /// <see cref="ITranscriptLogger"/> instance for the middleware with.
        /// </summary>
        /// <value>
        /// The <see cref="ITranscriptLoggerBuilder"/> instance used to construct the underlying
        /// <see cref="ITranscriptLogger"/> instance for the middleware with.
        /// </value>
        [JsonProperty("transcriptStore")]
        public ITranscriptLoggerBuilder TranscriptStore { get; set; }

        /// <summary>
        /// Builds an instance of type <see cref="TranscriptLoggerMiddleware"/>.
        /// </summary>
        /// <param name="services">
        /// Provider containing all services registered with the application's service collection.
        /// </param>
        /// <param name="configuration">Application configuration.</param>
        /// <returns>An instance of type <see cref="TranscriptLoggerMiddleware"/>.</returns>
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

            return new TranscriptLoggerMiddleware(
                transcriptLogger: this.TranscriptStore?.Build(services, configuration));
        }
    }
}
