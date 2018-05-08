using System;
using FluentAssertions;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Connector.Authentication;
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

                var action = new Action(() => nullServiceCollection.AddBot<TestBot>());

                action.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("services");
            }

            [Fact]
            public void WithoutConfigurationCallback()
            {
                var serviceCollectionMock = new Mock<IServiceCollection>();

                serviceCollectionMock.Object.AddBot<TestBot>();

                serviceCollectionMock.Verify(sc => sc.Add(It.Is<ServiceDescriptor>(sd => sd.ServiceType == typeof(IBot) && sd.ImplementationType == typeof(TestBot) && sd.Lifetime == ServiceLifetime.Transient)));
                serviceCollectionMock.Verify(sc => sc.Add(It.Is<ServiceDescriptor>(sd => sd.ServiceType == typeof(BotFrameworkAdapter) && sd.Lifetime == ServiceLifetime.Singleton)));
                serviceCollectionMock.Verify(sc => sc.Add(It.Is<ServiceDescriptor>(sd => sd.ServiceType == typeof(IConfigureOptions<BotFrameworkOptions>))), Times.Never());
            }

            [Fact]
            public void WithExplicitNullConfigurationCallback()
            {
                var serviceCollectionMock = new Mock<IServiceCollection>();

                serviceCollectionMock.Object.AddBot<TestBot>(null);

                serviceCollectionMock.Verify(sc => sc.Add(It.Is<ServiceDescriptor>(sd => sd.ServiceType == typeof(IBot) && sd.ImplementationType == typeof(TestBot) && sd.Lifetime == ServiceLifetime.Transient)));
                serviceCollectionMock.Verify(sc => sc.Add(It.Is<ServiceDescriptor>(sd => sd.ServiceType == typeof(BotFrameworkAdapter) && sd.Lifetime == ServiceLifetime.Singleton)));
                serviceCollectionMock.Verify(sc => sc.Add(It.Is<ServiceDescriptor>(sd => sd.ServiceType == typeof(IConfigureOptions<BotFrameworkOptions>))), Times.Never());
            }

            [Fact]
            public void WithConfigurationCallback()
            {
                var serviceCollectionMock = new Mock<IServiceCollection>();

                serviceCollectionMock.Object.AddBot<TestBot>(options => 
                {
                    options.Should().NotBeNull();
                });

                serviceCollectionMock.Verify(sc => sc.Add(It.Is<ServiceDescriptor>(sd => sd.ServiceType == typeof(IBot) && sd.ImplementationType == typeof(TestBot) && sd.Lifetime == ServiceLifetime.Transient)));
                serviceCollectionMock.Verify(sc => sc.Add(It.Is<ServiceDescriptor>(sd => sd.ServiceType == typeof(BotFrameworkAdapter) && sd.Lifetime == ServiceLifetime.Singleton)));
                serviceCollectionMock.Verify(sc => sc.Add(It.Is<ServiceDescriptor>(sd => sd.ServiceType == typeof(IConfigureOptions<BotFrameworkOptions>))), Times.Once());
            }
        }        
    }
}
