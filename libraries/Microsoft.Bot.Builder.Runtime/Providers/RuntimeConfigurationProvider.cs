// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using AdaptiveExpressions.Properties;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core.Skills;
using Microsoft.Bot.Builder.Runtime.Extensions;
using Microsoft.Bot.Builder.Runtime.Providers.Adapter;
using Microsoft.Bot.Builder.Runtime.Providers.Storage;
using Microsoft.Bot.Builder.Runtime.Providers.Telemetry;
using Microsoft.Bot.Builder.Runtime.Settings;
using Microsoft.Bot.Builder.Skills;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using IChannelProvider = Microsoft.Bot.Builder.Runtime.Providers.Channel.IChannelProvider;
using ICredentialProvider = Microsoft.Bot.Builder.Runtime.Providers.Credentials.ICredentialProvider;

namespace Microsoft.Bot.Builder.Runtime.Providers
{
    [JsonObject]
    public class RuntimeConfigurationProvider : IProvider
    {
        [JsonProperty("adapters")]
        public IList<IAdapterProvider> Adapters { get; } = new List<IAdapterProvider>();

        [JsonProperty("channel")]
        public IChannelProvider Channel { get; set; }

        [JsonProperty("credentials")]
        public ICredentialProvider Credentials { get; set; }

        [JsonProperty("defaultLocale")]
        public string DefaultLocale { get; set; }

        [JsonProperty("rootDialog")]
        public StringExpression RootDialog { get; set; }

        [JsonProperty("storage")]
        public IStorageProvider Storage { get; set; }

        [JsonProperty("telemetry")]
        public ITelemetryProvider Telemetry { get; set; }

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

            var providers = new List<IProvider>(this.Adapters);
            providers.AddRange(new IProvider[]
            {
                this.Channel,
                this.Credentials,
                this.Storage,
                this.Telemetry
            });

            foreach (IProvider provider in providers)
            {
                provider?.ConfigureServices(services, configuration);
            }

            ConfigureSkillServices(services);
            ConfigureBotStateServices(services);
            ConfigureAuthenticationConfigurationServices(services);
            ConfigureCoreBotServices(services, configuration);
        }

        private static void ConfigureAuthenticationConfigurationServices(IServiceCollection services)
        {
            services.AddSingleton<AuthenticationConfiguration>();
        }

        private static void ConfigureBotStateServices(IServiceCollection services)
        {
            services.AddSingleton<UserState>();
            services.AddSingleton<ConversationState>();
        }

        private static void ConfigureSkillServices(IServiceCollection services)
        {
            services.AddSingleton<SkillConversationIdFactoryBase, SkillConversationIdFactory>();
            services.AddHttpClient<BotFrameworkClient, SkillHttpClient>();
            services.AddSingleton<ChannelServiceHandler, SkillHandler>();
        }

        private void ConfigureCoreBotServices(IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<CoreBotOptions>(o =>
            {
                o.DefaultLocale = this.DefaultLocale;
                o.RootDialog = this.RootDialog?.GetConfigurationValue(configuration);
            });

            services.AddSingleton<IBot, CoreBot>();
        }
    }
}
