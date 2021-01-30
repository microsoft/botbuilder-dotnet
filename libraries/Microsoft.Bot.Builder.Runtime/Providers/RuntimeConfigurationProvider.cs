// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Runtime.Loader;
using AdaptiveExpressions.Properties;
using Microsoft.Bot.Builder.Integration.AspNet.Core.Skills;
using Microsoft.Bot.Builder.Runtime.Extensions;
using Microsoft.Bot.Builder.Runtime.Plugins;
using Microsoft.Bot.Builder.Runtime.Providers.Adapter;
using Microsoft.Bot.Builder.Runtime.Providers.Storage;
using Microsoft.Bot.Builder.Runtime.Providers.Telemetry;
using Microsoft.Bot.Builder.Runtime.Settings;
using Microsoft.Bot.Builder.Skills;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Runtime.Providers
{
    /// <summary>
    /// Defines an implementation of <see cref="IProvider"/> that orchestrates the registration of standard
    /// services using various <see cref="IProvider"/> objects as derived from the runtime definition.
    /// </summary>
    [JsonObject]
    internal class RuntimeConfigurationProvider : IProvider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RuntimeConfigurationProvider"/> class.
        /// </summary>
        [JsonConstructor]
        public RuntimeConfigurationProvider()
        {
        }

        /// <summary>
        /// Gets or sets the <see cref="IAdapterProvider"/> instances to utilize for configuring
        /// adapter-related services.
        /// </summary>
        /// <value>
        /// The <see cref="IAdapterProvider"/> to utilize for configuring adapter-related services.
        /// </value>
        [JsonProperty("adapter")]
        public IAdapterProvider Adapter { get; set; }

        /// <summary>
        /// Gets or sets the default locale to utilize. Defaults to 'en-US'.
        /// </summary>
        /// <value>
        /// The default locale to utilize. Defaults to 'en-US'.
        /// </value>
        [JsonProperty("defaultLocale")]
        public string DefaultLocale { get; set; }

        /// <summary>
        /// Gets or sets the resource identifier of the dialog to serve as the root dialog of the bot.
        /// </summary>
        /// <value>
        /// The resource identifier of the dialog to serve as the root dialog of the bot.
        /// </value>
        [JsonProperty("rootDialog")]
        public StringExpression RootDialog { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="IStorageProvider"/> to utilize for configuring storage-related services.
        /// </summary>
        /// <value>
        /// The <see cref="IStorageProvider"/> to utilize for configuring storage-related services.
        /// </value>
        [JsonProperty("storage")]
        public IStorageProvider Storage { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="ITelemetryProvider"/> to utilize for configuring telemetry-related services.
        /// </summary>
        /// <value>
        /// The <see cref="ITelemetryProvider"/> to utilize for configuring telemetry-related services.
        /// </value>
        [JsonProperty("telemetry")]
        public ITelemetryProvider Telemetry { get; set; }

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

            var providers = new List<IProvider>(new IProvider[]
            {
                this.Adapter,
                this.Storage,
                this.Telemetry
            });

            foreach (IProvider provider in providers)
            {
                provider?.ConfigureServices(services, configuration);
            }

            ConfigureCoreBotServices(services, configuration);
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
