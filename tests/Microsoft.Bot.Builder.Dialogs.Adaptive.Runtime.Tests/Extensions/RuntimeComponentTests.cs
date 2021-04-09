// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Runtime;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Runtime.Component;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Runtime.Extensions;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Runtime.Settings;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Runtime.Tests.Components.TestComponents;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Microsoft.Bot.Builder.Dialogs.Memory;
using Microsoft.Bot.Builder.Dialogs.Memory.Scopes;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Builder.Runtime.Tests.Components.Implementations;
using Microsoft.Bot.Builder.Runtime.Tests.Components.TestComponents;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.Bot.Builder.Runtime.Tests.Extensions
{
    public class RuntimeComponentTests
    {
        [Fact]
        public void AddComponent_ConfiguredComponent()
        {
            // Setup
            const string contosoSecret = "shh123";
            const string adventureWorksSecret = "superSecret!@!";

            var assemblyName = typeof(RuntimeComponentTests).Assembly.GetName().Name;

            var settings = new Dictionary<string, string>
                {
                    { $"{assemblyName}:contosoSecret", contosoSecret },
                    { $"{assemblyName}:AdventureWorksSkillId", "myCoolSkill" },
                    { $"{assemblyName}:AdventureWorksSecret", adventureWorksSecret },
                };

            // Full component settings
            var runtimeSettings = new RuntimeSettings()
            {
                // Declare plugin
                Components = new[]
                {
                    new BotComponentDefinition()
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

            IServiceCollection services = new ServiceCollection();
            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(settings)
                .AddRuntimeSettings(runtimeSettings)
                .Build();

            services.AddSingleton<IConfiguration>(configuration);

            // Test
            services.AddBotRuntimeComponents(configuration);

            // Assert 
            var provider = services.BuildServiceProvider();

            // Assert adapters
            var httpAdapters = provider.GetServices<IBotFrameworkHttpAdapter>();
            Assert.Contains(httpAdapters, a => a.GetType().Equals(typeof(ContosoAdapter)));
            Assert.Contains(httpAdapters, a => a.GetType().Equals(typeof(AdventureWorksAdapter)));
        }

        [Fact]
        public void AddComponent_RegistersConverters_CustomActions()
        {
            // Arrange
            IServiceCollection services = new ServiceCollection();
            IConfiguration configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string>() { { "applicationRoot", "." } }).Build();

            services.AddSingleton<IConfiguration>(configuration);

            const string activityJson = "pirateActivity";

            // Test 

            // Register a declarative component
            new PirateBotComponent().ConfigureServices(services, configuration);

            services.AddBotRuntime(configuration);

            // Assert

            // Build service provider
            var serviceProvider = services.BuildServiceProvider();

            // Get ResourceExplorer
            var resourceExplorer = serviceProvider.GetRequiredService<ResourceExplorer>();

            // Load a SentActivityAsPirate dialog. For this to work, the following needs to be true:
            //      1- Runtime service collection extensions registered a resource explorer
            //      2- Pirate component added custom converter and custom action to service collection
            //      3- DI detected the pirate component converter and action and passed it to the ResourceExplorerOptions constructor
            //      4- DI passed the registered resource explorer options to the resource explorer
            //      5- Resource explorer processed the passed in registrations, and the converter was properly wired in for json serialization
            var declarativeType = resourceExplorer.LoadType<SendActivityAsPirate>(new SimpleMemoryResource($"\"{activityJson}\""));

            // Assert

            // Verify that we got a constructed SendActivityAsPirate
            Assert.NotNull(declarativeType);

            // Verify that the Data property of the SendActivityAsPirate came from the memory resource below.
            // This guarantees that the custom converter was called since it's the only code that is passing the json data to the
            // SendActivityAsPirate constructor.
            Assert.Equal(activityJson, declarativeType.Data);

            Assert.IsType<ConfigurationResourceExplorer>(resourceExplorer);
        }

        [Fact]
        public void AddComponent_RegistersMemoryScopes_PathResolvers()
        {
            // Arrange
            IServiceCollection services = new ServiceCollection();
            IConfiguration configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string>() { { "applicationRoot", "." } }).Build();

            services.AddSingleton<IConfiguration>(configuration);

            // Test 

            // Register a declarative component
            new PirateBotComponent().ConfigureServices(services, configuration);

            services.AddBotRuntime(configuration);

            // Assert

            // Build service provider
            var serviceProvider = services.BuildServiceProvider();

            // Assert

            // Verify that memory scopes and path resolvers were registered. This tests the contract of the runtime
            // to register these classes. The other end of the contract is using these classes in CoreBot / AdaptiveBot
            // and adding to TurnState for DialogStateManager to pick up.
            var memoryScopes = serviceProvider.GetServices<MemoryScope>();
            Assert.Contains(memoryScopes, s => s.GetType().Equals(typeof(TestMemoryScope)));

            var pathResolvers = serviceProvider.GetServices<IPathResolver>();
            Assert.Contains(pathResolvers, pr => pr.GetType().Equals(typeof(DoubleCaratPathResolver)));
        }

        internal class SimpleMemoryResource : Resource
        {
            private readonly string _json;

            public SimpleMemoryResource(string json)
            {
                _json = json;
            }

            public override Task<Stream> OpenStreamAsync()
            {
                byte[] byteArray = Encoding.UTF8.GetBytes(_json);
                return Task.FromResult<Stream>(new MemoryStream(byteArray));
            }
        }
    }
}
