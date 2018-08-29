// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Microsoft.Bot.Builder.Integration.AspNet.Core.Tests
{
    public class ServiceCollectionExtensionsTests
    {
        public class AddBotTests
        {
            [Fact]
            public void NullServiceCollectionThrows()
            {
                var nullServiceCollection = default(IServiceCollection);

                var action = new Action(() => nullServiceCollection.AddBot<ServiceRegistrationTestBot>());

                action.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("services");
            }

            [Fact]
            public void WithoutConfigurationCallback()
            {
                var serviceCollectionMock = new Mock<IServiceCollection>();

                serviceCollectionMock.Object.AddBot<ServiceRegistrationTestBot>();

                serviceCollectionMock.Verify(sc => sc.Add(It.Is<ServiceDescriptor>(sd => sd.ServiceType == typeof(IBot) && sd.ImplementationType == typeof(ServiceRegistrationTestBot) && sd.Lifetime == ServiceLifetime.Transient)));
                serviceCollectionMock.Verify(sc => sc.Add(It.Is<ServiceDescriptor>(sd => sd.ServiceType == typeof(BotFrameworkAdapter) && sd.Lifetime == ServiceLifetime.Singleton)));
                serviceCollectionMock.Verify(sc => sc.Add(It.Is<ServiceDescriptor>(sd => sd.ServiceType == typeof(IConfigureOptions<BotFrameworkOptions>))), Times.Never());
            }

            [Fact]
            public void WithExplicitNullConfigurationCallback()
            {
                var serviceCollectionMock = new Mock<IServiceCollection>();

                serviceCollectionMock.Object.AddBot<ServiceRegistrationTestBot>(null);

                serviceCollectionMock.Verify(sc => sc.Add(It.Is<ServiceDescriptor>(sd => sd.ServiceType == typeof(IBot) && sd.ImplementationType == typeof(ServiceRegistrationTestBot) && sd.Lifetime == ServiceLifetime.Transient)));
                serviceCollectionMock.Verify(sc => sc.Add(It.Is<ServiceDescriptor>(sd => sd.ServiceType == typeof(BotFrameworkAdapter) && sd.Lifetime == ServiceLifetime.Singleton)));
                serviceCollectionMock.Verify(sc => sc.Add(It.Is<ServiceDescriptor>(sd => sd.ServiceType == typeof(IConfigureOptions<BotFrameworkOptions>))), Times.Never());
            }

            [Fact]
            public void WithConfigurationCallback()
            {
                var serviceCollectionMock = new Mock<IServiceCollection>();

                serviceCollectionMock.Object.AddBot<ServiceRegistrationTestBot>(options => 
                {
                    options.Should().NotBeNull();
                });

                serviceCollectionMock.Verify(sc => sc.Add(It.Is<ServiceDescriptor>(sd => sd.ServiceType == typeof(IBot) && sd.ImplementationType == typeof(ServiceRegistrationTestBot) && sd.Lifetime == ServiceLifetime.Transient)));
                serviceCollectionMock.Verify(sc => sc.Add(It.Is<ServiceDescriptor>(sd => sd.ServiceType == typeof(BotFrameworkAdapter) && sd.Lifetime == ServiceLifetime.Singleton)));
                serviceCollectionMock.Verify(sc => sc.Add(It.Is<ServiceDescriptor>(sd => sd.ServiceType == typeof(IConfigureOptions<BotFrameworkOptions>))), Times.Once());
            }
        }

        public sealed class ServiceRegistrationTestBot : IBot
        {
            public Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken)
            {
                throw new NotImplementedException("This test bot has no implementation and is intended only for testing service registration.");
            }
        }
    }
}
