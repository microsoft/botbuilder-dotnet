using Microsoft.Bot.Connector;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Adapters
{
    public class BotFrameworkAdapter : ActivityAdapter, IHttpActivityAdapter
    {
        private readonly MicrosoftAppCredentials _credentials;
        private readonly BotAuthenticator _authenticator;       

        public BotFrameworkAdapter(string appId, string appPassword) : base()
        {
            _authenticator = new BotAuthenticator(appId, appPassword);
            _credentials = new MicrosoftAppCredentials(appId, appPassword);            
        }
     
        public async override Task Post(IList<Activity> activities, CancellationToken token)
        {
            BotAssert.ActivityListNotNull(activities);
            BotAssert.CancellationTokenNotNull(token); 

            foreach (Activity activity in activities)
            {
                var connectorClient = new ConnectorClient(new Uri(activity.ServiceUrl), _credentials); 
                await connectorClient.Conversations.SendToConversationAsync(activity, token).ConfigureAwait(false);
            }
        }

        public async Task Receive(IDictionary<string, StringValues> headers, Activity activity, CancellationToken token)
        {
            if (headers == null)
                throw new ArgumentNullException(nameof(headers)); 

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

    }
