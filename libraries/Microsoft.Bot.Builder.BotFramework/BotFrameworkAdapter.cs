// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.BotFramework
{
    public class BotFrameworkAdapter : ActivityAdapterBase
    {
        private readonly SimpleCredentialProvider _credentialProvider;
        private readonly MicrosoftAppCredentials _credentials;
        private readonly HttpClient _httpClient; 

        public BotFrameworkAdapter(IConfiguration configuration, HttpClient httpClient = null) : base()
        {
            _httpClient = httpClient ?? new HttpClient();
            _credentialProvider = new ConfigurationCredentialProvider(configuration);
            _credentials = new MicrosoftAppCredentials(_credentialProvider.AppId, _credentialProvider.Password);                                   
        }

        public BotFrameworkAdapter(string appId, string appPassword, HttpClient httpClient = null) : base()
        {
            _httpClient = httpClient ?? new HttpClient();
            _credentials = new MicrosoftAppCredentials(appId, appPassword);
            _credentialProvider = new SimpleCredentialProvider(appId, appPassword);
        }

        public async override Task Send(IList<IActivity> activities)
        {
            BotAssert.ActivityListNotNull(activities);

            foreach (Activity activity in activities)
            {
                if (activity.Type == ActivityTypesEx.Delay)
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

        public async Task Receive(AspNetCore.Http.HttpRequest httpRequest)
        {
            var ser = new JsonSerializer();
            using (var sr = new System.IO.StreamReader(httpRequest.Body))
            using (var jtr = new JsonTextReader(sr))
            {
                // Should handle if incoming msg isn't an activity - throw some "malformed" error? 400 bad request?
                var activity = ser.Deserialize<Activity>(jtr);
                BotAssert.ActivityNotNull(activity);

                try
                {
                    var authHeader = httpRequest.Headers[@"Authorization"].FirstOrDefault();
            await JwtTokenValidation.AssertValidActivity(activity, authHeader, _credentialProvider, _httpClient);

                    if (this.OnReceive != null)
                    {
                await OnReceive(activity).ConfigureAwait(false);
                    }

                    httpRequest.HttpContext.Response.StatusCode = AspNetCore.Http.StatusCodes.Status200OK;
                }
                catch (UnauthorizedAccessException)
                {
                    httpRequest.HttpContext.Response.StatusCode = AspNetCore.Http.StatusCodes.Status401Unauthorized;
                }
            }

        }
    }
}
