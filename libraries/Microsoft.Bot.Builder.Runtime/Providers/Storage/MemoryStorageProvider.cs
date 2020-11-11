// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using Microsoft.Bot.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Runtime.Providers.Storage
{
    [JsonObject]
    public class MemoryStorageProvider : IStorageProvider
    {
        [JsonProperty("$kind")]
        public const string Kind = "Microsoft.MemoryStorage";

        [JsonProperty("content")]
#pragma warning disable CA2227 // Collection properties should be read only
        public JObject Content { get; set; }
#pragma warning restore CA2227 // Collection properties should be read only

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
