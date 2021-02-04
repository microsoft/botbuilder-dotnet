// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Microsoft.Bot.Builder.Azure;
using Microsoft.Bot.Builder.Azure.Blobs;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Builder.Integration.AspNet.Core.Skills;
using Microsoft.Bot.Builder.Runtime.Extensions;
using Microsoft.Bot.Builder.Runtime.Plugins;
using Microsoft.Bot.Builder.Runtime.Settings;
using Microsoft.Bot.Builder.Runtime.Skills;
using Microsoft.Bot.Builder.Runtime.Tests.Plugins.TestComponents;
using Microsoft.Bot.Builder.Skills;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
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

            var settings = new Dictionary<string, string>
                {
                    { $"{typeof(ContosoAdapter).FullName}:contosoSecret", contosoSecret },
                    { $"{typeof(AdventureWorksAdapter).FullName}:AdventureWorksSkillId", "myCoolSkill" },
                    { $"{typeof(AdventureWorksAdapter).FullName}:AdventureWorksSecret", adventureWorksSecret },
                };

            IServiceCollection services = new ServiceCollection();
            IConfiguration configuration = new ConfigurationBuilder().AddInMemoryCollection(settings).Build();

            // Full component settings
            var runtimeSettings = new RuntimeSettings()
            {
                // Declare plugin
                Plugins = new[]
                {
                    new BotPluginDefinition()
                    {
                        Name = typeof(PluginRegistrationTests).Assembly.GetName().Name,
                    } 
                },

                // Adapters
                Resources = new ResourcesSettings()
                {
                    Adapters = new[] 
                    { 
                        new AdapterSettings() { Name = typeof(ContosoAdapter).FullName, Route = "contoso", Enabled = true },
                        new AdapterSettings() { Name = typeof(AdventureWorksAdapter).FullName, Route = "adventureworks", Enabled = true },
                    }
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

            var botAdapters = provider.GetServices<BotAdapter>();
            Assert.Contains(botAdapters, a => a.GetType().Equals(typeof(ContosoAdapter)));
            Assert.Contains(botAdapters, a => a.GetType().Equals(typeof(AdventureWorksAdapter)));

            // Assert adapter settings
            var adapterSettings = provider.GetServices<AdapterSettings>();
            Assert.Contains(adapterSettings, s => s.Name == typeof(ContosoAdapter).FullName);
            Assert.Contains(adapterSettings, s => s.Name == typeof(AdventureWorksAdapter).FullName);
        }
    }
}
