// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
<<<<<<< HEAD
=======
using System.Linq;
>>>>>>> webapi done - unit tests for dotnet work in progress
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
        public class MockServiceCollection : List<ServiceDescriptor>, IServiceCollection
        {
        }

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
                var serviceCollection = new MockServiceCollection();

                serviceCollection.AddBot<ServiceRegistrationTestBot>();

<<<<<<< HEAD
                VerifyStandardBotServicesAreRegistered(serviceCollectionMock);
=======
                Assert.Contains(serviceCollection, sd => sd.ServiceType == typeof(IBot) && sd.ImplementationType == typeof(ServiceRegistrationTestBot) && sd.Lifetime == ServiceLifetime.Transient);


                //Assert.Contains(serviceCollection, sd => sd.ServiceType == typeof(IAdapterIntegration) && sd.ImplementationType == typeof(BotFrameworkAdapter) && sd.Lifetime == ServiceLifetime.Singleton);

                var x = serviceCollection.Find(sd => sd.ServiceType == typeof(IAdapterIntegration) && sd.Lifetime == ServiceLifetime.Singleton);

                var sp = new Mock<IServiceProvider>();
                var obj = x.ImplementationFactory(sp.Object);

                Assert.DoesNotContain(serviceCollection, sd => sd.ServiceType == typeof(IConfigureOptions<BotFrameworkOptions>));
>>>>>>> webapi done - unit tests for dotnet work in progress
            }

            [Fact]
            public void WithExplicitNullConfigurationCallback()
            {
                var serviceCollectionMock = new Mock<IServiceCollection>();

                serviceCollectionMock.Object.AddBot<ServiceRegistrationTestBot>(null);

                VerifyStandardBotServicesAreRegistered(serviceCollectionMock);
            }

            [Fact]
            public void WithConfigurationCallback()
            {
                var serviceCollectionMock = new Mock<IServiceCollection>();
                var registeredServices = new List<ServiceDescriptor>();

                serviceCollectionMock.Setup(sc => sc.Add(It.IsAny<ServiceDescriptor>()))
                    .Callback<ServiceDescriptor>(sd => registeredServices.Add(sd));

                serviceCollectionMock.Setup(sc => sc.GetEnumerator()).Returns(() => registeredServices.GetEnumerator());


                var configAction = new Action<BotFrameworkOptions>(options =>
                {
                });

                serviceCollectionMock.Object.AddBot<ServiceRegistrationTestBot>(configAction);

                VerifyStandardBotServicesAreRegistered(serviceCollectionMock);

                // Make sure the configuration action was registered
                serviceCollectionMock.Verify(sc => sc.Add(It.Is<ServiceDescriptor>(sd => (sd.ImplementationInstance is ConfigureNamedOptions<BotFrameworkOptions>) && ((ConfigureNamedOptions<BotFrameworkOptions>)sd.ImplementationInstance).Action == configAction)));
            }

            private static void VerifyStandardBotServicesAreRegistered(Mock<IServiceCollection> serviceCollectionMock)
            {
                serviceCollectionMock.Verify(sc => sc.Add(It.Is<ServiceDescriptor>(sd => sd.ServiceType == typeof(IBot) && sd.ImplementationType == typeof(ServiceRegistrationTestBot) && sd.Lifetime == ServiceLifetime.Transient)));
                serviceCollectionMock.Verify(sc => sc.Add(It.Is<ServiceDescriptor>(sd => sd.ServiceType == typeof(BotFrameworkAdapter) && sd.Lifetime == ServiceLifetime.Singleton)));
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
