// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Bot.Builder.Integration.Runtime.Plugins;
using Microsoft.Bot.Builder.Runtime.Plugins;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.Bot.Builder.Runtime.Tests.Plugins
{
    public class BotPluginDefinitionTests
    {
        public static IEnumerable<object[]> GetLoadArgumentNullExceptionData()
        {
            var pluginEnumerator = new TestBotPluginEnumerator();
            var services = new ServiceCollection();
            IConfiguration configuration = new ConfigurationBuilder().Build();

            yield return new object[]
            {
                "pluginEnumerator",
                (IBotPluginEnumerator)null,
                (IServiceCollection)services,
                (IConfiguration)configuration
            };

            yield return new object[]
            {
                "services",
                (IBotPluginEnumerator)pluginEnumerator,
                (IServiceCollection)null,
                (IConfiguration)configuration
            };

            yield return new object[]
            {
                "configuration",
                (IBotPluginEnumerator)pluginEnumerator,
                (IServiceCollection)services,
                (IConfiguration)null
            };
        }

        public static IEnumerable<object[]> GetLoadSucceedsConfigurationData()
        {
            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>()
                {
                    { "foo", "bar" },
                    { "other", "setting" }
                }).Build();

            Action<IBotPluginLoadContext> assertEmptyConfiguration = (context) =>
            {
                Assert.NotNull(context.Configuration);
                var settings = new Dictionary<string, string>(context.Configuration.AsEnumerable());
                Assert.Empty(settings);
            };

            yield return new object[]
            {
                configuration,
                (string)null,
                assertEmptyConfiguration
            };

            yield return new object[]
            {
                configuration,
                string.Empty,
                assertEmptyConfiguration
            };

            yield return new object[]
            {
                configuration,
                "missing",
                assertEmptyConfiguration
            };

            yield return new object[]
            {
                configuration,
                "foo",
                (Action<IBotPluginLoadContext>)(context =>
                {
                    Assert.NotNull(context.Configuration);

                    var expectedSettings = new Dictionary<string, string>
                    {
                        { "foo", "bar" }
                    };

                    var actualSettings = new Dictionary<string, string>(context.Configuration.AsEnumerable());

                    Assert.Equal(expectedSettings, actualSettings);
                })
            };
        }

        [Theory]
        [MemberData(nameof(GetLoadSucceedsConfigurationData))]
        public void Load_Succeeds_Configuration(
            IConfiguration configuration,
            string settingsPrefix,
            Action<IBotPluginLoadContext> loadAction)
        {
            var plugins = new Dictionary<string, ICollection<IBotPlugin>>(StringComparer.OrdinalIgnoreCase)
            {
                {
                    "TestPlugin", new[] { new TestBotPlugin(loadAction) }
                }
            };

            var pluginEnumerator = new TestBotPluginEnumerator(plugins);
            var services = new ServiceCollection();

            new BotPluginDefinition
            {
                Name = "TestPlugin",
                SettingsPrefix = settingsPrefix
            }.Load(pluginEnumerator, services, configuration);
        }

        [Fact]
        public void Load_Succeeds_Services()
        {
            var plugins = new Dictionary<string, ICollection<IBotPlugin>>(StringComparer.OrdinalIgnoreCase)
            {
                {
                    "TestPlugin", new[]
                    {
                        new TestBotPlugin(loadAction: (context) =>
                        {
                            Assert.NotNull(context.Services);
                            Assert.Empty(context.Services);
                            context.Services.AddSingleton<IStorage, MemoryStorage>();
                        }),
                        new TestBotPlugin(loadAction: (context) =>
                        {
                            Assert.NotNull(context.Services);
                            Assert.Empty(context.Services);
                            context.Services.AddSingleton<IChannelProvider, SimpleChannelProvider>();
                        })
                    }
                }
            };

            var pluginEnumerator = new TestBotPluginEnumerator(plugins);
            var services = new ServiceCollection();
            IConfiguration configuration = new ConfigurationBuilder().Build();

            new BotPluginDefinition
            {
                Name = "TestPlugin"
            }.Load(pluginEnumerator, services, configuration);

            IServiceProvider provider = services.BuildServiceProvider();

            Assertions.AssertService<IStorage, MemoryStorage>(
                services,
                provider,
                ServiceLifetime.Singleton);

            Assertions.AssertService<IChannelProvider, SimpleChannelProvider>(
                services,
                provider,
                ServiceLifetime.Singleton);
        }

        [Theory]
        [MemberData(nameof(GetLoadArgumentNullExceptionData))]
        public void Load_Throws_ArgumentNullException(
            string paramName,
            TestBotPluginEnumerator pluginEnumerator,
            IServiceCollection services,
            IConfiguration configuration)
        {
            Assert.Throws<ArgumentNullException>(
                paramName,
                () => new BotPluginDefinition().Load(pluginEnumerator, services, configuration));
        }

        [Theory]
        [InlineData((string)null)]
        [InlineData("")]
        public void Load_Throws_NameMissing(string name)
        {
            var pluginEnumerator = new TestBotPluginEnumerator();
            var services = new ServiceCollection();
            var configuration = new ConfigurationBuilder().Build();

            Assert.Throws<ArgumentNullException>(
                "pluginName",
                () => new BotPluginDefinition
                    {
                        Name = name
                    }.Load(pluginEnumerator, services, configuration));
        }
    }
}
