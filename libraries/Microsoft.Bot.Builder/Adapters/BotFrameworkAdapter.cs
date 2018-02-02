// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Primitives;
using Microsoft.Bot.Connector.Authentication;

namespace Microsoft.Bot.Builder.Adapters
{
    public class BotFrameworkAdapter : ActivityAdapterBase
    {
        private readonly ICredentialProvider _credentialProvider;
        private readonly MicrosoftAppCredentials _credentials;

        public BotFrameworkAdapter(string appId, string appPassword) : base()
        {
            _credentials = new MicrosoftAppCredentials(appId, appPassword);
            _credentialProvider = new StaticCredentialProvider(appId, appPassword);
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

            if (await _credentialProvider.IsAuthenticationDisabledAsync() == false)
            {
                if (headers.TryGetValue("Authorization", out StringValues values))
                {
                    var claims = await JwtTokenValidation.ValidateAuthHeader(values.SingleOrDefault(), _credentialProvider, activity.ServiceUrl);
                    MicrosoftAppCredentials.TrustServiceUrl(activity.ServiceUrl);
                }
                else
                {
                    throw new UnauthorizedAccessException("Caller does not have a valid authentication header");
                }
            }

            if (this.OnReceive != null)
                await this.OnReceive(activity).ConfigureAwait(false);
        }
    }
}
