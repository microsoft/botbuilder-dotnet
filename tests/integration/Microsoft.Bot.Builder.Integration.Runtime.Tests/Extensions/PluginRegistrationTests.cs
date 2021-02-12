// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Builder.Integration.Runtime.Extensions;
using Microsoft.Bot.Builder.Integration.Runtime.Plugins;
using Microsoft.Bot.Builder.Integration.Runtime.Settings;
using Microsoft.Bot.Builder.Runtime.Tests.Plugins.TestComponents;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.Bot.Builder.Runtime.Tests.Extensions
{
    public class PluginRegistrationTests
    {
        [Fact]
        public void AddComponentPlugin()
        {
            // Setup
            const string contosoSecret = "shh123";
            const string adventureWorksSecret = "superSecret!@!";

            var assemblyName = typeof(PluginRegistrationTests).Assembly.GetName().Name;

            var settings = new Dictionary<string, string>
                {
                    { $"{assemblyName}:contosoSecret", contosoSecret },
                    { $"{assemblyName}:AdventureWorksSkillId", "myCoolSkill" },
                    { $"{assemblyName}:AdventureWorksSecret", adventureWorksSecret },
                };

            IServiceCollection services = new ServiceCollection();
            IConfiguration configuration = new ConfigurationBuilder().AddInMemoryCollection(settings).Build();

            services.AddSingleton<IConfiguration>(configuration);

            // Full component settings
            var runtimeSettings = new RuntimeSettings()
            {
                // Declare plugin
                Plugins = new[]
                {
                    new BotPluginDefinition()
                    {
                        Name = assemblyName,
                        SettingsPrefix = assemblyName
                    } 
                },

                // Adapters
                Adapters = new[]
                {
                    new AdapterSettings() { Name = assemblyName, Route = "contoso", Enabled = true },
                    new AdapterSettings() { Name = assemblyName, Route = "adventureworks", Enabled = true },
                }
            };

            // Test
            services.AddBotRuntimePlugins(configuration, runtimeSettings);

            // Assert 
            var provider = services.BuildServiceProvider();

            // Assert adapters
            var httpAdapters = provider.GetServices<IBotFrameworkHttpAdapter>();
            Assert.Contains(httpAdapters, a => a.GetType().Equals(typeof(ContosoAdapter)));
            Assert.Contains(httpAdapters, a => a.GetType().Equals(typeof(AdventureWorksAdapter)));
        }
    }
}
