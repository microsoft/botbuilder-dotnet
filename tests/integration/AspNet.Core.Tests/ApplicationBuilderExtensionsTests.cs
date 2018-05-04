using System;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace AspNet.Core
{
    public class ApplicationBuilderExtensionsTests
    {
        public class UseBotFramework
        {
            [Fact]
            public void NullApplicationBuilderThrows()
            {
                var nullApplicationBuilder = default(IApplicationBuilder);

                var action = new Action(() => nullApplicationBuilder.UseBotFramework());

                action.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("applicationBuilder");
            }

            [Fact]
            public void WithoutPathConfigurationCallback()
            {
                var botFrameworkOptionsMock = new Mock<IOptions<BotFrameworkOptions>>();
                botFrameworkOptionsMock.Setup(o => o.Value)
                    .Returns(new BotFrameworkOptions());

                var serviceProviderMock = new Mock<IServiceProvider>();
                serviceProviderMock.Setup(sp => sp.GetService(typeof(IOptions<BotFrameworkOptions>)))
                    .Returns(botFrameworkOptionsMock.Object);

                var applicationBuilderMock = new Mock<IApplicationBuilder>();
                applicationBuilderMock.Setup(ab => ab.ApplicationServices)
                    .Returns(serviceProviderMock.Object);

                var mappedApplicationBuilderMock = new Mock<IApplicationBuilder>();

                applicationBuilderMock.Setup(ab => ab.New())
                    .Returns(mappedApplicationBuilderMock.Object);

                applicationBuilderMock.Object.UseBotFramework();

                applicationBuilderMock.Verify(ab => ab.New(), Times.Once());
                mappedApplicationBuilderMock.Verify(mab => mab.Build(), Times.Once());
                mappedApplicationBuilderMock.Verify(ab => ab.Use(It.IsAny<Func<RequestDelegate, RequestDelegate>>()), Times.Once());
            }

            [Fact]
            public void WithExplicitNullPathConfigurationCallback()
            {
                var applicationBuilderMock = new Mock<IApplicationBuilder>();

                var action = new Action(() => applicationBuilderMock.Object.UseBotFramework(null));

                action.Should().Throw<ArgumentNullException>();
            }

            [Fact]
            public void WithPathConfigurationCallback()
            {
                var botFrameworkOptionsMock = new Mock<IOptions<BotFrameworkOptions>>();
                botFrameworkOptionsMock.Setup(o => o.Value)
                    .Returns(new BotFrameworkOptions());

                var serviceProviderMock = new Mock<IServiceProvider>();
                serviceProviderMock.Setup(sp => sp.GetService(typeof(IOptions<BotFrameworkOptions>)))
                    .Returns(botFrameworkOptionsMock.Object);

                var mappedApplicationBuilderMock = new Mock<IApplicationBuilder>();

                var applicationBuilderMock = new Mock<IApplicationBuilder>();
                applicationBuilderMock.Setup(ab => ab.ApplicationServices)
                    .Returns(serviceProviderMock.Object);

                applicationBuilderMock.Setup(ab => ab.New())
                    .Returns(mappedApplicationBuilderMock.Object);

                applicationBuilderMock.Object.UseBotFramework(paths =>
                {
                    paths.Should().NotBeNull();
                });

                applicationBuilderMock.Verify(ab => ab.New(), Times.Once());
                mappedApplicationBuilderMock.Verify(mab => mab.Build(), Times.Once());
                mappedApplicationBuilderMock.Verify(mab => mab.Use(It.IsAny<Func<RequestDelegate, RequestDelegate>>()), Times.Once());
            }

            [Fact]
            public void WhenEnableProactiveTrueShouldMapMultipleHandlers()
            {
                var botFrameworkOptionsMock = new Mock<IOptions<BotFrameworkOptions>>();
                botFrameworkOptionsMock.Setup(o => o.Value)
                    .Returns(new BotFrameworkOptions
                    {
                        EnableProactiveMessages = true
                    });

                var serviceProviderMock = new Mock<IServiceProvider>();
                serviceProviderMock.Setup(sp => sp.GetService(typeof(IOptions<BotFrameworkOptions>)))
                    .Returns(botFrameworkOptionsMock.Object);

                var applicationBuilderMock = new Mock<IApplicationBuilder>();
                applicationBuilderMock.Setup(ab => ab.ApplicationServices)
                    .Returns(serviceProviderMock.Object);

                var mappedApplicationBlocks = new[]
                {
                    new Mock<IApplicationBuilder>(),
                    new Mock<IApplicationBuilder>()
                };

                var rootApplicationBuilderNewCallCount = 0;
                applicationBuilderMock.Setup(ab => ab.New())
                    .Returns(() => mappedApplicationBlocks[rootApplicationBuilderNewCallCount++].Object);

                applicationBuilderMock.Object.UseBotFramework(paths =>
                {
                    paths.Should().NotBeNull();
                });

                applicationBuilderMock.Verify(ab => ab.New(), Times.Exactly(2));

                foreach (var mappedApplicationBlock in mappedApplicationBlocks)
                {
                    mappedApplicationBlock.Verify(mab => mab.Build(), Times.Once());
                    mappedApplicationBlock.Verify(mab => mab.Use(It.IsAny<Func<RequestDelegate, RequestDelegate>>()), Times.Once());
                }
            }
        }
    }
}
