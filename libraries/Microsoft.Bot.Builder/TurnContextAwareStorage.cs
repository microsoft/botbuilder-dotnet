using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder
{
    public class TurnContextAwareStorage : ITurnContextAwareStorage
    {
        private const string ApiEndpoint = "state";
        private const string ShortMemoryPropertyName = "ShortMemory";
        private const string ConversationIdKeyName = "ConversationId";
        private const string PropertyNameKeyName = "PropertyName";
        private readonly HttpClient _httpClient;
        private readonly ICredentialProvider _credentialProvider;
        private readonly IChannelProvider _channelProvider;

        public TurnContextAwareStorage(
            HttpClient httpClient, 
            ICredentialProvider credentialProvider,
            IChannelProvider channelProvider = null)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _credentialProvider = credentialProvider;
            _channelProvider = channelProvider;
        }

        public Task DeleteAsync(string[] keys, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<IDictionary<string, object>> ReadAsync(string[] keys, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public async Task<IDictionary<string, object>> ReadAsync(ITurnContext turnContext, CancellationToken cancellationToken = default)
        {
            using (var httpRequestMessage = new HttpRequestMessage())
            {
                var activity = turnContext.Activity;
                var token = await GetTokenAsync(turnContext).ConfigureAwait(false);
                activity.ServiceUrl = "https://sidlocalhost.ngrok.io/api/skills/";
                httpRequestMessage.Method = HttpMethod.Get;
                httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                httpRequestMessage.RequestUri = new Uri($"{activity.ServiceUrl}{ApiEndpoint}?{ConversationIdKeyName}={turnContext.Activity.Conversation.Id}&{PropertyNameKeyName}={ShortMemoryPropertyName}");

                var response = await _httpClient.SendAsync(httpRequestMessage, cancellationToken).ConfigureAwait(false);
                var responseString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                var responseData = JsonConvert.DeserializeObject<StateRestPayload>(responseString);

                return responseData.Data;
            }
        }

        public Task WriteAsync(IDictionary<string, object> changes, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public async Task WriteAsync(ITurnContext turnContext, IDictionary<string, object> data, CancellationToken cancellationToken = default)
        {
            using (var httpRequestMessage = new HttpRequestMessage())
            {
                var activity = turnContext.Activity;
                var token = await GetTokenAsync(turnContext).ConfigureAwait(false);
                httpRequestMessage.Method = HttpMethod.Post;
                httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                httpRequestMessage.RequestUri = new Uri($"{activity.ServiceUrl}{ApiEndpoint}");

                var payload = new StateRestPayload();
                payload.ConversationId = turnContext.Activity.Conversation.Id;
                payload.PropertyName = ShortMemoryPropertyName;
                payload.Data = data;

                httpRequestMessage.Content = new StringContent(JsonConvert.SerializeObject(payload));

                await _httpClient.SendAsync(httpRequestMessage, cancellationToken).ConfigureAwait(false);
            }
        }

        private async Task<string> GetTokenAsync(ITurnContext turnContext)
        {
            var oAuthScope = turnContext.TurnState.Get<string>("Microsoft.Bot.Builder.BotAdapter.OAuthScope");
            var token = await (await GetAppCredentialsAsync(GetBotAppId(turnContext), oAuthScope).ConfigureAwait(false)).GetTokenAsync().ConfigureAwait(false);

            return token;
        }

        /// <summary>
        /// Gets the AppId of the Bot out of the TurnState.
        /// </summary>
        /// <param name="turnContext">The context object for the turn.</param>
        /// <returns>Bot's AppId.</returns>
        private string GetBotAppId(ITurnContext turnContext)
        {
            var botIdentity = (ClaimsIdentity)turnContext.TurnState.Get<IIdentity>("BotIdentity");
            if (botIdentity == null)
            {
                throw new InvalidOperationException("An IIdentity is required in TurnState for this operation.");
            }

            var appId = botIdentity.Claims.FirstOrDefault(claim => claim.Type == AuthenticationConstants.AudienceClaim)?.Value;
            if (string.IsNullOrWhiteSpace(appId))
            {
                throw new InvalidOperationException("Unable to get the bot AppId from the audience claim.");
            }

            return appId;
        }

        /// <summary>
        /// Gets the application credentials. App credentials are cached to avoid refreshing the
        /// token each time.
        /// </summary>
        /// <param name="appId">The application identifier (AAD ID for the bot).</param>
        /// <param name="oAuthScope">The scope for the token. Skills use the skill's app ID. </param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>App credentials.</returns>
        private async Task<AppCredentials> GetAppCredentialsAsync(string appId, string oAuthScope, CancellationToken cancellationToken = default)
        {
            ConcurrentDictionary<string, AppCredentials> appCredentialMap = new ConcurrentDictionary<string, AppCredentials>();
            AppCredentials botAppCredentials = null;
            if (string.IsNullOrWhiteSpace(appId))
            {
                return MicrosoftAppCredentials.Empty;
            }

            var cacheKey = $"{appId}{oAuthScope}";
            if (appCredentialMap.TryGetValue(cacheKey, out var appCredentials))
            {
                return appCredentials;
            }

            // If app credentials were provided, use them as they are the preferred choice moving forward
            if (botAppCredentials != null)
            {
                // Cache the credentials for later use
                appCredentialMap[cacheKey] = botAppCredentials;
                return botAppCredentials;
            }

            // Credentials not found in cache, build them
            botAppCredentials = await BuildCredentialsAsync(appId, oAuthScope).ConfigureAwait(false);

            // Cache the credentials for later use
            appCredentialMap[cacheKey] = botAppCredentials;
            return botAppCredentials;
        }

        /// <summary>
        /// Logic to build an <see cref="AppCredentials"/> object to be used to acquire tokens
        /// for this HttpClient.
        /// </summary>
        /// <param name="appId">The application id.</param>
        /// <param name="oAuthScope">The optional OAuth scope.</param>
        /// <returns>The app credentials to be used to acquire tokens.</returns>
        private async Task<AppCredentials> BuildCredentialsAsync(string appId, string oAuthScope = null)
        {
            // Get the password from the credential provider
            var appPassword = await _credentialProvider.GetAppPasswordAsync(appId).ConfigureAwait(false);

            // Construct an AppCredentials using the app + password combination. If government, we create a government specific credential.
            return _channelProvider != null && _channelProvider.IsGovernment() ? new MicrosoftGovernmentAppCredentials(appId, appPassword) : new MicrosoftAppCredentials(appId, appPassword);
        }
    }
}
