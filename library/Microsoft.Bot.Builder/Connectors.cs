using Microsoft.Bot.Connector;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder
{
    public abstract class Connector : IConnector
    {        
        public Bot Bot {get; set;}               

        public Connector()
        {            
        }

        public abstract Task Post(IList<IActivity> activities, CancellationToken token);

        public virtual async Task Receive(IActivity activity, CancellationToken token)
        {
            BotAssert.ActivityNotNull(activity);
            BotAssert.CancellationTokenNotNull(token);

            await Bot.RunPipeline(activity, token).ConfigureAwait(false) ;
        }
    }

    public class BotFrameworkConnector : Connector, IHttpConnector
    {
        private readonly MicrosoftAppCredentials _credentials;
        private readonly BotAuthenticator _authenticator;       

        public BotFrameworkConnector(string appId, string appPassword) : base()
        {
            _authenticator = new BotAuthenticator(appId, appPassword);
            _credentials = new MicrosoftAppCredentials(appId, appPassword);            
        }
     
        public async override Task Post(IList<IActivity> activities, CancellationToken token)
        {
            BotAssert.ActivityListNotNull(activities);
            BotAssert.CancellationTokenNotNull(token); 

            foreach (Activity activity in activities)
            {
                var connectorClient = new ConnectorClient(new Uri(activity.ServiceUrl), _credentials); 
                await connectorClient.Conversations.SendToConversationAsync(activity, token).ConfigureAwait(false);
            }
        }

        public async Task Receive(IDictionary<string, StringValues> headers, IActivity activity, CancellationToken token)
        {
            if (headers == null)
                throw new ArgumentNullException("headers");

            BotAssert.ActivityNotNull(activity);
            BotAssert.CancellationTokenNotNull(token); 

            if (await _authenticator.TryAuthenticateAsync(headers, new[] { activity }, token))
            {
                await base.Receive(activity, token).ConfigureAwait(false);
            }
            else
            {
                throw new UnauthorizedAccessException();
            }
        }
    }

    public class TraceConnector : Connector
    {
        public TraceConnector() : base ()
        {
        }

        public override Task Post(IList<IActivity> activities, CancellationToken token)
        {
            foreach (var activity in activities)
            {
                if (activity.GetActivityType() == ActivityTypes.Message)
                {
                    Trace.WriteLine((activity as IMessageActivity).Text);
                }
                else
                {
                    Trace.WriteLine((activity as Activity).Type);
                }
            }
            return Task.CompletedTask;
        }
    }
  
    public class ConsoleConnector : Connector
    {
        public ConsoleConnector() : base()
        {
        }

        public override Task Post(IList<IActivity> activities, CancellationToken token)
        {
            foreach (Activity activity in activities)
            {
                switch (activity.GetActivityType())
                {
                    case ActivityTypes.Message:
                        if (activity.Attachments != null && activity.Attachments.Any())
                        {
                            var attachment = activity.Attachments.Count == 1 ? "1 attachments" : $"{activity.Attachments.Count()} attachments";
                            Console.WriteLine($"{activity.Text} with {attachment} ");
                        }
                        else
                        {
                            Console.WriteLine($"{activity.Text}");
                        }
                        break;
                    default:
                        Console.WriteLine("Bot: activity type: {0}", activity.Type);
                        break;

                }
            }
            return Task.CompletedTask;
        }

        public async Task Listen()
        {            
            while (true)
            {
                var msg = Console.ReadLine();
                if (msg == null || msg.ToLower() == "quit")
                {
                    break;
                }

                var activity = new Activity()
                {
                    Text = msg,
                    ChannelId = "console",
                    From = new ChannelAccount(id: "user", name: "User1"),
                    Recipient = new ChannelAccount(id: "bot", name: "Bot"),
                    Conversation = new ConversationAccount(id: "Convo1"),
                    Timestamp = DateTime.UtcNow,
                    Id = Guid.NewGuid().ToString(),
                    Type = ActivityTypes.Message
                };

                await this.Receive(activity, CancellationToken.None);                
            }
        }
    }    
}
