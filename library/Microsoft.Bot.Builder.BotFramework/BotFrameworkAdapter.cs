using Microsoft.Bot.Connector;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Adapters
{
    public class BotFrameworkAdapter : ActivityAdapterBase
    {
        private readonly MicrosoftAppCredentials _credentials;
        private readonly BotAuthenticator _authenticator;

        public BotFrameworkAdapter(string appId, string appPassword) : base()
        {
            _authenticator = new BotAuthenticator(appId, appPassword);
            _credentials = new MicrosoftAppCredentials(appId, appPassword);
        }

        public async override Task Post(IList<IActivity> activities)
        {
            BotAssert.ActivityListNotNull(activities);

            foreach (Activity activity in activities)
            {
                if (activity.Type == "delay")
                {   
                    // The Activity Schema doesn't have a delay type build in, so it's simulated
                    // here in the Bot. This matches the behavior in the Node connector. 
                    int delayMs = (int)activity.Value;
                    await Task.Delay(delayMs).ConfigureAwait(false);
                }
                else
                {
                    var connectorClient = new ConnectorClient(new Uri(activity.ServiceUrl), _credentials);
                    await connectorClient.Conversations.SendToConversationAsync(activity).ConfigureAwait(false);
                }
            }
        }

        public async Task Receive(IDictionary<string, StringValues> headers, Activity activity)
        {
            if (headers == null)
                throw new ArgumentNullException(nameof(headers));

            BotAssert.ActivityNotNull(activity);

            if (this.OnReceive != null)
                await this.OnReceive(activity).ConfigureAwait(false);
        }
    }
}
