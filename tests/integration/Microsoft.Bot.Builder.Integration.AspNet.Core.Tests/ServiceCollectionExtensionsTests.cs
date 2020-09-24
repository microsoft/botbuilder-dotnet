// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Bot.Builder.Integration.AspNet.Core.Tests.Mocks;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Microsoft.Bot.Builder.Integration.AspNet.Core.Tests
{
    public class ServiceCollectionExtensionsTests
    {
        private static Mock<IServiceCollection> CreateServiceCollectionMock()
        {
            var serviceCollectionMock = new Mock<IServiceCollection>();
            var registeredServices = new List<ServiceDescriptor>();

            serviceCollectionMock.Setup(sc => sc.Add(It.IsAny<ServiceDescriptor>())).Callback<ServiceDescriptor>(sd => registeredServices.Add(sd));
            serviceCollectionMock.Setup(sc => sc.GetEnumerator()).Returns(() => registeredServices.GetEnumerator());

            return serviceCollectionMock;
        }

        public class AddBotFrameworkAdapterIntegrationTests : ServiceCollectionExtensionsTests
        {
            [Fact]
            public void NullServiceCollectionThrows()
            {
                var nullServiceCollection = default(IServiceCollection);

                var action = new Action(() => nullServiceCollection.AddBotFrameworkAdapterIntegration());

                action.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("services");
            }

            [Fact]
            public void WithoutConfigurationCallback()
            {
                var serviceCollectionMock = CreateServiceCollectionMock();

                serviceCollectionMock.Object.AddBotFrameworkAdapterIntegration();

                VerifyBotFrameworkAdapterIntegrationIsRegistered(serviceCollectionMock);
            }

            [Fact]
            public void WithExplicitNullConfigurationCallback()
            {
                var serviceCollectionMock = CreateServiceCollectionMock();

                serviceCollectionMock.Object.AddBotFrameworkAdapterIntegration((Action<BotFrameworkOptions>)null);

                VerifyBotFrameworkAdapterIntegrationIsRegistered(serviceCollectionMock);
            }

            [Fact]
            public void WithConfigurationCallback()
            {
                var serviceCollectionMock = CreateServiceCollectionMock();

                var configAction = new Action<BotFrameworkOptions>(options =>
                {
                });

                serviceCollectionMock.Object.AddBotFrameworkAdapterIntegration(configAction);

                VerifyBotFrameworkAdapterIntegrationIsRegistered(serviceCollectionMock);

                // Make sure the configuration action was registered
                serviceCollectionMock.Verify(sc => sc.Add(It.Is<ServiceDescriptor>(sd => (sd.ImplementationInstance is ConfigureNamedOptions<BotFrameworkOptions>) && ((ConfigureNamedOptions<BotFrameworkOptions>)sd.ImplementationInstance).Action == configAction)));
            }

            [Fact]
            public void RegistersEvenIfAnExistingIAdapterIntegrationIsAlreadyRegistered()
            {
                var serviceCollectionMock = CreateServiceCollectionMock();

                serviceCollectionMock.Object.AddSingleton<IAdapterIntegration>(Mock.Of<IAdapterIntegration>());

                serviceCollectionMock.Object.AddBotFrameworkAdapterIntegration();

                serviceCollectionMock.Verify(sc => sc.Add(It.Is<ServiceDescriptor>(sd => sd.ServiceType == typeof(IAdapterIntegration))), Times.Exactly(2));
            }

            private static void VerifyBotFrameworkAdapterIntegrationIsRegistered(Mock<IServiceCollection> serviceCollectionMock)
            {
                serviceCollectionMock.Verify(sc => sc.Add(It.Is<ServiceDescriptor>(sd => sd.ServiceType == typeof(IAdapterIntegration) && sd.ImplementationFactory != null)));
            }
        }

        public class AddBotTests
        {
            private static void VerifyStandardBotServicesAreRegistered(Mock<IServiceCollection> serviceCollectionMock)
            {
                serviceCollectionMock.Verify(sc => sc.Add(It.Is<ServiceDescriptor>(sd => sd.ServiceType == typeof(IAdapterIntegration) && sd.Lifetime == ServiceLifetime.Singleton)));
            }

            public class TBotOnly : AddBotTests
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

                    VerifyExpectedBotServicesAreRegistered(serviceCollectionMock);
                }

                [Fact]
                public void WithExplicitNullConfigurationCallback()
                {
                    var serviceCollectionMock = CreateServiceCollectionMock();

                    serviceCollectionMock.Object.AddBot<ServiceRegistrationTestBot>((Action<BotFrameworkOptions>)null);

                    VerifyExpectedBotServicesAreRegistered(serviceCollectionMock);
                }

                [Fact]
                public void WithConfigurationCallback()
                {
                    var serviceCollectionMock = CreateServiceCollectionMock();

                    var configAction = new Action<BotFrameworkOptions>(options =>
                    {
                    });

                    serviceCollectionMock.Object.AddBot<ServiceRegistrationTestBot>(configAction);

                    VerifyExpectedBotServicesAreRegistered(serviceCollectionMock);

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

                    Func<ITurnContext, Exception, Task> onTurnError = (turnContext, exception) =>
                    {
                        return Task.CompletedTask;
                    };

                    var middlewareMock = new Mock<IMiddleware>();
                    middlewareMock.Setup(m => m.OnTurnAsync(It.IsAny<TurnContext>(), It.IsAny<NextDelegate>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

                    var configAction = new Action<BotFrameworkOptions>(options =>
                    {
                        options.OnTurnError = onTurnError;
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
                    middlewareMock.Verify(
                        m => m.OnTurnAsync(
                        It.Is<TurnContext>(tc => true),
                        It.Is<NextDelegate>(nd => true),
                        It.Is<CancellationToken>(ct => true)), Times.Once());

                    // And make sure the error handler was added.
                    Assert.Equal(onTurnError, adapter.OnTurnError);

                    // Make sure the configuration action was registered.
                    serviceCollectionMock.Verify(sc => sc.Add(It.Is<ServiceDescriptor>(sd => (sd.ImplementationInstance is ConfigureNamedOptions<BotFrameworkOptions>) && ((ConfigureNamedOptions<BotFrameworkOptions>)sd.ImplementationInstance).Action == configAction)));
                }

                [Fact]
                public void WithConfigurationCallbackWithOptionsAndCustomAppCredentials()
                {
                    var serviceCollectionMock = new Mock<IServiceCollection>();
                    var registeredServices = new List<ServiceDescriptor>();
                    serviceCollectionMock.Setup(sc => sc.Add(It.IsAny<ServiceDescriptor>())).Callback<ServiceDescriptor>(sd => registeredServices.Add(sd));
                    serviceCollectionMock.Setup(sc => sc.GetEnumerator()).Returns(() => registeredServices.GetEnumerator());

                    Func<ITurnContext, Exception, Task> onTurnError = (turnContext, exception) =>
                    {
                        return Task.CompletedTask;
                    };

                    var middlewareMock = new Mock<IMiddleware>();
                    middlewareMock.Setup(m => m.OnTurnAsync(It.IsAny<TurnContext>(), It.IsAny<NextDelegate>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

                    var configAction = new Action<BotFrameworkOptions>(options =>
                    {
                        options.AppCredentials = new MockAppCredentials();
                        options.OnTurnError = onTurnError;
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
                }

                [Fact]
                public void DoesntReplaceExistingAdapterIntegration()
                {
                    var serviceCollectionMock = CreateServiceCollectionMock();
                    var adapterIntegration = Mock.Of<IAdapterIntegration>();

                    serviceCollectionMock.Object.AddSingleton<IAdapterIntegration>(adapterIntegration);

                    serviceCollectionMock.Object.AddBot<ServiceRegistrationTestBot>();

                    serviceCollectionMock.Verify(sc => sc.Add(It.Is<ServiceDescriptor>(sd => sd.ServiceType == typeof(IAdapterIntegration))), Times.Once());
                    serviceCollectionMock.Verify(sc => sc.Add(It.Is<ServiceDescriptor>(sd => sd.ServiceType == typeof(IAdapterIntegration) && sd.ImplementationInstance == adapterIntegration)));
                }

                private void VerifyExpectedBotServicesAreRegistered(Mock<IServiceCollection> serviceCollectionMock)
                {
                    VerifyStandardBotServicesAreRegistered(serviceCollectionMock);

                    serviceCollectionMock.Verify(sc => sc.Add(It.Is<ServiceDescriptor>(sd => sd.ServiceType == typeof(IBot) && sd.ImplementationType == typeof(ServiceRegistrationTestBot) && sd.Lifetime == ServiceLifetime.Transient)));
                }
            }

            public class FactoryMethod : AddBotTests
            {
                [Fact]
                public void NullServiceCollectionThrows()
                {
                    var nullServiceCollection = default(IServiceCollection);

                    var action = new Action(() => nullServiceCollection.AddBot(sp => new ServiceRegistrationTestBot()));

                    action.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("services");
                }

                [Fact]
                public void NullFactoryMethodThrows()
                {
                    var serviceCollectionMock = CreateServiceCollectionMock();

                    var action = new Action(() => serviceCollectionMock.Object.AddBot((Func<IServiceProvider, IBot>)null));

                    action.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("botFactory");
                }

#if disabled
                [Fact]
                public void WithoutConfigurationCallback()
                {
                    var serviceCollectionMock = CreateServiceCollectionMock();

                    var botFactory = new Func<IServiceProvider, ServiceRegistrationTestBot>(sp => new ServiceRegistrationTestBot());

                    serviceCollectionMock.Object.AddBot(botFactory);

                    VerifyExpectedBotServicesAreRegistered(serviceCollectionMock);
                }

                [Fact]
                public void WithExplicitNullConfigurationCallback()
                {
                    var serviceCollectionMock = CreateServiceCollectionMock();

                    var botFactory = new Func<IServiceProvider, ServiceRegistrationTestBot>(sp => new ServiceRegistrationTestBot());

                    serviceCollectionMock.Object.AddBot(botFactory, (Action<BotFrameworkOptions>)null);

                    VerifyExpectedBotServicesAreRegistered(serviceCollectionMock);
                }

                [Fact]
                public void WithConfigurationCallback()
                {
                    var serviceCollectionMock = CreateServiceCollectionMock();

                    var botFactory = new Func<IServiceProvider, ServiceRegistrationTestBot>(sp => new ServiceRegistrationTestBot());

                    serviceCollectionMock.Object.AddBot(
                        botFactory,
                        options =>
                        {
                            options.Should().NotBeNull();
                        });

                    VerifyExpectedBotServicesAreRegistered(serviceCollectionMock);
                }
#endif 

                [Fact]
                public void DoesntReplaceExistingAdapterIntegration()
                {
                    var serviceCollectionMock = CreateServiceCollectionMock();
                    var adapterIntegration = Mock.Of<IAdapterIntegration>();

                    serviceCollectionMock.Object.AddSingleton<IAdapterIntegration>(adapterIntegration);

                    serviceCollectionMock.Object.AddBot<ServiceRegistrationTestBot>(sp => new ServiceRegistrationTestBot());

                    serviceCollectionMock.Verify(sc => sc.Add(It.Is<ServiceDescriptor>(sd => sd.ServiceType == typeof(IAdapterIntegration))), Times.Once());
                    serviceCollectionMock.Verify(sc => sc.Add(It.Is<ServiceDescriptor>(sd => sd.ServiceType == typeof(IAdapterIntegration) && sd.ImplementationInstance == adapterIntegration)));
                }

                private void VerifyExpectedBotServicesAreRegistered(Mock<IServiceCollection> serviceCollectionMock)
                {
                    VerifyStandardBotServicesAreRegistered(serviceCollectionMock);

                    serviceCollectionMock.Verify(sc => sc.Add(It.Is<ServiceDescriptor>(sd => sd.ServiceType == typeof(IBot) && sd.ImplementationFactory != null && sd.Lifetime == ServiceLifetime.Transient)));
                }
            }

            public class SingletonInstance : AddBotTests
            {
                [Fact]
                public void NullServiceCollectionThrows()
                {
                    var nullServiceCollection = default(IServiceCollection);

                    var action = new Action(() => nullServiceCollection.AddBot(sp => new ServiceRegistrationTestBot()));

                    action.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("services");
                }

                [Fact]
                public void NullBotInstanceThrows()
                {
                    var serviceCollectionMock = new Mock<IServiceCollection>();

                    var action = new Action(() => serviceCollectionMock.Object.AddBot((IBot)null));

                    action.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("bot");
                }

                [Fact]
                public void WithoutConfigurationCallback()
                {
                    var serviceCollectionMock = CreateServiceCollectionMock();

                    var botInstance = Mock.Of<IBot>();

                    serviceCollectionMock.Object.AddBot(botInstance);

                    VerifyExpectedBotServicesAreRegistered(serviceCollectionMock);
                }

                [Fact]
                public void WithExplicitNullConfigurationCallback()
                {
                    var serviceCollectionMock = CreateServiceCollectionMock();

                    var botInstance = Mock.Of<IBot>();

                    serviceCollectionMock.Object.AddBot(botInstance, (Action<BotFrameworkOptions>)null);

                    VerifyExpectedBotServicesAreRegistered(serviceCollectionMock);
                }

                [Fact]
                public void WithConfigurationCallback()
                {
                    var serviceCollectionMock = CreateServiceCollectionMock();

                    var botInstance = Mock.Of<IBot>();

                    serviceCollectionMock.Object.AddBot(
                        botInstance,
                        options =>
                        {
                            options.Should().NotBeNull();
                        });

                    VerifyExpectedBotServicesAreRegistered(serviceCollectionMock);
                }

                [Fact]
                public void DoesntReplaceExistingAdapterIntegration()
                {
                    var serviceCollectionMock = CreateServiceCollectionMock();
                    var adapterIntegration = Mock.Of<IAdapterIntegration>();

                    serviceCollectionMock.Object.AddSingleton<IAdapterIntegration>(adapterIntegration);

                    serviceCollectionMock.Object.AddBot(new ServiceRegistrationTestBot());

                    serviceCollectionMock.Verify(sc => sc.Add(It.Is<ServiceDescriptor>(sd => sd.ServiceType == typeof(IAdapterIntegration))), Times.Once());
                    serviceCollectionMock.Verify(sc => sc.Add(It.Is<ServiceDescriptor>(sd => sd.ServiceType == typeof(IAdapterIntegration) && sd.ImplementationInstance == adapterIntegration)));
                }

                private void VerifyExpectedBotServicesAreRegistered(Mock<IServiceCollection> serviceCollectionMock)
                {
                    VerifyStandardBotServicesAreRegistered(serviceCollectionMock);

                    serviceCollectionMock.Verify(sc => sc.Add(It.Is<ServiceDescriptor>(sd => sd.ServiceType == typeof(IBot) && sd.ImplementationInstance != null && sd.Lifetime == ServiceLifetime.Singleton)));
                }
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
