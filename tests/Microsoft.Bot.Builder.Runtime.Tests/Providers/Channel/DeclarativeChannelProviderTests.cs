// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using AdaptiveExpressions.Properties;
using Microsoft.Bot.Builder.Runtime.Providers.Channel;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using Xunit;
using IChannel = Microsoft.Bot.Connector.Authentication.IChannelProvider;

namespace Microsoft.Bot.Builder.Runtime.Tests.Providers.Channel
{
    public class DeclarativeChannelProviderTests
    {
        public static IEnumerable<object[]> GetConfigureServicesSucceedsData()
        {
            string channelService = Guid.NewGuid().ToString();

            yield return new object[]
            {
                (StringExpression)null,
                (IConfiguration)TestDataGenerator.BuildConfigurationRoot(),
                (Action<SimpleChannelProvider>)((channelProvider) =>
                {
                    Assert.Null(channelProvider.ChannelService);
                })
            };

            yield return new object[]
            {
                new StringExpression(channelService),
                (IConfiguration)TestDataGenerator.BuildConfigurationRoot(),
                (Action<SimpleChannelProvider>)((channelProvider) =>
                {
                    Assert.Equal(expected: channelService, actual: channelProvider.ChannelService);
                })
            };

            yield return new object[]
            {
                new StringExpression("=channelService"),
                (IConfiguration)TestDataGenerator.BuildConfigurationRoot(new JObject
                {
                    { "channelService", channelService }
                }),
                (Action<SimpleChannelProvider>)((channelProvider) =>
                {
                    Assert.Equal(expected: channelService, actual: channelProvider.ChannelService);
                })
            };
        }

        [Theory]
        [MemberData(nameof(GetConfigureServicesSucceedsData))]
        public void ConfigureServices_Succeeds(
            StringExpression channelService,
            IConfiguration configuration,
            Action<SimpleChannelProvider> assertChannelProvider)
        {
            var services = new ServiceCollection();

            new DeclarativeChannelProvider
            {
                ChannelService = channelService
            }.ConfigureServices(services, configuration);

            IServiceProvider provider = services.BuildServiceProvider();

            Assertions.AssertService<IChannel, SimpleChannelProvider>(
                services,
                provider,
                ServiceLifetime.Singleton,
                assertChannelProvider);
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
                () => new DeclarativeChannelProvider().ConfigureServices(services, configuration));
        }
    }
}
