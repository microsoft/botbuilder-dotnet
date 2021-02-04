﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Microsoft.Bot.Builder.Runtime.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Runtime.Plugins
{
    /// <summary>
    /// Represents the definition of a plugin that can be loaded into the bot runtime.
    /// </summary>
    [JsonObject]
    internal class BotPluginDefinition
    {
        /// <summary>
        /// Gets or sets the name of the assembly containing the plugin entry point.
        /// </summary>
        /// <value>
        /// The name of the assembly containing the plugin entry point.
        /// </value>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the prefix of application settings that should be supplied to the plugin.
        /// </summary>
        /// <value>
        /// The prefix of the application settings that should be supplied to the plugin.
        /// </value>
        [JsonProperty("settingsPrefix")]
        public string SettingsPrefix { get; set; }

        /// <summary>
        /// Gets an empty <see cref="IConfiguration"/> instance.
        /// </summary>
        [JsonIgnore]
        private static IConfiguration EmptyConfiguration => new ConfigurationBuilder().Build();

        /// <summary>
        /// Loads the plugin based on the definition information into the runtime.
        /// </summary>
        /// <param name="pluginEnumerator">Enumerates available plugins from the definition information.</param>
        /// <param name="services">The application's collection of registered services.</param>
        /// <param name="configuration">Application configuration.</param>
        /// <param name="serviceFilter">Optional filter to decide whether a specific <see cref="ServiceDescriptor"/> should be consumed or discarded. 
        /// This is relevant for optional security checks and features such as configurable adapters.</param>
        public void Load(
            IBotPluginEnumerator pluginEnumerator,
            IServiceCollection services,
            IConfiguration configuration,
            Func<ServiceDescriptor, bool> serviceFilter = null)
        {
            if (pluginEnumerator == null)
            {
                throw new ArgumentNullException(nameof(pluginEnumerator));
            }

            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            IConfiguration pluginConfiguration = this.GetPluginConfiguration(configuration);

            foreach (IBotPlugin plugin in pluginEnumerator.GetPlugins(this.Name))
            {
                var context = new BotPluginLoadContext(pluginConfiguration);
                plugin.Load(context);

                foreach (var service in context.Services)
                {
                    if (serviceFilter == null || serviceFilter(service))
                    {
                        services.Add(service);
                    }
                }
            }
        }

        private IConfiguration GetPluginConfiguration(IConfiguration configuration)
        {
            if (string.IsNullOrEmpty(this.SettingsPrefix))
            {
                return configuration;
            }

            IConfigurationSection section = configuration.GetSection(this.SettingsPrefix);

            if (!section.Exists())
            {
                return EmptyConfiguration;
            }

            return section;
        }
    }
}
