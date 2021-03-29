// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Microsoft.Bot.Builder.Integration.Runtime.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Integration.Runtime.Component
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
        public void Load(
            IBotComponentEnumerator pluginEnumerator,
            IServiceCollection services,
            IConfiguration configuration)
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
