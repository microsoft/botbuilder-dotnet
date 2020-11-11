// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Builder.Runtime.Builders.Middleware;
using Microsoft.Bot.Builder.Runtime.Providers.Adapter;
using Microsoft.Bot.Builder.Runtime.Settings;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.Bot.Builder.Runtime.Tests.Providers.Adapter
{
    public class BotCoreAdapterProviderTests
    {
        public static IEnumerable<object[]> GetConfigureServicesSucceedsData()
        {
            yield return new object[]
            {
                (IList<IMiddlewareBuilder>)Array.Empty<IMiddlewareBuilder>()
            };

            yield return new object[]
            {
                new List<IMiddlewareBuilder>
                {
                    new InspectionMiddlewareBuilder()
                }
            };
        }

        [Theory]
        [MemberData(nameof(GetConfigureServicesSucceedsData))]
        public void ConfigureServices_Succeeds(IList<IMiddlewareBuilder> middleware)
        {
            var services = new ServiceCollection();
            IConfiguration configuration = TestDataGenerator.BuildConfigurationRoot();

            services.AddTransient<ConversationState>();
            services.AddTransient<IConfiguration>(_ => configuration);
            services.AddTransient<ICredentialProvider>(_ =>
                new SimpleCredentialProvider(appId: Guid.NewGuid().ToString(), password: Guid.NewGuid().ToString()));
            services.AddTransient<IStorage>(_ => new MemoryStorage());
            services.AddTransient<UserState>();

            var adapterProvider = new BotCoreAdapterProvider();
            foreach (IMiddlewareBuilder m in middleware)
            {
                adapterProvider.Middleware.Add(m);
            }

            adapterProvider.ConfigureServices(services, configuration);

            IServiceProvider provider = services.BuildServiceProvider();

            Assertions.AssertOptions<CoreBotAdapterOptions>(
                provider,
                (options) =>
                {
                    Assert.Equal(expected: adapterProvider.Middleware.Count, actual: options.Middleware.Count);
                    for (int i = 0; i < adapterProvider.Middleware.Count; i++)
                    {
                        Assert.Equal(expected: adapterProvider.Middleware[i], actual: options.Middleware[i]);
                    }
                });

            Assertions.AssertService<IBotFrameworkHttpAdapter, CoreBotAdapter>(
                services,
                provider,
                ServiceLifetime.Singleton);

            Assertions.AssertService<BotAdapter, CoreBotAdapter>(
                services,
                provider,
                ServiceLifetime.Singleton);
        }

        [Theory]
        [MemberData(
            nameof(ProviderTestDataGenerator.GetConfigureServicesArgumentNullExceptionData),
            MemberType = typeof(ProviderTestDataGenerator))]
        public void ConfigureServices_Throws_ArgumentNullException(
            string paramName,
            IServiceCollection services,
            IConfiguration configuration)
        {
            Assert.Throws<ArgumentNullException>(
                paramName,
                () => new BotCoreAdapterProvider().ConfigureServices(services, configuration));
        }
    }
}
