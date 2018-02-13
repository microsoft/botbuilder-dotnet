// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Primitives;
using System.Security.Claims;
using System.Linq;

namespace Microsoft.Bot.Builder.BotFramework
{
    public class BotFrameworkAdapter : ActivityAdapterBase
    {
        private const string AppIdRequestInfo = "AppId";

        private readonly ICredentialProvider _credentialProvider;

        /// <summary>
        /// App credentials map.
        /// Why?
        /// Tokens are stored in <see cref="MicrosoftAppCredentials"/>. If we init it everytime it will add token fetch to every single call. So
        /// everytime we encounter a new appId we create <see cref="MicrosoftAppCredentials"/> object and store it here.
        /// No lock?
        /// It can happen that a flood of requests end up initializing the same <see cref="MicrosoftAppCredentials"/> multiple times. But this will
        /// happen for a short duration so not worth using a ConcurrentDictionary which has it's own perf hit.
        /// </summary>
        private readonly IDictionary<string, MicrosoftAppCredentials> _appCredentialsMap = new Dictionary<string, MicrosoftAppCredentials>();

        public BotFrameworkAdapter(string appId, string appPassword) : base()
        {
            _credentialProvider = new SimpleCredentialProvider(appId, appPassword);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BotFrameworkAdapter"/> class.
        /// </summary>
        /// <param name="credentialProvider">The credential provider.</param>
        public BotFrameworkAdapter(ICredentialProvider credentialProvider) :  base()
        {
            _credentialProvider = credentialProvider;
        }

        public async override Task Send(IList<IActivity> activities, IBotContext botContext)
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
                    MicrosoftAppCredentials microsoftAppCredentials = await this.GetAppCredentialsAsync(botContext);

                    var connectorClient = new ConnectorClient(new Uri(activity.ServiceUrl), microsoftAppCredentials);

                    await connectorClient.Conversations.SendToConversationAsync(activity).ConfigureAwait(false);
                }
            }
        }

        public async Task Receive(IDictionary<string, StringValues> requestInfo, Activity activity)
        {
            if (requestInfo == null)
                throw new ArgumentNullException(nameof(requestInfo));

            BotAssert.ActivityNotNull(activity);

            if (await _credentialProvider.IsAuthenticationDisabledAsync() == false)
            {
                if (requestInfo.TryGetValue("Authorization", out StringValues values))
                {
                    ClaimsIdentity claimsIdentity = await JwtTokenValidation.AssertValidActivity(activity, values.SingleOrDefault(), _credentialProvider);

                    MicrosoftAppCredentials.TrustServiceUrl(activity.ServiceUrl);

                    if (claimsIdentity != null)
                    {
                        requestInfo.Add(AppIdRequestInfo, claimsIdentity.Claims.FirstOrDefault(c => c.Type == AuthenticationConstants.AudienceClaim).Value);
                    }
                    else if (_credentialProvider as SimpleCredentialProvider != null)
                    {
                        // Allowing single bot pipeline for unauthenticated requests as the only way to find bot's app Id is to
                        // get it from Token. The condition below for now always stores null.
                        requestInfo.Add(AppIdRequestInfo, (_credentialProvider as SimpleCredentialProvider).AppId);
                    }
                    else
                    {
                        throw new InvalidOperationException("Only StaticCredentialProvider is supported in UnAuthenticated scenarios");
                    }
                }
                else
                    throw new UnauthorizedAccessException("Caller does not have a valid authentication header");
            }

            if (this.OnReceive != null)
                await this.OnReceive(activity, requestInfo).ConfigureAwait(false);
        }

        /// <summary>
        /// Gets the application credentials for sending responses.
        /// </summary>
        /// <param name="botContext">The bot context.</param>
        /// <returns>Application credentials.</returns>
        /// <exception cref="ArgumentException">Failed to get Credentials for post. Only StaticCredentialProvider is supported in unauthenticated scenarios.</exception>
        private async Task<MicrosoftAppCredentials> GetAppCredentialsAsync(IBotContext botContext)
        {
            if (botContext?.RequestInfo == null)
            {
                throw new ArgumentNullException(nameof(botContext));
            }

            if (botContext.RequestInfo.TryGetValue(AppIdRequestInfo, out StringValues appId))
            {
                string botAppId = StringValues.IsNullOrEmpty(appId) ? throw new ArgumentNullException("No valid AppId found", nameof(appId)) : appId.Single();

                if (!this._appCredentialsMap.ContainsKey(botAppId))
                {
                    // Ideally we can just insert into the dictionary and then access it at next point. But this has a chance in which it can fail.
                    // This can happen if you have multiple threads adding values to dictionary and 2 threads add different value (one for Bot1 second for Bot2).
                    // In this case due to race, value can actually not get added. Hence just returing the object directly while trying to add it to dictionary.
                    KeyValuePair<string, MicrosoftAppCredentials> appCreds = new KeyValuePair<string, MicrosoftAppCredentials>(
                        botAppId, 
                        new MicrosoftAppCredentials(botAppId, await this._credentialProvider.GetAppPasswordAsync(botAppId)));
                    this._appCredentialsMap.Add(appCreds);

                    return appCreds.Value;
                }

                return this._appCredentialsMap[botAppId];
            }
            else
            {
                throw new ArgumentNullException("AppId was not found in Request Info", nameof(botContext.RequestInfo));
            }
        }
    }
}
