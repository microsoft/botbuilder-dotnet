// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Bot.Builder.Integration.Runtime.Component;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Microsoft.Bot.Builder.Runtime.Tests.Components
{
    public class BotPluginDefinitionTests
    {
        public static IEnumerable<object[]> GetLoadArgumentNullExceptionData()
        {
            var pluginEnumerator = new TestBotComponentEnumerator();
            var services = new ServiceCollection();
            IConfiguration configuration = new ConfigurationBuilder().Build();

            yield return new object[]
            {
                "pluginEnumerator",
                (IBotComponentEnumerator)null,
                (IServiceCollection)services,
                (IConfiguration)configuration
            };

            yield return new object[]
            {
                "services",
                (IBotComponentEnumerator)pluginEnumerator,
                (IServiceCollection)null,
                (IConfiguration)configuration
            };

            yield return new object[]
            {
                "configuration",
                (IBotComponentEnumerator)pluginEnumerator,
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

            Action<IServiceCollection, IConfiguration, ILogger> assertEmptyConfiguration = (services, config, logger) =>
            {
                Assert.NotNull(config);
                var settings = new Dictionary<string, string>(config.AsEnumerable());
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
                (Action<IServiceCollection, IConfiguration, ILogger>)((services, configuration, logger) =>
                {
                    Assert.NotNull(configuration);

                    var expectedSettings = new Dictionary<string, string>
                    {
                        { "foo", "bar" }
                    };

                    var actualSettings = new Dictionary<string, string>(configuration.AsEnumerable());

                    Assert.Equal(expectedSettings, actualSettings);
                })
            };
        }

        [Theory]
        [MemberData(nameof(GetLoadSucceedsConfigurationData))]
        public void Load_Succeeds_Configuration(
            IConfiguration configuration,
            string settingsPrefix,
            Action<IServiceCollection, IConfiguration, ILogger> loadAction)
        {
            var plugins = new Dictionary<string, ICollection<BotComponent>>(StringComparer.OrdinalIgnoreCase)
            {
                {
                    "TestPlugin", new[] { new TestBotComponent(loadAction) }
                }
            };

            var pluginEnumerator = new TestBotComponentEnumerator(plugins);
            var services = new ServiceCollection();

            new BotComponentDefinition
            {
                Name = "TestPlugin",
                SettingsPrefix = settingsPrefix
            }.Load(pluginEnumerator, services, configuration);
        }

        [Fact]
        public void Load_Succeeds_Services()
        {
            var components = new Dictionary<string, ICollection<BotComponent>>(StringComparer.OrdinalIgnoreCase)
            {
                {
                    "TestPlugin", new[]
                    {
                        new TestBotComponent(loadAction: (services, configuration, logger) =>
                        {
                            Assert.NotNull(services);
                            Assert.Empty(services);
                            services.AddSingleton<IStorage, MemoryStorage>();
                        }),
                        new TestBotComponent(loadAction: (services, configuration, logger) =>
                        {
                            Assert.NotNull(services);
                            Assert.Empty(services);
                            services.AddSingleton<IChannelProvider, SimpleChannelProvider>();
                        })
                    }
                }
            };

            var pluginEnumerator = new TestBotComponentEnumerator(components);
            var services = new ServiceCollection();
            IConfiguration configuration = new ConfigurationBuilder().Build();

            new BotComponentDefinition
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
            TestBotComponentEnumerator pluginEnumerator,
            IServiceCollection services,
            IConfiguration configuration)
        {
            Assert.Throws<ArgumentNullException>(
                paramName,
                () => new BotComponentDefinition().Load(pluginEnumerator, services, configuration));
        }

        [Theory]
        [InlineData((string)null)]
        [InlineData("")]
        public void Load_Throws_NameMissing(string name)
        {
            var pluginEnumerator = new TestBotComponentEnumerator();
            var services = new ServiceCollection();
            var configuration = new ConfigurationBuilder().Build();

            Assert.Throws<ArgumentNullException>(
                "componentName",
                () => new BotComponentDefinition
                {
                    Name = name
                }.Load(pluginEnumerator, services, configuration));
        }
    }
}
