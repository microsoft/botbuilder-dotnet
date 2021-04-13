// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Teams
{
    /// <summary>
    /// If the activity name is signin/tokenExchange, this middleware will attempt to
    /// exchange the token, and deduplicate the incoming call, ensuring only one
    /// exchange request is processed.
    /// </summary>
    /// <remarks>
    /// If a user is signed into multiple Teams clients, the Bot could receive a
    /// "signin/tokenExchange" from each client. Each token exchange request for a
    /// specific user login will have an identical Activity.Value.Id.
    /// 
    /// Only one of these token exchange requests should be processed by the bot.
    /// The others return <see cref="System.Net.HttpStatusCode.PreconditionFailed"/>.
    /// For a distributed bot in production, this requires a distributed storage
    /// ensuring only one token exchange is processed. This middleware supports
    /// CosmosDb storage found in Microsoft.Bot.Builder.Azure, or MemoryStorage for
    /// local development. IStorage's ETag implementation for token exchange activity
    /// deduplication.
    /// </remarks>
    public class TeamsSSOTokenExchangeMiddleware : IMiddleware
    {
        private readonly IStorage _storage;
        private readonly string _oAuthConnectionName;

        /// <summary>
        /// Initializes a new instance of the <see cref="TeamsSSOTokenExchangeMiddleware"/> class.
        /// </summary>
        /// <param name="storage">The <see cref="IStorage"/> to use for deduplication.</param>
        /// <param name="connectionName">The connection name to use for the single
        /// sign on token exchange.</param>
        public TeamsSSOTokenExchangeMiddleware(IStorage storage, string connectionName)
        {
            if (storage == null)
            {
                throw new ArgumentNullException(nameof(storage));
            }

            if (string.IsNullOrEmpty(connectionName))
            {
                throw new ArgumentNullException(nameof(connectionName));
            }

            _oAuthConnectionName = connectionName;
            _storage = storage;
        }

        /// <inheritdoc/>
        public async Task OnTurnAsync(ITurnContext turnContext, NextDelegate next, CancellationToken cancellationToken = default)
        {
            if (turnContext.Activity.Name == SignInConstants.TokenExchangeOperationName)
            {
                // If the TokenExchange is NOT successful, the response will have already been sent by ExchangedTokenAsync
                if (!await this.ExchangedTokenAsync(turnContext, cancellationToken).ConfigureAwait(false))
                {
                    return;
                }

                // Only one token exchange should proceed from here. Deduplication is performed second because in the case
                // of failure due to consent required, every caller needs to receive the 
                if (!await DeduplicatedTokenExchangeIdAsync(turnContext, cancellationToken).ConfigureAwait(false))
                {
                    // If the token is not exchangeable, do not process this activity further.
                    return;
                }
            }

            await next(cancellationToken).ConfigureAwait(false);
        }

        private async Task<bool> DeduplicatedTokenExchangeIdAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            // Create a StoreItem with Etag of the unique 'signin/tokenExchange' request
            var storeItem = new TokenStoreItem
            {
                ETag = (turnContext.Activity.Value as JObject).Value<string>("id")
            };

            var storeItems = new Dictionary<string, object> { { TokenStoreItem.GetStorageKey(turnContext), storeItem } };
            try
            {
                // Writing the IStoreItem with ETag of unique id will succeed only once
                await _storage.WriteAsync(storeItems, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)

                // Memory storage throws a generic exception with a Message of 'Etag conflict. [other error info]'
                // CosmosDbPartitionedStorage throws: ex.Message.Contains("pre-condition is not met")
                when (ex.Message.StartsWith("Etag conflict", StringComparison.OrdinalIgnoreCase) || ex.Message.Contains("pre-condition is not met"))
            {
                // Do NOT proceed processing this message, some other thread or machine already has processed it.

                // Send 200 invoke response.
                await SendInvokeResponseAsync(turnContext, cancellationToken).ConfigureAwait(false);
                return false;
            }

            return true;
        }

        private async Task SendInvokeResponseAsync(ITurnContext turnContext, object body = null, HttpStatusCode httpStatusCode = HttpStatusCode.OK, CancellationToken cancellationToken = default)
        {
            await turnContext.SendActivityAsync(
                new Activity
                {
                    Type = ActivityTypesEx.InvokeResponse,
                    Value = new InvokeResponse
                    {
                        Status = (int)httpStatusCode,
                        Body = body,
                    },
                }, cancellationToken).ConfigureAwait(false);
        }

        private async Task<bool> ExchangedTokenAsync(ITurnContext turnContext, CancellationToken cancellationToken)
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

        private class TokenStoreItem : IStoreItem
        {
            public string ETag { get; set; }

            public static string GetStorageKey(ITurnContext turnContext)
            {
                var activity = turnContext.Activity;
                var channelId = activity.ChannelId ?? throw new InvalidOperationException("invalid activity-missing channelId");
                var conversationId = activity.Conversation?.Id ?? throw new InvalidOperationException("invalid activity-missing Conversation.Id");

                var value = activity.Value as JObject;
                if (value == null || !value.ContainsKey("id"))
                {
                    throw new InvalidOperationException("Invalid signin/tokenExchange. Missing activity.Value.Id.");
                }

                return $"{channelId}/{conversationId}/{value.Value<string>("id")}";
            }
        }
    }
}
