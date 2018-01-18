using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Claims;

namespace Microsoft.Bot.Connector
{
    public partial class StateClient
    {
        /// <summary>
        /// Create a new instance of the StateClient class
        /// </summary>
        /// <param name="baseUri">Base URI for the State service</param>
        /// <param name="microsoftAppId">Optional. Your Microsoft app id. If null, this setting is read from settings["MicrosoftAppId"]</param>
        /// <param name="microsoftAppPassword">Optional. Your Microsoft app password. If null, this setting is read from settings["MicrosoftAppPassword"]</param>
        /// <param name="handlers">Optional. The delegating handlers to add to the http client pipeline.</param>
        public StateClient(Uri baseUri, string microsoftAppId = null, string microsoftAppPassword = null, params DelegatingHandler[] handlers)
            : this(baseUri, new MicrosoftAppCredentials(microsoftAppId, microsoftAppPassword), handlers: handlers)
        {
        }

        /// <summary>
        /// Create a new instance of the StateClient class
        /// </summary>
        /// <param name="baseUri">Base URI for the State service</param>
        /// <param name="credentials">Credentials for the Connector service</param>
        /// <param name="addJwtTokenRefresher">True, if JwtTokenRefresher should be included; False otherwise.</param>
        /// <param name="handlers">Optional. The delegating handlers to add to the http client pipeline.</param>
        public StateClient(Uri baseUri, MicrosoftAppCredentials credentials, bool addJwtTokenRefresher = true, params DelegatingHandler[] handlers)
            : this(baseUri, addJwtTokenRefresher ? AddJwtTokenRefresher(handlers, credentials) : handlers)
        {
            this.Credentials = credentials;
        }

        /// <summary>
        /// Create a new instance of the StateClient class using Credential source
        /// </summary>
        /// <param name="baseUri">Base URI for the State service</param>
        /// <param name="credentialProvider">Credential source to use</param>
        /// <param name="claimsIdentity">ClaimsIDentity to create the client for</param>
        /// <param name="handlers">Optional. The delegating handlers to add to the http client pipeline.</param>
        public StateClient(Uri baseUri, ICredentialProvider credentialProvider, ClaimsIdentity claimsIdentity = null, params DelegatingHandler[] handlers)
            : this(baseUri, handlers: handlers)
        {
            string appId = null;

            if (claimsIdentity == null)
            {
                var section = BotServiceProvider.Instance.ConfigurationRoot.GetSection(MicrosoftAppCredentials.MicrosoftAppIdKey);

                if (section != null)
                {
                    appId = section.Value;
                }
                else
                {
                    throw new ArgumentNullException($"{nameof(claimsIdentity)} was not provided and it was not possible to resolve AppId from configuration service. Please provide a value for parameter {nameof(claimsIdentity)}.");
                }
            }
            else
            {
                appId = claimsIdentity.GetAppIdFromClaims();
            }

            var password = credentialProvider.GetAppPasswordAsync(appId).Result;
            this.Credentials = new MicrosoftAppCredentials(appId, password);
        }

        /// <summary>
        /// Create a new instance of the StateClient class using Credential source
        /// </summary>
        /// <param name="credentialProvider">Credential source to use</param>
        /// <param name="claimsIdentity">ClaimsIDentity to create the client for</param>
        /// <param name="handlers">Optional. The delegating handlers to add to the http client pipeline.</param>
        public StateClient(ICredentialProvider credentialProvider, ClaimsIdentity claimsIdentity = null, params DelegatingHandler[] handlers)
            : this(null, credentialProvider, claimsIdentity, handlers: handlers)
        {
        }

        /// <summary>
        /// Create a new instance of the StateClient class
        /// </summary>
        /// <remarks> This constructor will use https://state.botframework.com as the baseUri</remarks>
        /// <param name="credentials">Credentials for the Connector service</param>
        /// <param name="addJwtTokenRefresher">True, if JwtTokenRefresher should be included; False otherwise.</param>
        /// <param name="handlers">Optional. The delegating handlers to add to the http client pipeline.</param>
        public StateClient(MicrosoftAppCredentials credentials, bool addJwtTokenRefresher = true, params DelegatingHandler[] handlers)
            : this(addJwtTokenRefresher ? AddJwtTokenRefresher(handlers, credentials) : handlers)
        {
            this.Credentials = credentials;
        }

        private static DelegatingHandler[] AddJwtTokenRefresher(DelegatingHandler[] srcHandlers, MicrosoftAppCredentials credentials)
        {
            var handlers = new List<DelegatingHandler>(srcHandlers);
            handlers.Add(new JwtTokenRefresher(credentials));
            return handlers.ToArray();
        }

        partial void CustomInitialize()
        {
            ConnectorClient.AddUserAgent(this);
        }
    }
}
