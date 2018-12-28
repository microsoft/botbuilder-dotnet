// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
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

            [Fact]
            public void WithConfigurationCallbackWithOptions()
            {
                var serviceCollectionMock = new Mock<IServiceCollection>();
                var registeredServices = new List<ServiceDescriptor>();
                serviceCollectionMock.Setup(sc => sc.Add(It.IsAny<ServiceDescriptor>())).Callback<ServiceDescriptor>(sd => registeredServices.Add(sd));
                serviceCollectionMock.Setup(sc => sc.GetEnumerator()).Returns(() => registeredServices.GetEnumerator());

                Func<ITurnContext, Exception, Task> OnTurnError = (turnContext, exception) =>
                {
                    return Task.CompletedTask;
                };

                var middlewareMock = new Mock<IMiddleware>();
                middlewareMock.Setup(m => m.OnTurnAsync(It.IsAny<TurnContext>(), It.IsAny<NextDelegate>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

                var configAction = new Action<BotFrameworkOptions>(options =>
                {
                    options.OnTurnError = OnTurnError;
                    options.Middleware.Add(middlewareMock.Object);
                });

                serviceCollectionMock.Object.AddBot<ServiceRegistrationTestBot>(configAction);

                VerifyStandardBotServicesAreRegistered(serviceCollectionMock);

                var adapterServiceDescriptor = registeredServices.FirstOrDefault(sd => sd.ServiceType == typeof(IAdapterIntegration));

                Assert.NotNull(adapterServiceDescriptor);

                var mockOptions = new BotFrameworkOptions();
                configAction(mockOptions);

                var mockLog = new Mock<ILogger<IAdapterIntegration>>();

                // The following tests the factory that was added to the ServiceDescriptor.
                var serviceProviderMock = new Mock<IServiceProvider>();
                serviceProviderMock.Setup(sp => sp.GetService(typeof(IOptions<BotFrameworkOptions>))).Returns(Options.Create(mockOptions));
                serviceProviderMock.Setup(sp => sp.GetService(typeof(ILogger<IAdapterIntegration>))).Returns(mockLog.Object);

                // Invoke the factory to create an adapter
                var adapter = adapterServiceDescriptor.ImplementationFactory(serviceProviderMock.Object) as BotFrameworkAdapter;

                // Make sure we have a BotFrameworkAdapter (the default added by the Add<IBot>).
                Assert.NotNull(adapter);

                // Now we have to run the adapter to test whether the middleware was actually added.
                var activity = MessageFactory.Text("hi");
                activity.ServiceUrl = "http://localhost";

                var result = adapter.ProcessActivityAsync(
                    string.Empty,
                    activity,
                    (turnContext, ct) => { return Task.CompletedTask; },
                    default(CancellationToken))
                    .Result;

                // Verify the mock middleware was actually invoked (the only indicator we have that it was added).
                middlewareMock.Verify(m => m.OnTurnAsync(
                    It.Is<TurnContext>(tc => true),
                    It.Is<NextDelegate>(nd => true),
                    It.Is<CancellationToken>(ct => true)), Times.Once());

                // And make sure the error handler was added.
                Assert.Equal(OnTurnError, adapter.OnTurnError);

                // Make sure the configuration action was registered.
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
                serviceCollectionMock.Verify(sc => sc.Add(It.Is<ServiceDescriptor>(sd => sd.ServiceType == typeof(ILogger<IAdapterIntegration>))), Times.Once());
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
