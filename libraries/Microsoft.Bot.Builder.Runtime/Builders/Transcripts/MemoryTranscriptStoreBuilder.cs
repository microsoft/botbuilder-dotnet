// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Runtime.Builders.Transcripts
{
    /// <summary>
    /// Defines an implementation of <see cref="ITranscriptLoggerBuilder"/> that returns an instance
    /// of <see cref="MemoryTranscriptStore"/>.
    /// </summary>
    [JsonObject]
    public class MemoryTranscriptStoreBuilder : ITranscriptLoggerBuilder
    {
        /// <summary>
        /// Class identifier.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "Microsoft.MemoryTranscriptStore";

        /// <summary>
        /// Builds an instance of type <see cref="MemoryTranscriptStore"/>.
        /// </summary>
        /// <param name="services">
        /// Provider containing all services registered with the application's service collection.
        /// </param>
        /// <param name="configuration">Application configuration.</param>
        /// <returns>An instance of type <see cref="MemoryTranscriptStore"/>.</returns>
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

            return new MemoryTranscriptStore();
        }
    }
}
