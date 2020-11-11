// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using AdaptiveExpressions.Properties;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Runtime.Extensions;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Runtime.Builders.Middleware
{
    [JsonObject]
    public class ShowTypingMiddlewareBuilder : IMiddlewareBuilder
    {
        [JsonProperty("$kind")]
        public const string Kind = "Microsoft.ShowTypingMiddleware";

        private const int DefaultDelay = 500;
        private const int DefaultPeriod = 2000;

        [JsonProperty("delay")]
        public IntExpression Delay { get; set; }

        [JsonProperty("period")]
        public IntExpression Period { get; set; }

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

            return new ShowTypingMiddleware(
                delay: this.Delay?.GetConfigurationValue(configuration) ?? DefaultDelay,
                period: this.Period?.GetConfigurationValue(configuration) ?? DefaultPeriod);
        }
    }
}
