using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder
{
    internal class TokenResolver
    {
        private const int PollingInterval = 1000;   // Poll for token every 1 second.

        public static void CheckForOAuthCards(BotFrameworkAdapter adapter, ITurnContext turnContext, Activity activity, CancellationToken cancellationToken)
        {
            if (activity?.Attachments != null)
            {
                foreach (var attachment in activity?.Attachments?.Where(a => a.ContentType == OAuthCard.ContentType))
                {
                    OAuthCard oauthCard = attachment.Content as OAuthCard;
                    if (oauthCard != null && !string.IsNullOrEmpty(oauthCard.ConnectionName))
                    {
                        // Poll as a background task
                        Task.Run(() => PollForTokenAsync(adapter, turnContext, activity, oauthCard.ConnectionName, cancellationToken));
                    }
                }
            }
        }

        private static async Task PollForTokenAsync(BotFrameworkAdapter adapter, ITurnContext turnContext, Activity activity, string connectionName, CancellationToken cancellationToken)
        {
            TokenResponse tokenResponse = null;
            bool shouldEndPolling = false;
            int pollingTimeout = TurnStateConstants.OAuthLoginTimeoutMsValue;
            int pollingRequestsInterval = PollingInterval;
            var loginTimeout = turnContext?.TurnState?.Get<object>(TurnStateConstants.OAuthLoginTimeoutKey) as int?;

            // Override login timeout with value set from the OAuthPrompt or by the developer
            if (loginTimeout != null)
            {
                pollingTimeout = (int)loginTimeout;
            }

            if (activity.From == null || string.IsNullOrWhiteSpace(activity.From.Id))
            {
                throw new ArgumentNullException("TokenResolver.PollForTokenAsync: missing Activity from or from.id");
            }

            if (string.IsNullOrWhiteSpace(connectionName))
            {
                throw new ArgumentNullException(nameof(connectionName));
            }

            var stopwatch = Stopwatch.StartNew();

            while (stopwatch.Elapsed < TimeSpan.FromMilliseconds(pollingTimeout) && !shouldEndPolling)
            {
                tokenResponse = await adapter.GetUserTokenAsync(turnContext, connectionName, null, cancellationToken).ConfigureAwait(false);

                if (tokenResponse != null)
                {
                    // This can be used to short-circuit the polling loop.
                    if (tokenResponse.Properties != null)
                    {
                        JToken tokenPollingSettingsToken = null;
                        TokenPollingSettings tokenPollingSettings = null;
                        tokenResponse.Properties.TryGetValue(TurnStateConstants.TokenPollingSettingsKey, out tokenPollingSettingsToken);

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

                    // once there is a token, send it to the bot and stop polling
                    if (tokenResponse.Token != null)
                    {
                        var tokenResponseActivityEvent = CreateTokenResponse(turnContext.Activity.GetConversationReference(), tokenResponse.Token, connectionName);
                        var identity = turnContext.TurnState.Get<IIdentity>(BotFrameworkAdapter.BotIdentityKey) as ClaimsIdentity;
                        var callback = turnContext.TurnState.Get<BotCallbackHandler>();
                        await adapter.ProcessActivityAsync(identity, tokenResponseActivityEvent, callback, cancellationToken).ConfigureAwait(false);
                        shouldEndPolling = true;
                    }
                }

                if (!shouldEndPolling)
                {
                    await Task.Delay(pollingRequestsInterval).ConfigureAwait(false);
                }
            }

            stopwatch.Stop();
        }

        private static Activity CreateTokenResponse(ConversationReference relatesTo, string token, string connectionName)
        {
            var tokenResponse = Activity.CreateEventActivity() as Activity;

            // IActivity properties
            tokenResponse.Id = Guid.NewGuid().ToString();
            tokenResponse.Timestamp = DateTime.UtcNow;
            tokenResponse.From = relatesTo.User;
            tokenResponse.Recipient = relatesTo.Bot;
            tokenResponse.ReplyToId = relatesTo.ActivityId;
            tokenResponse.ServiceUrl = relatesTo.ServiceUrl;
            tokenResponse.ChannelId = relatesTo.ChannelId;
            tokenResponse.Conversation = relatesTo.Conversation;
            tokenResponse.Attachments = new List<Attachment>().ToArray();
            tokenResponse.Entities = new List<Entity>().ToArray();

            // IEventActivity properties
            tokenResponse.Name = "tokens/response";
            tokenResponse.RelatesTo = relatesTo;
            tokenResponse.Value = new TokenResponse()
            {
                Token = token,
                ConnectionName = connectionName,
            };

            return tokenResponse;
        }
    }
}
