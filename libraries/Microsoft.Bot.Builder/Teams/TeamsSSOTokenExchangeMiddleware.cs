// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Concurrent;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Teams
{
    /// <summary>
    /// This middleware is designed to intercept incoming 'signin/tokenExchange' activities,
    /// perform the token exchange operation, and in case of duplicated requests, the bot will only process one of them.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Duplicated requests:</b> when a user is signed into multiple MS Teams clients, the Bot will receive a 'signin/tokenExchange' activity from each client,
    /// causing the next conversation step to be executed more than once.<br/>
    /// When this behavior happens, the middleware will detect any duplicated request, 
    /// executing <see cref="NextDelegate"/> inside <see cref="OnTurnAsync"/> method once, to continue the to next middleware.<br/>
    /// <i>NOTE: when receiving this type of request, the 'signin/tokenExchange' activity will contain the same Value.Id.</i>
    /// </para>
    /// <para>
    /// <b>Token exchange:</b> when a user is signed into multiple MS Teams clients, the token exchange will be done to each client,
    /// ensuring that if the token could not be exchanged (which could be due to a consent requirement),
    /// the bot will notify the sender with a <see cref="HttpStatusCode.PreconditionFailed"/> so they can respond accordingly.
    /// </para>
    /// </remarks>
    public class TeamsSSOTokenExchangeMiddleware : IMiddleware
    {
        private readonly string _oAuthConnectionName;

        private readonly ConcurrentDictionary<string, (string, int)> _users = new ConcurrentDictionary<string, (string, int)>();

        /// <summary>
        /// Initializes a new instance of the <see cref="TeamsSSOTokenExchangeMiddleware"/> class.
        /// </summary>
        /// <param name="storage">The <see cref="IStorage"/> to use for deduplication.</param>
        /// <param name="connectionName">The connection name to use for the single
        /// sign on token exchange.</param>
        public TeamsSSOTokenExchangeMiddleware(IStorage storage, string connectionName)
            : this(connectionName)
        {
            // We don't mark this as obsolete, in case we may need a storage for something else in the future.
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TeamsSSOTokenExchangeMiddleware"/> class.
        /// </summary>
        /// <param name="connectionName">The connection name to use for the single
        /// sign on token exchange.</param>
        public TeamsSSOTokenExchangeMiddleware(string connectionName)
        {
            if (string.IsNullOrEmpty(connectionName))
            {
                throw new ArgumentNullException(nameof(connectionName));
            }

            _oAuthConnectionName = connectionName;
        }

        /// <inheritdoc/>
        public async Task OnTurnAsync(ITurnContext turnContext, NextDelegate next, CancellationToken cancellationToken = default)
        {
            if (await ProcessActivityAsync(turnContext, cancellationToken).ConfigureAwait(false))
            {
                await next(cancellationToken).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Evaluates if the incoming activity is from MSTeams channel and if it's a 'signin/tokenExchange' activity.
        /// </summary>
        /// <param name="turnContext">The context object for this turn.</param>
        /// <returns>True if the activity is from MS Teams and it's a token exchange request.</returns>
        private bool IsTokenExchangeActivity(ITurnContext turnContext)
        {
            return string.Equals(Channels.Msteams, turnContext.Activity.ChannelId, StringComparison.OrdinalIgnoreCase)
                && string.Equals(SignInConstants.TokenExchangeOperationName, turnContext.Activity.Name, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Evaluates if the incoming activity is a 'signin/tokenExchange' activity,
        /// it processes the token exchange operation and in case of duplicated requests,
        /// it will return true on the first one, whereas the rest will return false and send an invoke response to notify the bot.
        /// </summary>
        /// <param name="turnContext">The context object for this turn.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>False if the token couldn't be exchanged or if the method received a duplicated 'signin/tokenExchange' activity, otherwise True.</returns>
        private async Task<bool> ProcessActivityAsync(ITurnContext turnContext, CancellationToken cancellationToken = default)
        {
            if (!IsTokenExchangeActivity(turnContext))
            {
                return true;
            }

            // Add the user to a thread-safe collection to avoid duplicated requests of the same user with equal or different MS Teams session id.
            // The item in the collection will live in the collection for a short period of time, only in the ProcessActivityAsync scope, and then it will be removed.
            // Additionally, the item in the collection keeps track of the number of requests + the token 'sid (Session ID)' claim.
            var key = CreateKey(turnContext);
            var tokenExchangeRequest = ((JObject)turnContext.Activity.Value)?.ToObject<TokenExchangeInvokeRequest>() ?? throw new InvalidOperationException("Invalid 'signin/tokenExchange' operation. Missing TokenExchangeInvokeRequest object in Activity.Value property.");
            var token = new JwtSecurityToken(tokenExchangeRequest.Token);
            var sessionId = token.Payload.Claims.FirstOrDefault(claim => claim.Type == "sid")?.Value ?? throw new InvalidOperationException("Invalid 'signin/tokenExchange' operation. Missing 'sid' claim in the Activity.Value.Token property.");
            var tracker = _users.AddOrUpdate(key, _ => (sessionId, 1), (_, val) => (val.Item1, val.Item2 + 1));

            // Exchange the token.
            if (!await ExchangeTokenAsync(turnContext, cancellationToken))
            {
                return false;
            }

            // Delay processing the user from the collection to capture a burst of incoming duplicated requests over a small period of time. 
            await Task.Delay(500, cancellationToken).ConfigureAwait(false);

            // Capture the first request that reached this point, and mark it as processed, the rest will be ignored.
            var processed = false;
            tracker = _users.AddOrUpdate(key, _ => (string.Empty, 0), (_, val) =>
            {
                processed = val.Item1 == sessionId;
                var id = processed ? $"{val.Item1}/processed" : val.Item1;
                return (id, val.Item2 - 1);
            });

            if (System.Diagnostics.Debugger.IsAttached)
            {
                Console.WriteLine($"key={key}, processed={processed}, session:processed={tracker.Item1}, session:incoming={sessionId}, session:count={tracker.Item2}");
            }

            if (!processed)
            {
                // Notify the sender that the request is duplicated. Send 200 invoke response.
                await SendInvokeResponseAsync(turnContext, cancellationToken: cancellationToken).ConfigureAwait(false);
            }

            // Remove the user from the collection to free up memory.
            if (tracker.Item2 <= 0)
            {
                _users.TryRemove(key, out _);
            }

            return processed;
        }

        /// <summary>
        /// Sends an invoke response to the caller.
        /// </summary>
        /// <param name="turnContext">The context object for this turn.</param>
        /// <param name="body">The body response to send. Defaults to null.</param>
        /// <param name="httpStatusCode">The HTTP status code to send. Defaults to <see cref="HttpStatusCode.OK"/>.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A void task.</returns>
        private async Task SendInvokeResponseAsync(ITurnContext turnContext, object body = null, HttpStatusCode httpStatusCode = HttpStatusCode.OK, CancellationToken cancellationToken = default)
        {
            var activity = new Activity
            {
                Type = ActivityTypesEx.InvokeResponse,
                Value = new InvokeResponse { Status = (int)httpStatusCode, Body = body },
            };
            await turnContext.SendActivityAsync(activity, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// It performs the token exchange operation based on the token exchange request activity for a specific connection.
        /// </summary>
        /// <param name="turnContext">The context object for this turn.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>False if the token couldn't be exchanged, otherwise True.</returns>
        private async Task<bool> ExchangeTokenAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            TokenResponse tokenExchangeResponse = null;
            var tokenExchangeRequest = ((JObject)turnContext.Activity.Value)?.ToObject<TokenExchangeInvokeRequest>();

            try
            {
                var userTokenClient = turnContext.TurnState.Get<UserTokenClient>();
                if (userTokenClient != null)
                {
                    tokenExchangeResponse = await userTokenClient.ExchangeTokenAsync(
                        turnContext.Activity.From.Id,
                        _oAuthConnectionName,
                        turnContext.Activity.ChannelId,
                        new TokenExchangeRequest { Token = tokenExchangeRequest.Token },
                        cancellationToken).ConfigureAwait(false);
                }
                else if (turnContext.Adapter is IExtendedUserTokenProvider adapter)
                {
                    tokenExchangeResponse = await adapter.ExchangeTokenAsync(
                        turnContext,
                        _oAuthConnectionName,
                        turnContext.Activity.From.Id,
                        new TokenExchangeRequest { Token = tokenExchangeRequest.Token },
                        cancellationToken).ConfigureAwait(false);
                }
                else
                {
                    throw new NotSupportedException("Token Exchange is not supported by the current adapter.");
                }
            }
#pragma warning disable CA1031 // Do not catch general exception types (ignoring, see comment below)
            catch
#pragma warning restore CA1031 // Do not catch general exception types
            {
                // Ignore Exceptions
                // If token exchange failed for any reason, tokenExchangeResponse above stays null,
                // and hence we send back a failure invoke response to the caller.
            }

            if (string.IsNullOrEmpty(tokenExchangeResponse?.Token))
            {
                // The token could not be exchanged (which could be due to a consent requirement)
                // Notify the sender that PreconditionFailed so they can respond accordingly.

                var invokeResponse = new TokenExchangeInvokeResponse
                {
                    Id = tokenExchangeRequest.Id,
                    ConnectionName = _oAuthConnectionName,
                    FailureDetail = "The bot is unable to exchange token. Proceed with regular login.",
                };

                await SendInvokeResponseAsync(turnContext, invokeResponse, HttpStatusCode.PreconditionFailed, cancellationToken).ConfigureAwait(false);

                return false;
            }

            return true;
        }

        /// <summary>
        /// Creates a key based on the channel, conversation and user id.
        /// </summary>
        /// <param name="turnContext">The context object for this turn.</param>
        /// <returns>The generated key for a specific user.</returns>
        /// <exception cref="InvalidOperationException">If any of the ChannelId, Conversation.Id and From.Id are missing.</exception>
        private string CreateKey(ITurnContext turnContext)
        {
            var channelId = turnContext.Activity.ChannelId ?? throw new InvalidOperationException("Invalid Activity, missing ChannelId property.");
            var conversationId = turnContext.Activity.Conversation?.Id ?? throw new InvalidOperationException("Invalid Activity, missing Conversation.Id property.");
            var userId = turnContext.Activity.From?.Id ?? throw new InvalidOperationException("Invalid Activity, missing From.Id property.");
            return $"{channelId}/conversations/{conversationId}/users/{userId}";
        }
    }
}
