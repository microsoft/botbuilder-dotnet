// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Microsoft.Bot.Builder.Integration.AspNet.Core.Tests
{
    public class ServiceResolutionTests
    {
        public class ResolveBotFrameworkOptions
        {
            [Fact]
            public void DefaultOptionsShouldResolve()
            {
                var serviceCollection = new ServiceCollection()
                    .AddOptions()
                    .AddBot<ServiceResolutionTestBot>();

                var serviceProvider = serviceCollection.BuildServiceProvider();

                var botFrameworkOptions = serviceProvider.GetService<IOptions<BotFrameworkOptions>>();

                botFrameworkOptions.Value.Should().NotBeNull();
            }

            [Fact]
            public void DefaultOptionsShouldResolveWithDefaultSimpleCredentialProviderWhenNotExplicitlyConfigured()
            {
                var serviceCollection = new ServiceCollection()
                    .AddOptions()
                    .AddBot<ServiceResolutionTestBot>();

                var serviceProvider = serviceCollection.BuildServiceProvider();

                var botFrameworkOptions = serviceProvider.GetService<IOptions<BotFrameworkOptions>>();

                botFrameworkOptions.Value.CredentialProvider.Should().NotBeNull()
                    .And.BeOfType<SimpleCredentialProvider>();
            }
        }

        public class ResolveIAdapterIntegration
        {
            [Fact]
            public void IAdapterIntegrationShouldResolve()
            {
                var serviceCollection = new ServiceCollection()
                    .AddLogging()
                    .AddOptions()
                    .AddBot<ServiceResolutionTestBot>(options =>
                    {
                        options.CredentialProvider = Mock.Of<ICredentialProvider>();
                    });

                var serviceProvider = serviceCollection.BuildServiceProvider();

                var adapterIntegration = serviceProvider.GetService<IAdapterIntegration>();

                adapterIntegration.Should().NotBeNull().And.BeOfType<BotFrameworkAdapter>();
            }
        }

        public class ResolveIBot
        {
            [Fact]
            public void IBotShouldResolve()
            {
                var serviceCollection = new ServiceCollection()
                    .AddOptions()
                    .AddBot<ServiceResolutionTestBot>();

                var serviceProvider = serviceCollection.BuildServiceProvider();

                var bot = serviceProvider.GetService<IBot>();

                bot.Should().NotBeNull()
                    .And.BeOfType<ServiceResolutionTestBot>();
            }
        }

        public sealed class ServiceResolutionTestBot : IBot
        {
            public Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken)
            {
                throw new NotImplementedException("This test bot has no implementation and is intended only for testing service resolution.");
            }
        }
    }
}
