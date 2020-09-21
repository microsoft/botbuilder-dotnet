// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder
{
    internal class UserTokenProvider : IExtendedUserTokenProvider
    {
        private static readonly HttpClient DefaultHttpClient = new HttpClient();

        // Cache for OAuthClient to speed up OAuth operations
        // _oAuthClients is a cache using [appId + oAuthCredentialAppId]
        private readonly ConcurrentDictionary<string, OAuthClient> _oAuthClients = new ConcurrentDictionary<string, OAuthClient>();
        private readonly HttpClient _httpClient;
        private readonly AppCredentials _appCredentials;

        // Cache for appCredentials to speed up token acquisition (a token is not requested unless is expired)
        // AppCredentials are cached using appId + skillId (this last parameter is only used if the app credentials are used to call a skill)
        private readonly ConcurrentDictionary<string, AppCredentials> _appCredentialMap = new ConcurrentDictionary<string, AppCredentials>();

        public UserTokenProvider(
            ICredentialProvider credentialProvider,
            IChannelProvider channelProvider,
            HttpClient customHttpClient,
            ILogger logger)
        {
            CredentialProvider = credentialProvider ?? throw new ArgumentNullException(nameof(credentialProvider));
            ChannelProvider = channelProvider;
            _httpClient = customHttpClient ?? DefaultHttpClient;
            Logger = logger ?? NullLogger.Instance;
        }

        public UserTokenProvider(
            AppCredentials credentials,
            IChannelProvider channelProvider,
            HttpClient customHttpClient,
            ILogger logger)
        {
            _appCredentials = credentials ?? throw new ArgumentNullException(nameof(credentials));
            ChannelProvider = channelProvider;
            _httpClient = customHttpClient ?? DefaultHttpClient;
            Logger = logger ?? NullLogger.Instance;
        }

        /// <summary>
        /// Gets the credential provider for this adapter.
        /// </summary>
        /// <value>
        /// The credential provider for this adapter.
        /// </value>
        protected ICredentialProvider CredentialProvider { get; private set; }

        /// <summary>
        /// Gets the channel provider for this adapter.
        /// </summary>
        /// <value>
        /// The channel provider for this adapter.
        /// </value>
        protected IChannelProvider ChannelProvider { get; private set; }

        /// <summary>
        /// Gets the logger for this adapter.
        /// </summary>
        /// <value>
        /// The logger for this adapter.
        /// </value>
        protected ILogger Logger { get; private set; }

        /// <summary>
        /// Gets the custom <see cref="HttpClient"/> for this adapter if specified.
        /// </summary>
        /// <value>
        /// The custom <see cref="HttpClient"/> for this adapter if specified.
        /// </value>
        protected HttpClient HttpClient { get => _httpClient; }

        /// <summary>
        /// Attempts to retrieve the token for a user that's in a login flow, using customized AppCredentials.
        /// </summary>
        /// <param name="turnContext">Context for the current turn of conversation with the user.</param>
        /// <param name="oAuthAppCredentials">AppCredentials for OAuth.</param>
        /// <param name="connectionName">Name of the auth connection to use.</param>
        /// <param name="magicCode">(Optional) Optional user entered code to validate.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Token Response.</returns>
        public virtual async Task<TokenResponse> GetUserTokenAsync(ITurnContext turnContext, AppCredentials oAuthAppCredentials, string connectionName, string magicCode, CancellationToken cancellationToken = default)
        {
            BotAssert.ContextNotNull(turnContext);
            if (turnContext.Activity.From == null || string.IsNullOrWhiteSpace(turnContext.Activity.From.Id))
            {
                throw new ArgumentNullException($"{nameof(BotFrameworkAdapter)}.{nameof(GetUserTokenAsync)}(): missing from or from.id");
            }

            if (string.IsNullOrWhiteSpace(connectionName))
            {
                throw new ArgumentNullException(nameof(connectionName));
            }

            var client = await CreateOAuthApiClientAsync(turnContext, oAuthAppCredentials).ConfigureAwait(false);
            return await client.UserToken.GetTokenAsync(turnContext.Activity.From.Id, connectionName, turnContext.Activity.ChannelId, magicCode, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Attempts to retrieve the token for a user that's in a login flow, using the bot's AppCredentials.
        /// </summary>
        /// <param name="turnContext">Context for the current turn of conversation with the user.</param>
        /// <param name="connectionName">Name of the auth connection to use.</param>
        /// <param name="magicCode">(Optional) Optional user entered code to validate.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Token Response.</returns>
        public virtual async Task<TokenResponse> GetUserTokenAsync(ITurnContext turnContext, string connectionName, string magicCode, CancellationToken cancellationToken = default)
        {
            return await GetUserTokenAsync(turnContext, null, connectionName, magicCode, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Get the raw signin link to be sent to the user for signin for a connection name, using customized AppCredentials.
        /// </summary>
        /// <param name="turnContext">Context for the current turn of conversation with the user.</param>
        /// <param name="oAuthAppCredentials">AppCredentials for OAuth.</param>
        /// <param name="connectionName">Name of the auth connection to use.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <remarks>If the task completes successfully, the result contains the raw signin link.</remarks>
        public virtual async Task<string> GetOauthSignInLinkAsync(ITurnContext turnContext, AppCredentials oAuthAppCredentials, string connectionName, CancellationToken cancellationToken = default)
        {
            BotAssert.ContextNotNull(turnContext);
            if (string.IsNullOrWhiteSpace(connectionName))
            {
                throw new ArgumentNullException(nameof(connectionName));
            }

            var activity = turnContext.Activity;
            var appId = GetBotAppId(turnContext);
            var tokenExchangeState = new TokenExchangeState()
            {
                ConnectionName = connectionName,
                Conversation = new ConversationReference()
                {
                    ActivityId = activity.Id,
                    Bot = activity.Recipient,       // Activity is from the user to the bot
                    ChannelId = activity.ChannelId,
                    Conversation = activity.Conversation,
                    Locale = activity.Locale,
                    ServiceUrl = activity.ServiceUrl,
                    User = activity.From,
                },
                RelatesTo = activity.RelatesTo,
                MsAppId = appId,
            };

            var serializedState = JsonConvert.SerializeObject(tokenExchangeState);
            var encodedState = Encoding.UTF8.GetBytes(serializedState);
            var state = Convert.ToBase64String(encodedState);

            var client = await CreateOAuthApiClientAsync(turnContext, oAuthAppCredentials).ConfigureAwait(false);
            return await client.BotSignIn.GetSignInUrlAsync(state, null, null, null, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Get the raw signin link to be sent to the user for signin for a connection name, using the bot's AppCredentials.
        /// </summary>
        /// <param name="turnContext">Context for the current turn of conversation with the user.</param>
        /// <param name="connectionName">Name of the auth connection to use.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <remarks>If the task completes successfully, the result contains the raw signin link.</remarks>
        public virtual async Task<string> GetOauthSignInLinkAsync(ITurnContext turnContext, string connectionName, CancellationToken cancellationToken = default)
        {
            return await GetOauthSignInLinkAsync(turnContext, null, connectionName, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Get the raw signin link to be sent to the user for signin for a connection name, using customized AppCredentials.
        /// </summary>
        /// <param name="turnContext">Context for the current turn of conversation with the user.</param>
        /// <param name="oAuthAppCredentials">AppCredentials for OAuth.</param>
        /// <param name="connectionName">Name of the auth connection to use.</param>
        /// <param name="userId">The user id that will be associated with the token.</param>
        /// <param name="finalRedirect">The final URL that the OAuth flow will redirect to.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <remarks>If the task completes successfully, the result contains the raw signin link.</remarks>
        public virtual async Task<string> GetOauthSignInLinkAsync(ITurnContext turnContext, AppCredentials oAuthAppCredentials, string connectionName, string userId, string finalRedirect = null, CancellationToken cancellationToken = default)
        {
            BotAssert.ContextNotNull(turnContext);

            if (string.IsNullOrWhiteSpace(connectionName))
            {
                throw new ArgumentNullException(nameof(connectionName));
            }

            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new ArgumentNullException(nameof(userId));
            }

            var activity = turnContext.Activity;

            var appId = GetBotAppId(turnContext);
            var tokenExchangeState = new TokenExchangeState()
            {
                ConnectionName = connectionName,
                Conversation = new ConversationReference()
                {
                    ActivityId = activity.Id,
                    Bot = activity.Recipient,       // Activity is from the user to the bot
                    ChannelId = activity.ChannelId,
                    Conversation = activity.Conversation,
                    Locale = activity.Locale,
                    ServiceUrl = activity.ServiceUrl,
                    User = activity.From,
                },
                RelatesTo = activity.RelatesTo,
                MsAppId = appId,
            };

            var serializedState = JsonConvert.SerializeObject(tokenExchangeState);
            var encodedState = Encoding.UTF8.GetBytes(serializedState);
            var state = Convert.ToBase64String(encodedState);

            var client = await CreateOAuthApiClientAsync(turnContext, oAuthAppCredentials).ConfigureAwait(false);
            return await client.BotSignIn.GetSignInUrlAsync(state, null, null, finalRedirect, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Get the raw signin link to be sent to the user for signin for a connection name, using the bot's AppCredentials.
        /// </summary>
        /// <param name="turnContext">Context for the current turn of conversation with the user.</param>
        /// <param name="connectionName">Name of the auth connection to use.</param>
        /// <param name="userId">The user id that will be associated with the token.</param>
        /// <param name="finalRedirect">The final URL that the OAuth flow will redirect to.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <remarks>If the task completes successfully, the result contains the raw signin link.</remarks>
        public virtual async Task<string> GetOauthSignInLinkAsync(ITurnContext turnContext, string connectionName, string userId, string finalRedirect = null, CancellationToken cancellationToken = default)
        {
            return await GetOauthSignInLinkAsync(turnContext, null, connectionName, userId, finalRedirect, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Signs the user out with the token server, using customized AppCredentials.
        /// </summary>
        /// <param name="turnContext">Context for the current turn of conversation with the user.</param>
        /// <param name="oAuthAppCredentials">AppCredentials for OAuth.</param>
        /// <param name="connectionName">Name of the auth connection to use.</param>
        /// <param name="userId">User id of user to sign out.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        public virtual async Task SignOutUserAsync(ITurnContext turnContext, AppCredentials oAuthAppCredentials, string connectionName = null, string userId = null, CancellationToken cancellationToken = default)
        {
            BotAssert.ContextNotNull(turnContext);

            if (string.IsNullOrEmpty(userId))
            {
                userId = turnContext.Activity?.From?.Id;
            }

            var client = await CreateOAuthApiClientAsync(turnContext, oAuthAppCredentials).ConfigureAwait(false);
            await client.UserToken.SignOutAsync(userId, connectionName, turnContext.Activity?.ChannelId, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Signs the user out with the token server, using the bot's AppCredentials.
        /// </summary>
        /// <param name="turnContext">Context for the current turn of conversation with the user.</param>
        /// <param name="connectionName">Name of the auth connection to use.</param>
        /// <param name="userId">User id of user to sign out.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        public virtual async Task SignOutUserAsync(ITurnContext turnContext, string connectionName = null, string userId = null, CancellationToken cancellationToken = default)
        {
            await SignOutUserAsync(turnContext, null, connectionName, userId, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Retrieves the token status for each configured connection for the given user, using customized AppCredentials.
        /// </summary>
        /// <param name="context">Context for the current turn of conversation with the user.</param>
        /// <param name="oAuthAppCredentials">AppCredentials for OAuth.</param>
        /// <param name="userId">The user Id for which token status is retrieved.</param>
        /// <param name="includeFilter">Optional comma separated list of connection's to include. Blank will return token status for all configured connections.</param>
        /// <param name="cancellationToken">The async operation cancellation token.</param>
        /// <returns>Array of TokenStatus.</returns>
        public virtual async Task<TokenStatus[]> GetTokenStatusAsync(ITurnContext context, AppCredentials oAuthAppCredentials, string userId, string includeFilter = null, CancellationToken cancellationToken = default)
        {
            BotAssert.ContextNotNull(context);

            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new ArgumentNullException(nameof(userId));
            }

            var client = await CreateOAuthApiClientAsync(context, oAuthAppCredentials).ConfigureAwait(false);
            var result = await client.UserToken.GetTokenStatusAsync(userId, context.Activity?.ChannelId, includeFilter, cancellationToken).ConfigureAwait(false);
            return result?.ToArray();
        }

        /// <summary>
        /// Retrieves the token status for each configured connection for the given user, using the bot's AppCredentials.
        /// </summary>
        /// <param name="context">Context for the current turn of conversation with the user.</param>
        /// <param name="userId">The user Id for which token status is retrieved.</param>
        /// <param name="includeFilter">Optional comma separated list of connection's to include. Blank will return token status for all configured connections.</param>
        /// <param name="cancellationToken">The async operation cancellation token.</param>
        /// <returns>Array of TokenStatus.</returns>
        public virtual async Task<TokenStatus[]> GetTokenStatusAsync(ITurnContext context, string userId, string includeFilter = null, CancellationToken cancellationToken = default)
        {
            return await GetTokenStatusAsync(context, null, userId, includeFilter, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Retrieves Azure Active Directory tokens for particular resources on a configured connection, using customized AppCredentials.
        /// </summary>
        /// <param name="context">Context for the current turn of conversation with the user.</param>
        /// <param name="oAuthAppCredentials">AppCredentials for OAuth.</param>
        /// <param name="connectionName">The name of the Azure Active Directory connection configured with this bot.</param>
        /// <param name="resourceUrls">The list of resource URLs to retrieve tokens for.</param>
        /// <param name="userId">The user Id for which tokens are retrieved. If passing in null the userId is taken from the Activity in the ITurnContext.</param>
        /// <param name="cancellationToken">The async operation cancellation token.</param>
        /// <returns>Dictionary of resourceUrl to the corresponding TokenResponse.</returns>
        public virtual async Task<Dictionary<string, TokenResponse>> GetAadTokensAsync(ITurnContext context, AppCredentials oAuthAppCredentials, string connectionName, string[] resourceUrls, string userId = null, CancellationToken cancellationToken = default)
        {
            BotAssert.ContextNotNull(context);

            if (string.IsNullOrWhiteSpace(connectionName))
            {
                throw new ArgumentNullException(nameof(connectionName));
            }

            if (resourceUrls == null)
            {
                throw new ArgumentNullException(nameof(resourceUrls));
            }

            if (string.IsNullOrWhiteSpace(userId))
            {
                userId = context.Activity?.From?.Id;
            }

            var client = await CreateOAuthApiClientAsync(context, oAuthAppCredentials).ConfigureAwait(false);
            return (Dictionary<string, TokenResponse>)await client.UserToken.GetAadTokensAsync(userId, connectionName, new AadResourceUrls() { ResourceUrls = resourceUrls?.ToList() }, context.Activity?.ChannelId, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Get the raw signin link to be sent to the user for signin for a connection name.
        /// </summary>
        /// <param name="turnContext">Context for the current turn of conversation with the user.</param>
        /// <param name="connectionName">Name of the auth connection to use.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <remarks>If the task completes successfully, the result contains the raw signin link.</remarks>
        public virtual Task<SignInResource> GetSignInResourceAsync(ITurnContext turnContext, string connectionName, CancellationToken cancellationToken = default(CancellationToken))
        {
            return GetSignInResourceAsync(turnContext, connectionName, turnContext.Activity.From.Id, null, cancellationToken);
        }

        /// <summary>
        /// Get the raw signin link to be sent to the user for signin for a connection name.
        /// </summary>
        /// <param name="turnContext">Context for the current turn of conversation with the user.</param>
        /// <param name="connectionName">Name of the auth connection to use.</param>
        /// <param name="userId">The user id that will be associated with the token.</param>
        /// <param name="finalRedirect">The final URL that the OAuth flow will redirect to.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <remarks>If the task completes successfully, the result contains the raw signin link.</remarks>
        public virtual Task<SignInResource> GetSignInResourceAsync(ITurnContext turnContext, string connectionName, string userId, string finalRedirect = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return GetSignInResourceAsync(turnContext, null, connectionName, userId, finalRedirect, cancellationToken);
        }

        /// <summary>
        /// Get the raw signin link to be sent to the user for signin for a connection name.
        /// </summary>
        /// <param name="turnContext">Context for the current turn of conversation with the user.</param>
        /// <param name="oAuthAppCredentials">AppCredentials for OAuth.</param>
        /// <param name="connectionName">Name of the auth connection to use.</param>
        /// <param name="userId">The user id that will be associated with the token.</param>
        /// <param name="finalRedirect">The final URL that the OAuth flow will redirect to.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <remarks>If the task completes successfully, the result contains the raw signin link.</remarks>
        public virtual async Task<SignInResource> GetSignInResourceAsync(ITurnContext turnContext, AppCredentials oAuthAppCredentials, string connectionName, string userId, string finalRedirect = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            BotAssert.ContextNotNull(turnContext);

            if (string.IsNullOrWhiteSpace(connectionName))
            {
                throw new ArgumentNullException(nameof(connectionName));
            }

            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new ArgumentNullException(nameof(userId));
            }

            var activity = turnContext.Activity;

            var appId = GetBotAppId(turnContext);
            var tokenExchangeState = new TokenExchangeState()
            {
                ConnectionName = connectionName,
                Conversation = new ConversationReference()
                {
                    ActivityId = activity.Id,
                    Bot = activity.Recipient,       // Activity is from the user to the bot
                    ChannelId = activity.ChannelId,
                    Conversation = activity.Conversation,
                    Locale = activity.Locale,
                    ServiceUrl = activity.ServiceUrl,
                    User = activity.From,
                },
                RelatesTo = activity.RelatesTo,
                MsAppId = appId,
            };

            var serializedState = JsonConvert.SerializeObject(tokenExchangeState);
            var encodedState = Encoding.UTF8.GetBytes(serializedState);
            var state = Convert.ToBase64String(encodedState);

            var client = await CreateOAuthApiClientAsync(turnContext, oAuthAppCredentials).ConfigureAwait(false);
            return await client.GetSignInResourceAsync(state, null, null, finalRedirect, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Performs a token exchange operation such as for single sign-on.
        /// </summary>
        /// <param name="turnContext">Context for the current turn of conversation with the user.</param>
        /// <param name="connectionName">Name of the auth connection to use.</param>
        /// <param name="userId">The user id associated with the token..</param>
        /// <param name="exchangeRequest">The exchange request details, either a token to exchange or a uri to exchange.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>If the task completes, the exchanged token is returned.</returns>
        public virtual Task<TokenResponse> ExchangeTokenAsync(ITurnContext turnContext, string connectionName, string userId, TokenExchangeRequest exchangeRequest, CancellationToken cancellationToken = default(CancellationToken))
        {
            return ExchangeTokenAsync(turnContext, null, connectionName, userId, exchangeRequest, cancellationToken);
        }

        /// <summary>
        /// Performs a token exchange operation such as for single sign-on.
        /// </summary>
        /// <param name="turnContext">Context for the current turn of conversation with the user.</param>
        /// <param name="oAuthAppCredentials">AppCredentials for OAuth.</param>
        /// <param name="connectionName">Name of the auth connection to use.</param>
        /// <param name="userId">The user id associated with the token..</param>
        /// <param name="exchangeRequest">The exchange request details, either a token to exchange or a uri to exchange.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>If the task completes, the exchanged token is returned.</returns>
        public virtual async Task<TokenResponse> ExchangeTokenAsync(ITurnContext turnContext, AppCredentials oAuthAppCredentials, string connectionName, string userId, TokenExchangeRequest exchangeRequest, CancellationToken cancellationToken = default)
        {
            BotAssert.ContextNotNull(turnContext);

            if (string.IsNullOrWhiteSpace(connectionName))
            {
                LogAndThrowException(new ArgumentException($"{nameof(connectionName)} is null or empty", nameof(connectionName)));
            }

            if (string.IsNullOrWhiteSpace(userId))
            {
                LogAndThrowException(new ArgumentException($"{nameof(userId)} is null or empty", nameof(userId)));
            }

            if (exchangeRequest == null)
            {
                LogAndThrowException(new ArgumentException($"{nameof(exchangeRequest)} is null or empty", nameof(exchangeRequest)));
            }

            if (string.IsNullOrWhiteSpace(exchangeRequest.Token) && string.IsNullOrWhiteSpace(exchangeRequest.Uri))
            {
                LogAndThrowException(new ArgumentException("Either a Token or Uri property is required on the TokenExchangeRequest", nameof(exchangeRequest)));
            }

            var activity = turnContext.Activity;

            var client = await CreateOAuthApiClientAsync(turnContext, oAuthAppCredentials).ConfigureAwait(false);
            var result = await client.ExchangeAsyncAsync(userId, connectionName, turnContext.Activity.ChannelId, exchangeRequest, cancellationToken).ConfigureAwait(false);

            if (result is ErrorResponse errorResponse)
            {
                LogAndThrowException(new InvalidOperationException($"Unable to exchange token: ({errorResponse?.Error?.Code}) {errorResponse?.Error?.Message}"));
            }

            if (result is TokenResponse tokenResponse)
            {
                return tokenResponse;
            }

            LogAndThrowException(new InvalidOperationException($"ExchangeAsyncAsync returned improper result: {result.GetType()}"));

            // even though LogAndThrowException always throws, compiler gives an error about not all code paths returning a value.
            return null;
        }

        /// <summary>
        /// Retrieves Azure Active Directory tokens for particular resources on a configured connection, using the bot's AppCredentials.
        /// </summary>
        /// <param name="context">Context for the current turn of conversation with the user.</param>
        /// <param name="connectionName">The name of the Azure Active Directory connection configured with this bot.</param>
        /// <param name="resourceUrls">The list of resource URLs to retrieve tokens for.</param>
        /// <param name="userId">The user Id for which tokens are retrieved. If passing in null the userId is taken from the Activity in the ITurnContext.</param>
        /// <param name="cancellationToken">The async operation cancellation token.</param>
        /// <returns>Dictionary of resourceUrl to the corresponding TokenResponse.</returns>
        public virtual async Task<Dictionary<string, TokenResponse>> GetAadTokensAsync(ITurnContext context, string connectionName, string[] resourceUrls, string userId = null, CancellationToken cancellationToken = default)
        {
            return await GetAadTokensAsync(context, null, connectionName, resourceUrls, userId, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Creates an OAuth client for the bot with the credentials.
        /// </summary>
        /// <param name="turnContext">The context object for the current turn.</param>
        /// <param name="oAuthAppCredentials">AppCredentials for OAuth.</param>
        /// <returns>An OAuth client for the bot.</returns>
        protected virtual async Task<OAuthClient> CreateOAuthApiClientAsync(ITurnContext turnContext, AppCredentials oAuthAppCredentials)
        {
            if (!OAuthClientConfig.EmulateOAuthCards &&
                string.Equals(turnContext.Activity.ChannelId, Channels.Emulator, StringComparison.OrdinalIgnoreCase) &&
                (await CredentialProvider.IsAuthenticationDisabledAsync().ConfigureAwait(false)))
            {
                OAuthClientConfig.EmulateOAuthCards = true;
            }

            var appId = GetBotAppId(turnContext);

            var clientKey = $"{appId}:{oAuthAppCredentials?.MicrosoftAppId}";
            var oAuthScope = GetBotFrameworkOAuthScope();

            var appCredentials = oAuthAppCredentials ?? await GetAppCredentialsAsync(appId, oAuthScope).ConfigureAwait(false);

            if (!OAuthClientConfig.EmulateOAuthCards &&
                string.Equals(turnContext.Activity.ChannelId, Channels.Emulator, StringComparison.OrdinalIgnoreCase) &&
                (await CredentialProvider.IsAuthenticationDisabledAsync().ConfigureAwait(false)))
            {
                OAuthClientConfig.EmulateOAuthCards = true;
            }

            var oAuthClient = _oAuthClients.GetOrAdd(clientKey, (key) =>
            {
                OAuthClient oAuthClientInner;
                if (OAuthClientConfig.EmulateOAuthCards)
                {
                    // do not await task - we want this to run in the background
                    oAuthClientInner = new OAuthClient(new Uri(turnContext.Activity.ServiceUrl), appCredentials);
                    var task = Task.Run(() => OAuthClientConfig.SendEmulateOAuthCardsAsync(oAuthClientInner, OAuthClientConfig.EmulateOAuthCards));
                }
                else
                {
                    oAuthClientInner = new OAuthClient(new Uri(OAuthClientConfig.OAuthEndpoint), appCredentials);
                }

                return oAuthClientInner;
            });

            // adding the oAuthClient into the TurnState
            // TokenResolver.cs will use it get the correct credentials to poll for token for streaming scenario
            if (turnContext.TurnState.Get<OAuthClient>() == null)
            {
                turnContext.TurnState.Add(oAuthClient);
            }

            return oAuthClient;
        }

        /// <summary>
        /// Logic to build an <see cref="AppCredentials"/> object to be used to acquire tokens
        /// for this HttpClient.
        /// </summary>
        /// <param name="appId">The application id.</param>
        /// <param name="oAuthScope">The optional OAuth scope.</param>
        /// <returns>The app credentials to be used to acquire tokens.</returns>
        protected virtual async Task<AppCredentials> BuildCredentialsAsync(string appId, string oAuthScope = null)
        {
            // Get the password from the credential provider
            var appPassword = await CredentialProvider.GetAppPasswordAsync(appId).ConfigureAwait(false);

            // Construct an AppCredentials using the app + password combination. If government, we create a government specific credential.
            return ChannelProvider != null && ChannelProvider.IsGovernment() ? new MicrosoftGovernmentAppCredentials(appId, appPassword, HttpClient, Logger, oAuthScope) : new MicrosoftAppCredentials(appId, appPassword, HttpClient, Logger, oAuthScope);
        }

        /// <summary>
        /// Creates an OAuth client for the bot.
        /// </summary>
        /// <param name="turnContext">The context object for the current turn.</param>
        /// <returns>An OAuth client for the bot.</returns>
        protected virtual async Task<OAuthClient> CreateOAuthApiClientAsync(ITurnContext turnContext)
        {
            return await CreateOAuthApiClientAsync(turnContext, null).ConfigureAwait(false);
        }

        /// <summary>
        /// Gets the AppId of the Bot out of the TurnState.
        /// </summary>
        /// <param name="turnContext">The context object for the turn.</param>
        /// <returns>Bot's AppId.</returns>
        private static string GetBotAppId(ITurnContext turnContext)
        {
            var botIdentity = (ClaimsIdentity)turnContext.TurnState.Get<IIdentity>(BotAdapter.BotIdentityKey);
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
        /// <returns>App credentials.</returns>
        private async Task<AppCredentials> GetAppCredentialsAsync(string appId, string oAuthScope)
        {
            if (string.IsNullOrWhiteSpace(appId))
            {
                return MicrosoftAppCredentials.Empty;
            }

            var cacheKey = $"{appId}{oAuthScope}";
            if (_appCredentialMap.TryGetValue(cacheKey, out var appCredentials))
            {
                return appCredentials;
            }

            // If app credentials were provided, use them as they are the preferred choice moving forward
            if (_appCredentials != null)
            {
                // Cache the credentials for later use
                _appCredentialMap[cacheKey] = _appCredentials;
                return _appCredentials;
            }

            // Credentials not found in cache, build them
            appCredentials = await BuildCredentialsAsync(appId, oAuthScope).ConfigureAwait(false);

            // Cache the credentials for later use
            _appCredentialMap[cacheKey] = appCredentials;
            return appCredentials;
        }

        /// <summary>
        /// This method returns the correct Bot Framework OAuthScope for AppCredentials.
        /// </summary>
        private string GetBotFrameworkOAuthScope()
        {
            return ChannelProvider != null && ChannelProvider.IsGovernment() ?
                GovernmentAuthenticationConstants.ToChannelFromBotOAuthScope :
                AuthenticationConstants.ToChannelFromBotOAuthScope;
        }

        /// <summary>
        /// Logs and throws an exception.
        /// </summary>
        /// <param name="ex"> Exception instance to throw.</param>
        /// <param name="source"> Source method for the exception.</param>
        private void LogAndThrowException(Exception ex, string source = "ExchangeTokenAsync")
        {
            Logger.LogError(ex, source);
            throw ex;
        }
    }
}
