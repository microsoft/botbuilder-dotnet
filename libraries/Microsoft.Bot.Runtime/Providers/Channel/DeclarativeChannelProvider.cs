// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using AdaptiveExpressions.Properties;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Runtime.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using IChannel = Microsoft.Bot.Connector.Authentication.IChannelProvider;

namespace Microsoft.Bot.Runtime.Providers.Channel
{
    [JsonObject]
    public class DeclarativeChannelProvider : IChannelProvider
    {
        [JsonProperty("$kind")]
        public const string Kind = "Microsoft.DeclarativeChannelProvider";

        [JsonProperty("channelService")]
        public StringExpression ChannelService { get; set; }

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

            services.AddSingleton<IChannel>(_ => new SimpleChannelProvider(
                this.ChannelService?.GetConfigurationValue(configuration)));
        }
    }
}
