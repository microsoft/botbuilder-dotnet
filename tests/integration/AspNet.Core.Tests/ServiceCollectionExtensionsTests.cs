using System;
using System.Threading.Tasks;
using Microsoft.Bot;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using FluentAssertions;
using Moq;
using Xunit;

namespace AspNet.Core
{
    public class ServiceCollectionExtensionsTests
    {
        public class AddBot
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

        internal sealed class TestBot : IBot
        {
            public Task OnTurn(ITurnContext turnContext)
            {
                throw new NotImplementedException();
            }
        }
    }
}
