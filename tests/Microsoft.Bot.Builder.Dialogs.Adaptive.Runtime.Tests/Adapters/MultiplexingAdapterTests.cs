using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Runtime.Extensions;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Builder.Runtime.Tests;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Runtime.Tests.Adapters
{
    public class MultiplexingAdapterTests
    {
        [Fact]
        public void MultiplexingAdapterRegistered()
        {
            // Setup
            IServiceCollection services = new ServiceCollection();
            IConfiguration configuration = new ConfigurationBuilder().Build();

            // We do this here since in asp.net core world this is done for us, but here we need to do it manually.
            services.AddSingleton(configuration);

            // Test
            services.AddBotRuntime(configuration);

            // Assert
            var provider = services.BuildServiceProvider();

            // Core adapter should be register for as IBotFrameworkHttpAdapter for controllers.
            Assertions.AssertService<IBotFrameworkHttpAdapter, CoreBotAdapter>(services, provider, ServiceLifetime.Singleton);

            // Multiplexing adapter should be register as BotAdapter for SkillHandler.
            Assertions.AssertService<BotAdapter, MultiplexingAdapter>(services, provider, ServiceLifetime.Singleton);
        }

        [Fact]
        public async Task ProperlyRoutesPathBasedOnAdapterSettings()
        {
            // Setup
            IServiceCollection services = new ServiceCollection();
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("Adapters/Resources/appSettingsSomeRoute.json")
                .Build();

            var someRouteAdapter = new SomeRouteAdapter();
            var adapter = new MultiplexingAdapter(configuration, new List<IBotFrameworkHttpAdapter> { someRouteAdapter });
            
            var req = new Mock<HttpRequest>();
            req.Setup(mock => mock.Path).Returns("/api/someRoute");
            var res = new Mock<HttpResponse>().Object;
            var bot = new Mock<IBot>().Object;

            // Test
            await adapter.ProcessAsync(req.Object, res, bot);

            // Assert
            Assert.Equal(1, someRouteAdapter.ProcessCalls);
        }

        [Fact]
        public async Task ProperlyRoutesPathWithMultipleAdaptersAndDefaults()
        {
            // Setup
            IServiceCollection services = new ServiceCollection();
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("Adapters/Resources/appSettingsMultipleRoutes.json")
                .Build();

            var someRouteAdapter = new SomeRouteAdapter();
            var someOtherRouteAdapter = new SomeOtherRouteAdapter();
            var defaultAndMessagesAdapter = new DefaultAndMessagesAdapter();
            var adapter = new MultiplexingAdapter(configuration, new List<IBotFrameworkHttpAdapter> { someRouteAdapter, someOtherRouteAdapter, defaultAndMessagesAdapter });

            var req = new Mock<HttpRequest>();
            req.Setup(mock => mock.Path).Returns("/api/someRoute");
            var res = new Mock<HttpResponse>().Object;
            var bot = new Mock<IBot>().Object;

            // Test api/someRoute
            await adapter.ProcessAsync(req.Object, res, bot);

            // Assert
            Assert.Equal(1, someRouteAdapter.ProcessCalls);
            Assert.Equal(0, someOtherRouteAdapter.ProcessCalls);
            Assert.Equal(0, defaultAndMessagesAdapter.ProcessCalls);

            // Test api/someOtherRoute
            req.Setup(mock => mock.Path).Returns("/api/someOtherRoute");
            await adapter.ProcessAsync(req.Object, res, bot);

            // Assert
            Assert.Equal(1, someRouteAdapter.ProcessCalls);
            Assert.Equal(1, someOtherRouteAdapter.ProcessCalls);
            Assert.Equal(0, defaultAndMessagesAdapter.ProcessCalls);

            // Test api/messages
            req.Setup(mock => mock.Path).Returns("/api/messages");
            await adapter.ProcessAsync(req.Object, res, bot);

            // Assert
            Assert.Equal(1, someRouteAdapter.ProcessCalls);
            Assert.Equal(1, someOtherRouteAdapter.ProcessCalls);
            Assert.Equal(1, defaultAndMessagesAdapter.ProcessCalls);

            // Test api/unlistedRoute
            req.Setup(mock => mock.Path).Returns("/api/unlistedRoute");
            await adapter.ProcessAsync(req.Object, res, bot);

            // Assert
            Assert.Equal(1, someRouteAdapter.ProcessCalls);
            Assert.Equal(1, someOtherRouteAdapter.ProcessCalls);
            Assert.Equal(2, defaultAndMessagesAdapter.ProcessCalls);
        }

        [Fact]
        public async void OverriddenBuildAdapterMapAllowsCustomAdapterMapping()
        {
            // Setup
            IServiceCollection services = new ServiceCollection();
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("Adapters/Resources/appSettingsMultipleRoutes.json")
                .Build();

            var someRouteAdapter = new SomeRouteAdapter();
            var someOtherRouteAdapter = new SomeOtherRouteAdapter();
            var defaultAndMessagesAdapter = new DefaultAndMessagesAdapter();
            var adapter = new OverriddenMultiplexingAdapter(configuration, new List<IBotFrameworkHttpAdapter> { someRouteAdapter, someOtherRouteAdapter, defaultAndMessagesAdapter });

            var req = new Mock<HttpRequest>();
            req.Setup(mock => mock.Path).Returns("/api/someRoute");
            var res = new Mock<HttpResponse>().Object;
            var bot = new Mock<IBot>().Object;

            // Test api/someRoute
            await adapter.ProcessAsync(req.Object, res, bot);

            // Assert
            Assert.Equal(1, someRouteAdapter.ProcessCalls);
            Assert.Equal(0, someOtherRouteAdapter.ProcessCalls);
            Assert.Equal(0, defaultAndMessagesAdapter.ProcessCalls);

            // Test api/someOtherRoute
            req.Setup(mock => mock.Path).Returns("/api/someOtherRoute");
            var ex = await Assert.ThrowsAsync<System.ArgumentException>(async () => await adapter.ProcessAsync(req.Object, res, bot));
            Assert.Equal("No adapter available for channelId/route 'someOtherRoute' and no default adapter exists", ex.Message);

            // Assert
            Assert.Equal(1, someRouteAdapter.ProcessCalls);
            Assert.Equal(0, someOtherRouteAdapter.ProcessCalls);
            Assert.Equal(0, defaultAndMessagesAdapter.ProcessCalls);

            // Test api/messages
            req.Setup(mock => mock.Path).Returns("/api/messages");
            await adapter.ProcessAsync(req.Object, res, bot);

            // Assert
            Assert.Equal(1, someRouteAdapter.ProcessCalls);
            Assert.Equal(0, someOtherRouteAdapter.ProcessCalls);
            Assert.Equal(1, defaultAndMessagesAdapter.ProcessCalls);

            // Test api/unlistedRoute
            req.Setup(mock => mock.Path).Returns("/api/unlistedRoute");
            await adapter.ProcessAsync(req.Object, res, bot);

            // Assert
            Assert.Equal(1, someRouteAdapter.ProcessCalls);
            Assert.Equal(0, someOtherRouteAdapter.ProcessCalls);
            Assert.Equal(2, defaultAndMessagesAdapter.ProcessCalls);
        }

        internal class SomeRouteAdapter : BotAdapter, IBotFrameworkHttpAdapter
        {
            internal SomeRouteAdapter()
            {
                ProcessCalls = 0;
            }

            public int ProcessCalls { get; private set; }

            public override Task DeleteActivityAsync(ITurnContext turnContext, ConversationReference reference, CancellationToken cancellationToken)
            {
                throw new System.NotImplementedException();
            }

            public Task ProcessAsync(HttpRequest httpRequest, HttpResponse httpResponse, IBot bot, CancellationToken cancellationToken = default)
            {
                ProcessCalls++;
                return Task.FromResult(nameof(SomeRouteAdapter));
            }

            public override Task<ResourceResponse[]> SendActivitiesAsync(ITurnContext turnContext, Activity[] activities, CancellationToken cancellationToken)
            {
                throw new System.NotImplementedException();
            }

            public override Task<ResourceResponse> UpdateActivityAsync(ITurnContext turnContext, Activity activity, CancellationToken cancellationToken)
            {
                throw new System.NotImplementedException();
            }
        }

        internal class SomeOtherRouteAdapter : SomeRouteAdapter 
        {
        }

        internal class DefaultAndMessagesAdapter : SomeRouteAdapter
        {
        }

        internal class OverriddenMultiplexingAdapter : MultiplexingAdapter
        {
            internal OverriddenMultiplexingAdapter(IConfiguration configuration, IEnumerable<IBotFrameworkHttpAdapter> adapters)
                : base(configuration, adapters)
            {
            }

            protected override void BuildAdapterMap(IEnumerable<IBotFrameworkHttpAdapter> adapters)
            {
                Adapters.Add("someRoute", adapters.First());
                Adapters.Add("messages", adapters.Last());
                Adapters.Add("unlistedRoute", adapters.Last());
            }
        }
    }
}
