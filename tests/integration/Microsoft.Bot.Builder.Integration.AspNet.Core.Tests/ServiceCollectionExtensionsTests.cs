// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Bot.Builder.Integration.AspNet.Core.Tests.Mocks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Microsoft.Bot.Builder.Integration.AspNet.Core.Tests
{
    public class ServiceCollectionExtensionsTests
    {
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
                var serviceCollection = new ServiceCollection();

                serviceCollection.AddBotFrameworkAdapterIntegration();

                Assert.Single(serviceCollection);
                Assert.Equal(typeof(IAdapterIntegration), serviceCollection[0].ServiceType);
                Assert.NotNull(serviceCollection[0].ImplementationFactory);
            }

            [Fact]
            public void WithExplicitNullConfigurationCallback()
            {
                var serviceCollection = new ServiceCollection();

                serviceCollection.AddBotFrameworkAdapterIntegration((Action<BotFrameworkOptions>)null);

                Assert.Single(serviceCollection);
                Assert.Equal(typeof(IAdapterIntegration), serviceCollection[0].ServiceType);
                Assert.NotNull(serviceCollection[0].ImplementationFactory);
            }

            [Fact]
            public void WithConfigurationCallback()
            {
                var serviceCollection = new ServiceCollection();

                var configAction = new Action<BotFrameworkOptions>(options =>
                {
                });

                serviceCollection.AddBotFrameworkAdapterIntegration(configAction);

                Assert.Equal(7, serviceCollection.Count);
                Assert.Equal(typeof(IAdapterIntegration), serviceCollection[6].ServiceType);
                Assert.NotNull(serviceCollection[6].ImplementationFactory);
                Assert.True(serviceCollection[5].ImplementationInstance is ConfigureNamedOptions<BotFrameworkOptions>);
                Assert.Equal(configAction, ((ConfigureNamedOptions<BotFrameworkOptions>)serviceCollection[5].ImplementationInstance).Action);
            }

            [Fact]
            public void RegistersEvenIfAnExistingIAdapterIntegrationIsAlreadyRegistered()
            {
                var serviceCollection = new ServiceCollection();

                serviceCollection.AddSingleton<IAdapterIntegration>(Mock.Of<IAdapterIntegration>());

                serviceCollection.AddBotFrameworkAdapterIntegration();

                Assert.Equal(2, serviceCollection.Count);
                Assert.Equal(typeof(IAdapterIntegration), serviceCollection[0].ServiceType);
                Assert.Equal(typeof(IAdapterIntegration), serviceCollection[1].ServiceType);
                Assert.NotNull(serviceCollection[1].ImplementationFactory);
            }
        }

        public class AddBotTests
        {
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
                    var serviceCollection = new ServiceCollection();

                    serviceCollection.AddBot<ServiceRegistrationTestBot>();

                    Assert.Equal(2, serviceCollection.Count);
                    Assert.Equal(typeof(IAdapterIntegration), serviceCollection[0].ServiceType);
                    Assert.Equal(ServiceLifetime.Singleton, serviceCollection[0].Lifetime);
                    Assert.Equal(typeof(IBot), serviceCollection[1].ServiceType);
                    Assert.Equal(typeof(ServiceRegistrationTestBot), serviceCollection[1].ImplementationType);
                    Assert.Equal(ServiceLifetime.Transient, serviceCollection[1].Lifetime);
                }

                [Fact]
                public void WithExplicitNullConfigurationCallback()
                {
                    var serviceCollection = new ServiceCollection();

                    serviceCollection.AddBot<ServiceRegistrationTestBot>((Action<BotFrameworkOptions>)null);

                    Assert.Equal(2, serviceCollection.Count);
                    Assert.Equal(typeof(IAdapterIntegration), serviceCollection[0].ServiceType);
                    Assert.Equal(ServiceLifetime.Singleton, serviceCollection[0].Lifetime);
                    Assert.Equal(typeof(IBot), serviceCollection[1].ServiceType);
                    Assert.Equal(typeof(ServiceRegistrationTestBot), serviceCollection[1].ImplementationType);
                    Assert.Equal(ServiceLifetime.Transient, serviceCollection[1].Lifetime);
                }

                [Fact]
                public void WithConfigurationCallback()
                {
                    var serviceCollection = new ServiceCollection();

                    var configAction = new Action<BotFrameworkOptions>(options =>
                    {
                    });

                    serviceCollection.AddBot<ServiceRegistrationTestBot>(configAction);

                    Assert.Equal(8, serviceCollection.Count);
                    Assert.Equal(typeof(IAdapterIntegration), serviceCollection[6].ServiceType);
                    Assert.Equal(ServiceLifetime.Singleton, serviceCollection[6].Lifetime);
                    Assert.Equal(typeof(IBot), serviceCollection[7].ServiceType);
                    Assert.Equal(typeof(ServiceRegistrationTestBot), serviceCollection[7].ImplementationType);
                    Assert.Equal(ServiceLifetime.Transient, serviceCollection[7].Lifetime);
                    Assert.True(serviceCollection[5].ImplementationInstance is ConfigureNamedOptions<BotFrameworkOptions>);
                    Assert.Equal(configAction, ((ConfigureNamedOptions<BotFrameworkOptions>)serviceCollection[5].ImplementationInstance).Action);
                }

                [Fact]
                public void WithConfigurationCallbackWithOptions()
                {
                    var serviceCollection = new ServiceCollection();

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

                    serviceCollection.AddBot<ServiceRegistrationTestBot>(configAction);

                    Assert.Equal(8, serviceCollection.Count);
                    Assert.Equal(typeof(IAdapterIntegration), serviceCollection[6].ServiceType);
                    Assert.Equal(ServiceLifetime.Singleton, serviceCollection[6].Lifetime);

                    var adapterServiceDescriptor = serviceCollection.FirstOrDefault(sd => sd.ServiceType == typeof(IAdapterIntegration));

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
                        default(CancellationToken));

                    // Verify the mock middleware was actually invoked (the only indicator we have that it was added).
                    middlewareMock.Verify(
                        m => m.OnTurnAsync(
                        It.Is<TurnContext>(tc => true),
                        It.Is<NextDelegate>(nd => true),
                        It.Is<CancellationToken>(ct => true)), Times.Once());

                    // And make sure the error handler was added.
                    Assert.Equal(onTurnError, adapter.OnTurnError);

                    // Make sure the configuration action was registered.
                    Assert.True(serviceCollection[5].ImplementationInstance is ConfigureNamedOptions<BotFrameworkOptions>);
                    Assert.Equal(configAction, ((ConfigureNamedOptions<BotFrameworkOptions>)serviceCollection[5].ImplementationInstance).Action);
                }

                [Fact]
                public void WithConfigurationCallbackWithOptionsAndCustomAppCredentials()
                {
                    var serviceCollection = new ServiceCollection();

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

                    serviceCollection.AddBot<ServiceRegistrationTestBot>(configAction);

                    Assert.Equal(8, serviceCollection.Count);
                    Assert.Equal(typeof(IAdapterIntegration), serviceCollection[6].ServiceType);
                    Assert.Equal(ServiceLifetime.Singleton, serviceCollection[6].Lifetime);

                    var adapterServiceDescriptor = serviceCollection.FirstOrDefault(sd => sd.ServiceType == typeof(IAdapterIntegration));

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
                    var serviceCollection = new ServiceCollection();
                    var adapterIntegration = Mock.Of<IAdapterIntegration>();

                    serviceCollection.AddSingleton(adapterIntegration);
                    serviceCollection.AddBot<ServiceRegistrationTestBot>();

                    Assert.Equal(2, serviceCollection.Count);
                    Assert.Equal(typeof(IAdapterIntegration), serviceCollection[0].ServiceType);
                    Assert.Equal(adapterIntegration, serviceCollection[0].ImplementationInstance);
                    Assert.Equal(typeof(IBot), serviceCollection[1].ServiceType);
                }
            }

            public class FactoryMethod : AddBotTests
            {
                [Fact]
                public void NullServiceCollectionThrows()
                {
                    var nullServiceCollection = default(ServiceCollection);

                    var action = new Action(() => nullServiceCollection.AddBot(sp => new ServiceRegistrationTestBot()));

                    action.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("services");
                }

                [Fact]
                public void NullFactoryMethodThrows()
                {
                    var serviceCollection = new ServiceCollection();

                    var action = new Action(() => serviceCollection.AddBot((Func<IServiceProvider, IBot>)null));

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
                    var serviceCollection = new ServiceCollection();
                    var adapterIntegration = Mock.Of<IAdapterIntegration>();

                    serviceCollection.AddSingleton(adapterIntegration);
                    serviceCollection.AddBot(sp => new ServiceRegistrationTestBot());

                    Assert.Equal(2, serviceCollection.Count);
                    Assert.Equal(typeof(IAdapterIntegration), serviceCollection[0].ServiceType);
                    Assert.Equal(adapterIntegration, serviceCollection[0].ImplementationInstance);
                    Assert.Equal(typeof(IBot), serviceCollection[1].ServiceType);
                }
            }

            public class SingletonInstance : AddBotTests
            {
                [Fact]
                public void NullServiceCollectionThrows()
                {
                    var nullServiceCollection = default(ServiceCollection);

                    var action = new Action(() => nullServiceCollection.AddBot(sp => new ServiceRegistrationTestBot()));

                    action.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("services");
                }

                [Fact]
                public void NullBotInstanceThrows()
                {
                    var serviceCollection = new ServiceCollection();

                    var action = new Action(() => serviceCollection.AddBot((IBot)null));

                    action.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("bot");
                }

                [Fact]
                public void WithoutConfigurationCallback()
                {
                    var serviceCollection = new ServiceCollection();
                    var botInstance = Mock.Of<IBot>();

                    serviceCollection.AddBot(botInstance);

                    Assert.Equal(2, serviceCollection.Count);
                    Assert.Equal(typeof(IAdapterIntegration), serviceCollection[0].ServiceType);
                    Assert.Equal(ServiceLifetime.Singleton, serviceCollection[0].Lifetime);
                    Assert.Equal(typeof(IBot), serviceCollection[1].ServiceType);
                    Assert.NotNull(serviceCollection[1].ImplementationInstance);
                    Assert.Equal(ServiceLifetime.Singleton, serviceCollection[1].Lifetime);
                }

                [Fact]
                public void WithExplicitNullConfigurationCallback()
                {
                    var serviceCollection = new ServiceCollection();
                    var botInstance = Mock.Of<IBot>();

                    serviceCollection.AddBot(botInstance, null);

                    Assert.Equal(2, serviceCollection.Count);
                    Assert.Equal(typeof(IAdapterIntegration), serviceCollection[0].ServiceType);
                    Assert.Equal(ServiceLifetime.Singleton, serviceCollection[0].Lifetime);
                    Assert.Equal(typeof(IBot), serviceCollection[1].ServiceType);
                    Assert.NotNull(serviceCollection[1].ImplementationInstance);
                    Assert.Equal(ServiceLifetime.Singleton, serviceCollection[1].Lifetime);
                }

                [Fact]
                public void WithConfigurationCallback()
                {
                    var serviceCollection = new ServiceCollection();
                    var botInstance = Mock.Of<IBot>();

                    serviceCollection.AddBot(botInstance, options => options.Should().NotBeNull());

                    Assert.Equal(8, serviceCollection.Count);
                    Assert.Equal(typeof(IAdapterIntegration), serviceCollection[6].ServiceType);
                    Assert.Equal(ServiceLifetime.Singleton, serviceCollection[6].Lifetime);
                    Assert.Equal(typeof(IBot), serviceCollection[7].ServiceType);
                    Assert.NotNull(serviceCollection[7].ImplementationInstance);
                    Assert.Equal(ServiceLifetime.Singleton, serviceCollection[7].Lifetime);
                }

                [Fact]
                public void DoesntReplaceExistingAdapterIntegration()
                {
                    var serviceCollection = new ServiceCollection();
                    var adapterIntegration = Mock.Of<IAdapterIntegration>();

                    serviceCollection.AddSingleton(adapterIntegration);
                    serviceCollection.AddBot(new ServiceRegistrationTestBot());

                    Assert.Equal(2, serviceCollection.Count);
                    Assert.Equal(typeof(IAdapterIntegration), serviceCollection[0].ServiceType);
                    Assert.Equal(adapterIntegration, serviceCollection[0].ImplementationInstance);
                    Assert.Equal(typeof(IBot), serviceCollection[1].ServiceType);
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
