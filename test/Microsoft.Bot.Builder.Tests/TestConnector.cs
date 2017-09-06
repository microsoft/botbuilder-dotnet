using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Connector;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Bot.Builder.Tests
{
    public delegate Task TestValidator(IList<IActivity> activities);
    public class TestConnector : Connector
    {
        public TestConnector(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }
        
        public override async Task Post(IList<IActivity> activities, CancellationToken token)
        {
            var validator = _serviceProvider.GetRequiredService<TestValidator>();
            await validator(activities);
        }
    }

    public static partial class TestConnectorExtensions
    {
        public static IServiceCollection UseTestConnector(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddScoped<TestConnector>();
            serviceCollection.AddScoped<IConnector>(t => t.GetRequiredService<TestConnector>());
            return serviceCollection;
        }
    }
    
    public class TestRunner
    {
        private readonly IServiceCollection _serviceCollection;

        public TestRunner(IServiceCollection serviceCollection)
        {
            _serviceCollection = serviceCollection ?? throw new ArgumentNullException("serviceCollection");           
        }

        public static class ChannelID
        {
            public const string User = "testUser";
            public const string Bot = "testBot";
        }

        public static Activity MakeTestMessage()
        {
            return new Activity()
            {
                Id = Guid.NewGuid().ToString(),
                Type = ActivityTypes.Message,
                From = new ChannelAccount { Id = ChannelID.User },
                Conversation = new ConversationAccount { Id = Guid.NewGuid().ToString() },
                Recipient = new ChannelAccount { Id = ChannelID.Bot },
                ServiceUrl = "InvalidServiceUrl",
                ChannelId = "Test",
                Attachments = Array.Empty<Attachment>(),
                Entities = Array.Empty<Entity>(),
            };
        }
        
        public async Task<TestRunner> Test(string testMessage, TestValidator validator, CancellationToken token = default(CancellationToken))
        {
            _serviceCollection.AddSingleton<TestValidator>(validator);

            var provider = _serviceCollection.BuildServiceProvider();
            using (var scope = provider.CreateScope())
            {
                var connector = scope.ServiceProvider.GetRequiredService<TestConnector>();
                var message = MakeTestMessage();
                message.Text = testMessage;
                await connector.Receive(message, token);
            }
            return this; 
        }
    }
}
