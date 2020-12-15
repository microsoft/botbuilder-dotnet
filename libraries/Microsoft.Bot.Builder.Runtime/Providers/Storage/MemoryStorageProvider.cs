// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Runtime.Providers.Storage
{
    /// <summary>
    /// Defines an implementation of <see cref="IStorageProvider"/> that registers
    /// <see cref="MemoryStorage"/> with the application's service collection.
    /// </summary>
    [JsonObject]
    internal class MemoryStorageProvider : IStorageProvider
    {
        /// <summary>
        /// Class identifier.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "Microsoft.MemoryStorage";

        /// <summary>
        /// Gets or sets a <see cref="JObject"/> representing a dictionary of nested <see cref="JObject"/>
        /// values to pre-load storage with.
        /// </summary>
        /// <value>
        /// A <see cref="JObject"/> representing a dictionary of nested <see cref="JObject"/> values to
        /// pre-load storage with.
        /// </value>
        [JsonProperty("content")]
#pragma warning disable CA2227 // Collection properties should be read only
        public JObject Content { get; set; }
#pragma warning restore CA2227 // Collection properties should be read only

        /// <summary>
        /// Register services with the application's service collection.
        /// </summary>
        /// <param name="services">The application's collection of registered services.</param>
        /// <param name="configuration">Application configuration.</param>
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            var dictionary = new Dictionary<string, JObject>();

            foreach (JProperty property in this.Content?.Properties() ?? Array.Empty<JProperty>())
            {
                if (property.Type == JTokenType.Object)
                {
                    dictionary[property.Name] = (JObject)property.Value;
                }
            }

            services.AddSingleton<IStorage>(_ => new MemoryStorage(dictionary));
        }
    }
}
