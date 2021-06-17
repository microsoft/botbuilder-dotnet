// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Runtime.Component
{
    /// <summary>
    /// Represents the definition of a plugin that can be loaded into the bot runtime.
    /// </summary>
    [JsonObject]
    internal class BotComponentDefinition
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
        /// <param name="logger"><see cref="ILogger"/> instance.</param>
        public void Load(
            IBotComponentEnumerator pluginEnumerator,
            IServiceCollection services,
            IConfiguration configuration,
            ILogger logger)
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

            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            var components = new List<BotComponent>(pluginEnumerator.GetComponents(this.Name));

            if (components.Count > 0)
            {
                foreach (BotComponent component in pluginEnumerator.GetComponents(this.Name))
                {
                    var componentServices = new ServiceCollection();

                    component.ConfigureServices(componentServices, GetPluginConfiguration(configuration));

                    foreach (var serviceDescriptor in componentServices)
                    {
                        services.Add(serviceDescriptor);
                    }
                }
            }
            else
            {
                logger.LogWarning(
                    $"{this.Name} does not contain any discoverable implementations of {typeof(BotComponent).FullName}. " +
                    "Consider removing this component from the list of components in your application settings.");
            }
        }

        private IConfiguration GetPluginConfiguration(IConfiguration configuration)
        {
            if (string.IsNullOrEmpty(this.SettingsPrefix))
            {
                return EmptyConfiguration;
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
