// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Microsoft.Bot.Builder.Integration.AspNet.Core.Tests
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
        }
    }
}
