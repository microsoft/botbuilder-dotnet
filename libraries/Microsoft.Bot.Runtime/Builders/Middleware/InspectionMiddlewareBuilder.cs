// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Microsoft.Bot.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace Microsoft.Bot.Runtime.Builders.Middleware
{
    [JsonObject]
    public class InspectionMiddlewareBuilder : IMiddlewareBuilder
    {
        [JsonProperty("$kind")]
        public const string Kind = "Microsoft.InspectionMiddleware";

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

            var storage = services.GetService<IStorage>();

            return new InspectionMiddleware(new InspectionState(storage));
        }
    }
}
