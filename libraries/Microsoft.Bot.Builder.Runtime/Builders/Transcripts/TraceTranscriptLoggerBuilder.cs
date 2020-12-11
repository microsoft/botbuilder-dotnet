// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using AdaptiveExpressions.Properties;
using Microsoft.Bot.Builder.Runtime.Extensions;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Runtime.Builders.Transcripts
{
    /// <summary>
    /// Defines an implementation of <see cref="ITranscriptLoggerBuilder"/> that returns an instance
    /// of <see cref="TraceTranscriptLogger"/>.
    /// </summary>
    [JsonObject]
    internal class TraceTranscriptLoggerBuilder : ITranscriptLoggerBuilder
    {
        /// <summary>
        /// Class identifier.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "Microsoft.TraceTranscriptLogger";

        /// <summary>
        /// Gets or sets whether to log trace information. Defaults to true.
        /// </summary>
        /// <value>
        /// Indicates whether to log trace information. Defaults to true.
        /// </value>
        [JsonProperty("traceActivity")]
        public BoolExpression TraceActivity { get; set; }

        /// <summary>
        /// Builds an instance of type <see cref="TraceTranscriptLogger"/>.
        /// </summary>
        /// <param name="services">
        /// Provider containing all services registered with the application's service collection.
        /// </param>
        /// <param name="configuration">Application configuration.</param>
        /// <returns>An instance of type <see cref="TraceTranscriptLogger"/>.</returns>
        public ITranscriptLogger Build(IServiceProvider services, IConfiguration configuration)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            return new TraceTranscriptLogger(
                traceActivity: this.TraceActivity?.GetConfigurationValue(configuration) ?? true);
        }
    }
}
