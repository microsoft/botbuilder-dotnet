// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;

namespace Microsoft.Bot.Builder
{
    internal static class TokenResolver
    {
        private static readonly TimeSpan PollingInterval = TimeSpan.FromSeconds(1);   // Poll for token every 1 second.

        /// <summary>
        /// Inspects outgoing Activities for <see cref="OAuthCard">OAuthCards</see>.
        /// </summary>
        /// <param name="adapter">The BotFrameworkAdapter used for polling the token service.</param>
        /// <param name="logger">The ILogger implementation this TokenResolver should use.</param>
        /// <param name="turnContext">The context object for the turn.</param>
        /// <param name="activity">The activity to send.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <remarks>If an <see cref="OAuthCard"/> is found in an outgoing activity, the <see cref="TokenResolver"/> polls the Bot Framework Token Service in the background.
        /// When the user completes the login flow, the TokenResolver will retrieve the user's token from the service and create a <see cref="SignInConstants.TokenResponseEventName">TokenResponse</see> activity to "send" to the bot, mimicking non-streaming OAuth flows.
        /// <para />
        /// All bots using OAuth should query the service to ensure the user is successfully logged in before utilizing a user's token. The bot should never store the user's token.
        /// </remarks>
        public static void CheckForOAuthCards(BotFrameworkAdapter adapter, ILogger logger, ITurnContext turnContext, Activity activity, CancellationToken cancellationToken)
        {
            if (activity?.Attachments == null)
            {
                return;
            }

            var pollTokenTasks = new List<Task>();
            foreach (var attachment in activity.Attachments.Where(a => a.ContentType == OAuthCard.ContentType))
            {
                if (attachment.Content is OAuthCard oauthCard)
                {
                    if (string.IsNullOrWhiteSpace(oauthCard.ConnectionName))
                    {
                        throw new InvalidOperationException("The OAuthPrompt's ConnectionName property is missing a value.");
                    }

                    pollTokenTasks.Add(PollForTokenAsync(adapter, logger, turnContext, oauthCard.ConnectionName, cancellationToken));
                }
            }

            if (pollTokenTasks.Any())
            {
                // Run the poll operations in the background.
                // On retrieving a token from the token service the TokenResolver creates an Activity to route the token to the bot to continue the conversation.
                // If these Tasks are awaited and the user doesn't complete the login flow, the bot may timeout in sending its response to the channel which can cause the streaming connection to disconnect.
                Task.WhenAll(pollTokenTasks.ToArray());
            }
        }

        private static async Task PollForTokenAsync(BotFrameworkAdapter adapter, ILogger logger, ITurnContext turnContext, string connectionName, CancellationToken cancellationToken)
        {
            try
            {
                var shouldEndPolling = false;
                var pollingTimeout = TurnStateConstants.OAuthLoginTimeoutValue;
                var pollingRequestsInterval = PollingInterval;
                var sentToken = false;

                // Override login timeout with value set from the OAuthPrompt or by the developer
                if (turnContext.TurnState.ContainsKey(TurnStateConstants.OAuthLoginTimeoutKey))
                {
                    pollingTimeout = (TimeSpan)turnContext.TurnState.Get<object>(TurnStateConstants.OAuthLoginTimeoutKey);
                }

                var stopwatch = Stopwatch.StartNew();
                var oauthClient = turnContext.TurnState.Get<OAuthClient>();

                while (stopwatch.Elapsed < pollingTimeout && !shouldEndPolling)
                {
                    var tokenResponse = await adapter.GetUserTokenAsync(turnContext, oauthClient?.Credentials as AppCredentials, connectionName, null, cancellationToken).ConfigureAwait(false);

                    if (tokenResponse != null)
                    {
                        // This can be used to short-circuit the polling loop.
                        if (tokenResponse.Properties != null)
                        {
                            tokenResponse.Properties.TryGetValue(TurnStateConstants.TokenPollingSettingsKey, out var tokenPollingSettingsToken);

                            var tokenPollingSettings = tokenPollingSettingsToken?.ToObject<TokenPollingSettings>();
                            if (tokenPollingSettings != null)
                            {
                                logger.LogInformation($"PollForTokenAsync received new polling settings: timeout={tokenPollingSettings.Timeout}, interval={tokenPollingSettings.Interval}", tokenPollingSettings);
                                shouldEndPolling = tokenPollingSettings.Timeout <= 0 ? true : shouldEndPolling; // Timeout now and stop polling
                                pollingRequestsInterval = tokenPollingSettings.Interval > 0 ? TimeSpan.FromMilliseconds(tokenPollingSettings.Interval) : pollingRequestsInterval; // Only overrides if it is set.
                            }
                        }

                        // once there is a token, send it to the bot and stop polling
                        if (tokenResponse.Token != null)
                        {
                            var tokenResponseActivityEvent = CreateTokenResponse(turnContext.Activity.GetConversationReference(), tokenResponse.Token, connectionName);
                            var identity = turnContext.TurnState.Get<IIdentity>(BotAdapter.BotIdentityKey) as ClaimsIdentity;
                            var callback = turnContext.TurnState.Get<BotCallbackHandler>();
                            await adapter.ProcessActivityAsync(identity, tokenResponseActivityEvent, callback, cancellationToken).ConfigureAwait(false);
                            shouldEndPolling = true;
                            sentToken = true;

                            logger.LogInformation("PollForTokenAsync completed with a token", turnContext.Activity);
                        }
                    }

                    if (!shouldEndPolling)
                    {
                        await Task.Delay(pollingRequestsInterval, cancellationToken).ConfigureAwait(false);
                    }
                }

                if (!sentToken)
                {
                    logger.LogInformation("PollForTokenAsync completed without receiving a token", turnContext.Activity);
                }

                stopwatch.Stop();
            }
#pragma warning disable CA1031 // Do not catch general exception types (for now we just log the exception and continue)
            catch (Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types
            {
                logger.LogError(ex, "PollForTokenAsync threw an exception", connectionName);
            }
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
            tokenResponse.Name = SignInConstants.TokenResponseEventName;
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
