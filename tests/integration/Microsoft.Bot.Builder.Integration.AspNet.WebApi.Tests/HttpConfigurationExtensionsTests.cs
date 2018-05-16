using System;
using System.Web.Http;
using FluentAssertions;
using Microsoft.Bot.Builder.Integration.AspNet.WebApi.Handlers;
using Microsoft.Bot.Connector.Authentication;
using Moq;
using Xunit;

namespace Microsoft.Bot.Builder.Integration.AspNet.WebApi.Tests
{
    public class HttpConfigurationExtensionsTests
    {
        public class MapBotFramework
        {
            [Fact]
            public void NullHttpConfigurationThrows()
            {
                var httpConfiguration = default(HttpConfiguration);

                var action = new Action(() => httpConfiguration.MapBotFramework());

                action.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("httpConfiguration");
            }

            [Fact]
            public void WithoutConfigurationCallback()
            {
                var httpConfiguration = new HttpConfiguration();

                httpConfiguration.MapBotFramework();

                var routes = httpConfiguration.Routes;

                routes.Count.Should().Be(1);

                var botMessageHandlerRoute = routes[BotMessageHandler.RouteName];
                botMessageHandlerRoute.Handler.Should().BeOfType<BotMessageHandler>();
            }

            [Fact]
            public void WithConfigurationCallback()
            {
                var httpConfiguration = new HttpConfiguration();

                httpConfiguration.MapBotFramework(config =>
                {
                    config.Should().NotBeNull();
                });

                var routes = httpConfiguration.Routes;

                routes.Count.Should().Be(1);

                var botMessageHandlerRoute = routes[BotMessageHandler.RouteName];
                botMessageHandlerRoute.Handler.Should().BeOfType<BotMessageHandler>();
            }

            [Fact]
            public void EnableProactiveShouldMapMultipleHandlers()
            {
                var httpConfiguration = new HttpConfiguration();

                httpConfiguration.MapBotFramework(config =>
                {
                    config.EnableProactiveMessages();
                });

                var routes = httpConfiguration.Routes;
                routes.Count.Should().Be(2);

                var botMessageHandlerRoute = routes[BotMessageHandler.RouteName];
                botMessageHandlerRoute.Handler.Should().BeOfType<BotMessageHandler>();

                var botProactiveMessageHandlerRoute = routes[BotProactiveMessageHandler.RouteName];
                botProactiveMessageHandlerRoute.Handler.Should().BeOfType<BotProactiveMessageHandler>();
            }
        }
    }
}
