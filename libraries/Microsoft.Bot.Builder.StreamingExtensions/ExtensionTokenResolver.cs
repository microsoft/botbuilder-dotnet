using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.StreamingExtensions
{
    public class ExtensionTokenResolver : ITokenResolver
    {
        private readonly int _pollingTimeout = 900000; // Default is 900,000 milliseconds (15 minutes) as in the OAuthPrompt
        private readonly int _pollingRequestsInterval = 1000; // Poll for token every 1 second.
        private readonly BotFrameworkStreamingExtensionsAdapter _botFrameworkStreamingExtensionsAdapter;
        private readonly IBot _bot;

        public ExtensionTokenResolver(BotFrameworkStreamingExtensionsAdapter botFrameworkStreamingExtensionsAdapter, IBot bot)
        {
            _botFrameworkStreamingExtensionsAdapter = botFrameworkStreamingExtensionsAdapter;
            _bot = bot;
        }

        public async Task CheckForOAuthCard(ITurnContext turnContext, Activity activity, CancellationToken cancellationToken)
        {
            OAuthClient oAuthClient = await _botFrameworkStreamingExtensionsAdapter.GetOAuthApiClientAsync(turnContext).ConfigureAwait(false);

            if (activity?.Attachments != null)
            {
                foreach (var attachment in activity?.Attachments?.Where(a => a.ContentType == OAuthCard.ContentType))
                {
                    var oAuthCard = ExtensionTokenResolverHelper.FindOAuthCard(attachment);
                    var firstSignInButton = oAuthCard?.Buttons?.FirstOrDefault(b => b.Type == ActionTypes.Signin);
                    firstSignInButton.Value = await _botFrameworkStreamingExtensionsAdapter.GetOauthSignInLinkAsync(turnContext, oAuthCard.ConnectionName, cancellationToken);
                    StartPollingForToken(turnContext, activity, oAuthCard.ConnectionName, null, oAuthClient, cancellationToken);
                }
            }
        }

        public async Task<string> GetSignInUrl(ITurnContext turnContext, OAuthClient oAuthClient, string connectionName, string msAppId, CancellationToken cancellationToken)
        {
            BotAssert.ContextNotNull(turnContext);
            if (string.IsNullOrWhiteSpace(connectionName))
            {
                throw new ArgumentNullException(nameof(connectionName));
            }

            var activity = turnContext.Activity;

            var tokenExchangeState = new TokenExchangeState()
            {
                ConnectionName = connectionName,
                Conversation = ExtensionTokenResolverHelper.GetConversationReference(turnContext),
                MsAppId = msAppId,
                IsFromStreaming = true,
            };

            var serializedState = JsonConvert.SerializeObject(tokenExchangeState);
            var encodedState = Encoding.UTF8.GetBytes(serializedState);
            var state = Convert.ToBase64String(encodedState);

            return await oAuthClient.BotSignIn.GetSignInUrlAsync(state, null, null, null, cancellationToken).ConfigureAwait(false);
        }

        private async Task StartPollingForToken(ITurnContext turnContext, Activity activity, string connectionName, string magicCode, OAuthClient oAuthClient, CancellationToken cancellationToken)
        {
            TokenResponse tokenResponse = null;
            bool shouldEndPolling = false;
            int pollingTimeout = this._pollingTimeout;
            int pollingRequestsInterval = this._pollingRequestsInterval;
            var loginTimeout = turnContext?.TurnState?.Get<object>(LoginTimeout.Key);

            // Override login timeout with value set from the OAuthPrompt or by the developer
            if (loginTimeout != null)
            {
                pollingTimeout = (int)loginTimeout;
            }

            BotAssert.ContextNotNull(turnContext);

            if (activity == null)
            {
                throw new ArgumentNullException($"BotFrameworkStreamingExtensionsAdapter.StartPollingForToken(): getting response activity by {ExtensionTokenResolverHelper.InvokeResponseKey} not found.");
            }

            if (activity.From == null || string.IsNullOrWhiteSpace(activity.From.Id))
            {
                throw new ArgumentNullException("BotFrameworkStreamingExtensionsAdapter.GetUserTokenAsync(): missing from or from.id");
            }

            if (string.IsNullOrWhiteSpace(connectionName))
            {
                throw new ArgumentNullException(nameof(connectionName));
            }

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            while (stopwatch.Elapsed < TimeSpan.FromMilliseconds(pollingTimeout) && !shouldEndPolling)
            {
                tokenResponse = await oAuthClient.UserToken.GetTokenAsync(turnContext.Activity.From.Id, connectionName, activity.ChannelId, magicCode, cancellationToken).ConfigureAwait(false);

                if (tokenResponse != null)
                {
                    // This can be used to short-circuit the polling loop.
                    if (tokenResponse.Properties != null)
                    {
                        JToken tokenPollingSettingsToken = null;
                        TokenPollingSettings tokenPollingSettings = null;
                        var tokenPollingSettingsKey = nameof(TokenPollingSettings);
                        tokenPollingSettingsKey = char.ToLower(tokenPollingSettingsKey[0]) + tokenPollingSettingsKey.Substring(1);
                        tokenResponse.Properties.TryGetValue(tokenPollingSettingsKey, out tokenPollingSettingsToken);

                        if (tokenPollingSettingsToken != null)
                        {
                            tokenPollingSettings = tokenPollingSettingsToken.ToObject<TokenPollingSettings>();

                            if (tokenPollingSettings != null)
                            {
                                shouldEndPolling = tokenPollingSettings.Timeout <= 0 ? true : shouldEndPolling; // Timeout now and stop polling
                                pollingRequestsInterval = tokenPollingSettings.Interval > 0 ? tokenPollingSettings.Interval : pollingRequestsInterval; // Only overrides if it is set.
                            }
                        }
                    }

                    // if there is token, send it to the bot
                    if (tokenResponse.Token != null)
                    {
                        var tokenResponseActivityEvent = ExtensionTokenResolverHelper.CreateTokenResponse(ExtensionTokenResolverHelper.GetConversationReference(turnContext), tokenResponse.Token, connectionName);
                        await this._botFrameworkStreamingExtensionsAdapter?.ContinueConversationAsync(turnContext.Activity.Recipient.Id, ExtensionTokenResolverHelper.GetConversationReference(turnContext), new BotCallbackHandler(_bot.OnTurnAsync), cancellationToken);
                        shouldEndPolling = true;
                    }
                }

                if (!shouldEndPolling)
                {
                    await Task.Delay(pollingRequestsInterval);
                }
            }

            stopwatch.Stop();
        }
    }
}
