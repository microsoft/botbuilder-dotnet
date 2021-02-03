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
        public static void CheckForOAuthCards(BotAdapter adapter, ILogger logger, ITurnContext turnContext, Activity activity, CancellationToken cancellationToken)
        {
            if (activity?.Attachments == null)
            {
                return;
            }

            var pollingTimeout = TurnStateConstants.OAuthLoginTimeoutValue;

            // Override login timeout with value set from the OAuthPrompt or by the developer
            if (turnContext.TurnState.ContainsKey(TurnStateConstants.OAuthLoginTimeoutKey))
            {
                pollingTimeout = (TimeSpan)turnContext.TurnState.Get<object>(TurnStateConstants.OAuthLoginTimeoutKey);
            }

            var identity = turnContext.TurnState.Get<IIdentity>(BotAdapter.BotIdentityKey) as ClaimsIdentity;
            var callback = turnContext.TurnState.Get<BotCallbackHandler>();
            var pollingHelper = new PollingHelper()
            {
                Activity = turnContext.Activity,
                Adapter = adapter,
                DefaultPollingInterval = PollingInterval,
                DefaultPollingTimeout = pollingTimeout,
                CancellationToken = cancellationToken,
                Identity = identity,
                Logger = logger,
                Callback = callback
            };

            var pollTokenTasks = new List<Task>();
            foreach (var attachment in activity.Attachments.Where(a => a.ContentType == OAuthCard.ContentType))
            {
                if (attachment.Content is OAuthCard oauthCard)
                {
                    if (string.IsNullOrWhiteSpace(oauthCard.ConnectionName))
                    {
                        throw new InvalidOperationException("The OAuthPrompt's ConnectionName property is missing a value.");
                    }

                    pollTokenTasks.Add(PollForTokenAsync(pollingHelper, oauthCard.ConnectionName));
                }
            }

            if (pollTokenTasks.Any())
            {
                // Run the poll operations in the background.
                // On retrieving a token from the token service the TokenResolver creates an Activity to route the token to the bot to continue the conversation.
                // If these Tasks are awaited and the user doesn't complete the login flow, the bot may timeout in sending its response to the channel which can cause the streaming connection to disconnect.
#pragma warning disable VSTHRD110 // Observe result of async calls
                Task.WhenAll(pollTokenTasks.ToArray());
#pragma warning restore VSTHRD110 // Observe result of async calls
            }
        }

        private static async Task PollForTokenAsync(PollingHelper pollingHelper, string connectionName)
        {
            try
            {
                var pollingParams = new PollingParams()
                {
                    ConnectionName = connectionName,
                    PollingInterval = pollingHelper.DefaultPollingInterval,
                    PollingTimeout = pollingHelper.DefaultPollingTimeout,
                };

                var stopwatch = Stopwatch.StartNew();

                while (stopwatch.Elapsed < pollingParams.PollingTimeout && !pollingParams.ShouldEndPolling)
                {
                    await pollingHelper.PollForTokenAsync(pollingParams).ConfigureAwait(false);

                    if (!pollingParams.ShouldEndPolling)
                    {
                        await Task.Delay(pollingParams.PollingInterval, pollingHelper.CancellationToken).ConfigureAwait(false);
                    }
                }

                if (!pollingParams.SentToken)
                {
                    pollingHelper.Logger.LogInformation("PollForTokenAsync completed without receiving a token", pollingHelper.Activity);
                }

                stopwatch.Stop();
            }
#pragma warning disable CA1031 // Do not catch general exception types (for now we just log the exception and continue)
            catch (Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types
            {
                pollingHelper.Logger.LogError(ex, "PollForTokenAsync threw an exception", connectionName);
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

        private class PollingParams
        {
            public bool SentToken { get; set; }

            public bool ShouldEndPolling { get; set; }

            public TimeSpan PollingTimeout { get; set; }

            public TimeSpan PollingInterval { get; set; }

            public string ConnectionName { get; set; }
        }

        private class PollingHelper
        {
            public TimeSpan DefaultPollingInterval { get; set; }

            public TimeSpan DefaultPollingTimeout { get; set; }

            public CancellationToken CancellationToken { get; set; }

            public BotAdapter Adapter { get; set; }

            public ILogger Logger { get; set; }

            public Activity Activity { get; set; }

            public ClaimsIdentity Identity { get; set; }

            public BotCallbackHandler Callback { get; set; }

            public async Task PollForTokenAsync(PollingParams pollingParams)
            {
                BotCallbackHandler continueCallback = async (context, ctoken) =>
                {
                    // TODO: Should be using OAuthClient
                    //var oauthClient = context.TurnState.Get<OAuthClient>();
                    //var tokenResponse = await oauthClient.UserToken.GetTokenAsync(Activity.From.Id, pollingParams.ConnectionName, Activity.ChannelId, null, ctoken).ConfigureAwait(false);
                    var tokenResponse = await (Adapter as BotFrameworkAdapter).GetUserTokenAsync(context, pollingParams.ConnectionName, null, ctoken).ConfigureAwait(false);

                    if (tokenResponse != null)
                    {
                        // This can be used to short-circuit the polling loop.
                        if (tokenResponse.Properties != null)
                        {
                            tokenResponse.Properties.TryGetValue(TurnStateConstants.TokenPollingSettingsKey, out var tokenPollingSettingsToken);

                            var tokenPollingSettings = tokenPollingSettingsToken?.ToObject<TokenPollingSettings>();
                            if (tokenPollingSettings != null)
                            {
                                Logger.LogInformation($"PollForTokenAsync received new polling settings: timeout={tokenPollingSettings.Timeout}, interval={tokenPollingSettings.Interval}", tokenPollingSettings);
                                pollingParams.ShouldEndPolling = tokenPollingSettings.Timeout <= 0 ? true : pollingParams.ShouldEndPolling; // Timeout now and stop polling
                                pollingParams.PollingInterval = tokenPollingSettings.Interval > 0 ? TimeSpan.FromMilliseconds(tokenPollingSettings.Interval) : pollingParams.PollingInterval; // Only overrides if it is set.
                            }
                        }

                        // once there is a token, send it to the bot and stop polling
                        if (tokenResponse.Token != null)
                        {
                            var tokenResponseActivityEvent = CreateTokenResponse(Activity.GetConversationReference(), tokenResponse.Token, pollingParams.ConnectionName);
                            await Adapter.ProcessActivityAsync(Identity, tokenResponseActivityEvent, Callback, ctoken).ConfigureAwait(false);
                            pollingParams.ShouldEndPolling = true;
                            pollingParams.SentToken = true;

                            Logger.LogInformation("PollForTokenAsync completed with a token", Activity);
                        }
                    }
                };

                await Adapter.ContinueConversationAsync(Identity, Activity.GetConversationReference(), continueCallback, cancellationToken: CancellationToken).ConfigureAwait(false);
            }
        }
    }
}
