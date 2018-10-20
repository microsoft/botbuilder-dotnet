// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
                var serviceCollectionMock = CreateServiceCollectionMock();

                serviceCollectionMock.Object.AddBot<ServiceRegistrationTestBot>();

                VerifyStandardBotServicesAreRegistered(serviceCollectionMock);
            }

            [Fact]
            public void WithExplicitNullConfigurationCallback()
            {
                var serviceCollectionMock = CreateServiceCollectionMock();

                serviceCollectionMock.Object.AddBot<ServiceRegistrationTestBot>(null);

                VerifyStandardBotServicesAreRegistered(serviceCollectionMock);
            }

            [Fact]
            public void WithConfigurationCallback()
            {
                var serviceCollectionMock = CreateServiceCollectionMock();

                var configAction = new Action<BotFrameworkOptions>(options =>
                {
                });

                serviceCollectionMock.Object.AddBot<ServiceRegistrationTestBot>(configAction);

                VerifyStandardBotServicesAreRegistered(serviceCollectionMock);

                // Make sure the configuration action was registered
                serviceCollectionMock.Verify(sc => sc.Add(It.Is<ServiceDescriptor>(sd => (sd.ImplementationInstance is ConfigureNamedOptions<BotFrameworkOptions>) && ((ConfigureNamedOptions<BotFrameworkOptions>)sd.ImplementationInstance).Action == configAction)));
            }

            private static Mock<IServiceCollection> CreateServiceCollectionMock()
            {
                var serviceCollectionMock = new Mock<IServiceCollection>();
                var registeredServices = new List<ServiceDescriptor>();
                serviceCollectionMock.Setup(sc => sc.Add(It.IsAny<ServiceDescriptor>())).Callback<ServiceDescriptor>(sd => registeredServices.Add(sd));
                serviceCollectionMock.Setup(sc => sc.GetEnumerator()).Returns(() => registeredServices.GetEnumerator());
                return serviceCollectionMock;
            }

            private static void VerifyStandardBotServicesAreRegistered(Mock<IServiceCollection> serviceCollectionMock)
            {
                serviceCollectionMock.Verify(sc => sc.Add(It.Is<ServiceDescriptor>(sd => sd.ServiceType == typeof(IBot) && sd.ImplementationType == typeof(ServiceRegistrationTestBot) && sd.Lifetime == ServiceLifetime.Transient)));
                serviceCollectionMock.Verify(sc => sc.Add(It.Is<ServiceDescriptor>(sd => sd.ServiceType == typeof(IAdapterIntegration) && sd.Lifetime == ServiceLifetime.Singleton)));
                serviceCollectionMock.Verify(sc => sc.Add(It.Is<ServiceDescriptor>(sd => sd.ServiceType == typeof(ILogger<BotFrameworkAdapter>))), Times.Once());
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
