// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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

        public class ResolveBotFrameworkAdapter
        {
            [Fact]
            public void BotFrameworkAdapterShouldResolve()
            {
                // Simulate a LoggerFactory (could be AppInsights/etc)
                var mockLog = new Mock<ILogger<BotFrameworkAdapter>>();
                var mockLogFactory = new Mock<ILoggerFactory>();
                mockLogFactory.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(mockLog.Object);

                var serviceCollection = new ServiceCollection()
                    .AddOptions()
                    .AddSingleton<ILoggerFactory>(mockLogFactory.Object)
                    .AddBot<ServiceResolutionTestBot>(options =>
                    {
                        options.CredentialProvider = Mock.Of<ICredentialProvider>();
                    });

                var serviceProvider = serviceCollection.BuildServiceProvider();

                var botFrameworkAdapter = serviceProvider.GetService<IAdapterIntegration>();

                botFrameworkAdapter.Should().NotBeNull();
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

        public class ResolveILogger
        {
            [Fact]
            public void BotFrameworkAdapterILoggerShouldResolve()
            {
                // Simulate a LoggerFactory (could be AppInsights/etc)
                var mockLog = new Mock<ILogger<IAdapterIntegration>>();
                var mockLogFactory = new Mock<ILoggerFactory>();
                mockLogFactory.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(mockLog.Object);

                var serviceCollection = new ServiceCollection()
                    .AddSingleton<ILoggerFactory>(mockLogFactory.Object)
                    .AddOptions()
                    .AddBot<ServiceResolutionTestBot>(options =>
                    {
                        options.CredentialProvider = Mock.Of<ICredentialProvider>();
                    });

                var serviceProvider = serviceCollection.BuildServiceProvider();

                var frameworkLogger = serviceProvider.GetService<ILogger<IAdapterIntegration>>();

                frameworkLogger.Should().NotBeNull();
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
