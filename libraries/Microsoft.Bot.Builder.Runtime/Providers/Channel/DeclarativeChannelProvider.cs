// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using AdaptiveExpressions.Properties;
using Microsoft.Bot.Builder.Runtime.Extensions;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using IChannel = Microsoft.Bot.Connector.Authentication.IChannelProvider;

namespace Microsoft.Bot.Builder.Runtime.Providers.Channel
{
    /// <summary>
    /// Defines an interface for an implementation of <see cref="IChannelProvider"/> that registers
    /// <see cref="SimpleChannelProvider"/> with the application's service collection.
    /// </summary>
    [JsonObject]
    public class DeclarativeChannelProvider : IChannelProvider
    {
        /// <summary>
        /// Class identifier.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "Microsoft.DeclarativeChannelProvider";

        /// <summary>
        /// Gets or sets the host URI used to communicate with the channel service.
        /// Defaults to 'https://api.botframework.com'.
        /// </summary>
        /// <value>
        /// The host URI used to communicate with the channel service. Defaults to 'https://api.botframework.com'.
        /// </value>
        [JsonProperty("channelService")]
        public StringExpression ChannelService { get; set; }

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

            services.AddSingleton<IChannel>(_ => new SimpleChannelProvider(
                this.ChannelService?.GetConfigurationValue(configuration)));
        }
    }
}
