// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.OAuth
{
    /// <summary>
    /// Used for properly constructing, sending and recognizing responses to User OAuth flows.
    /// </summary>
    public class OAuthCardProvider
    {
        private const string PersistedCaller = "caller";
        private readonly OAuthSettings _settings;

        /// <summary>
        /// Initializes a new instance of the <see cref="OAuthCardProvider"/> class.
        /// </summary>
        public OAuthCardProvider() 
        { 
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OAuthCardProvider"/> class.
        /// </summary>
        /// <param name="settings">Settings specific to this <see cref="OAuthCardProvider"/>.</param>
        public OAuthCardProvider(OAuthSettings settings) 
        {
            _settings = settings ?? throw new NullReferenceException(nameof(settings));
        }

        /// <summary>
        /// Shared implementation of the SetCallerInfoInState function. This is intended for internal use, to
        /// consolidate the implementation of the OAuthPrompt and OAuthInput. Application logic should use
        /// those dialog classes.
        /// </summary>
        /// <param name="state">The dialog state.</param>
        /// <param name="context">ITurnContext.</param>
        public static void SetCallerInfoInState(IDictionary<string, object> state, ITurnContext context)
        {
            state[PersistedCaller] = CreateCallerInfo(context);
        }

        /// <summary>
        /// Useful for determining if an activity is an Azure Bot Service response to an OAuthCard.
        /// </summary>
        /// <param name="activity">The activity to check the type and name of.</param>
        /// <returns>True if the activity is of type event with name of tokens/response or an invoke
        /// with name of signin/verifyState or signin/tokenExchange.</returns>
        public static bool IsOAuthResponseActivity(Activity activity)
        {
            return (activity.Type == ActivityTypes.Event && activity.Name == SignInConstants.TokenResponseEventName)
                || (activity.Type == ActivityTypes.Invoke && activity.Name == SignInConstants.VerifyStateOperationName)
                || (activity.Type == ActivityTypes.Invoke && activity.Name == SignInConstants.TokenExchangeOperationName);
        }

        /// <summary>
        /// This method will properly construct an OAuthCard based on <see cref="OAuthSettings"/>
        /// provided during class construction, or from the <paramref name="prompt"/> if present.
        /// </summary>
        /// <param name="turnContext"><see cref="ITurnContext"/> to use for constructing and sending the OAuthCard.</param>
        /// <param name="settings"><see cref="OAuthSettings"/> to use while constructing the OAuthCard.</param>
        /// <param name="prompt"><see cref="IMessageActivity"/> to use for sending the OAuthCard. If this activity 
        /// does not have an attachment containing an OAuthCard, then one will be added.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to use for async operations from this method.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task SendOAuthCardAsync(ITurnContext turnContext, OAuthSettings settings = default(OAuthSettings), IMessageActivity prompt = default(IMessageActivity), CancellationToken cancellationToken = default(CancellationToken))
        {
            BotAssert.ContextNotNull(turnContext);
            settings = settings ?? _settings ?? throw new NullReferenceException(nameof(settings));

            // Ensure prompt initialized
            prompt ??= Activity.CreateMessageActivity();

            if (prompt.Attachments == null)
            {
                prompt.Attachments = new List<Attachment>();
            }

            // Append appropriate card if missing
            if (!ChannelSupportsOAuthCard(turnContext.Activity.ChannelId))
            {
                if (!prompt.Attachments.Any(a => a.Content is SigninCard))
                {
                    var signInResource = await GetSignInResourceAsync(turnContext, settings, cancellationToken).ConfigureAwait(false);
                    prompt.Attachments.Add(new Attachment
                    {
                        ContentType = SigninCard.ContentType,
                        Content = new SigninCard
                        {
                            Text = settings.Text,
                            Buttons = new[]
                            {
                                new CardAction
                                {
                                    Title = settings.Title,
                                    Value = signInResource.SignInLink,
                                    Type = ActionTypes.Signin,
                                },
                            },
                        },
                    });
                }
            }
            else if (!prompt.Attachments.Any(a => a.Content is OAuthCard))
            {
                var cardActionType = ActionTypes.Signin;
                var signInResource = await GetSignInResourceAsync(turnContext, settings, cancellationToken).ConfigureAwait(false);
                var value = signInResource.SignInLink;

                // use the SignInLink when 
                //   in speech channel or
                //   bot is a skill or
                //   an extra OAuthAppCredentials is being passed in
                if (turnContext.Activity.IsFromStreamingConnection() ||
                    (turnContext.TurnState.Get<ClaimsIdentity>(BotAdapter.BotIdentityKey) is ClaimsIdentity botIdentity && botIdentity.Claims.IsSkillClaim()) ||
                    settings.OAuthAppCredentials != null)
                {
                    if (turnContext.Activity.ChannelId == Channels.Emulator)
                    {
                        cardActionType = ActionTypes.OpenUrl;
                    }
                }
                else if ((settings.ShowSignInLink != null && settings.ShowSignInLink == false) ||
                    (settings.ShowSignInLink == null && !ChannelRequiresSignInLink(turnContext.Activity.ChannelId)))
                {
                    value = null;
                }

                prompt.Attachments.Add(new Attachment
                {
                    ContentType = OAuthCard.ContentType,
                    Content = new OAuthCard
                    {
                        Text = settings.Text,
                        ConnectionName = settings.ConnectionName,
                        Buttons = new[]
                        {
                            new CardAction
                            {
                                Title = settings.Title,
                                Text = settings.Text,
                                Type = cardActionType,
                                Value = value
                            },
                        },
                        TokenExchangeResource = signInResource.TokenExchangeResource,
                    },
                });
            }

            // Add the login timeout specified in OAuthPromptSettings to TurnState so it can be referenced if polling is needed
            if (!turnContext.TurnState.ContainsKey(TurnStateConstants.OAuthLoginTimeoutKey) && settings.Timeout.HasValue)
            {
                turnContext.TurnState.Add<object>(TurnStateConstants.OAuthLoginTimeoutKey, TimeSpan.FromMilliseconds(settings.Timeout.Value));
            }

            // Set input hint
            if (string.IsNullOrEmpty(prompt.InputHint))
            {
                prompt.InputHint = InputHints.AcceptingInput;
            }

            await turnContext.SendActivityAsync(prompt, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Determine if the currently processing activity is the final step in the User Auth flow, and
        /// respond appropriately by sending an InvokeResponse if required, performing a token exchange,
        /// or validating an incoming magic code and retrieving/returning the TokenResponse.
        /// </summary>
        /// <param name="turnContext"><see cref="ITurnContext"/> to use for finalizing the user OAuth flow and
        /// returning the appropriate TokenResponse, or sending the correct invoke response.</param>
        /// <param name="settings"><see cref="OAuthSettings"/>To use while retrieving the token response.</param>
        /// <param name="state">Parent dialog state, which might contain CallerInfo if it had previously been set.</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/> to use for this async operation.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<TokenResponse> RecognizeTokenAsync(ITurnContext turnContext, OAuthSettings settings = default(OAuthSettings), IDictionary<string, object> state = default(IDictionary<string, object>), CancellationToken cancellationToken = default(CancellationToken))
        {
            settings = settings ?? _settings ?? throw new NullReferenceException(nameof(settings));
            TokenResponse result = null;
            if (IsTokenResponseEvent(turnContext))
            {
                var tokenResponseObject = turnContext.Activity.Value as JObject;
                result = tokenResponseObject?.ToObject<TokenResponse>();

                // fixup the turnContext's state context if this was received from a skill host caller
                if (state[PersistedCaller] is CallerInfo callerInfo)
                {
                    // set the ServiceUrl to the skill host's Url
                    turnContext.Activity.ServiceUrl = callerInfo.CallerServiceUrl;

                    // recreate a ConnectorClient and set it in TurnState so replies use the correct one
                    var serviceUrl = turnContext.Activity.ServiceUrl;
                    var claimsIdentity = turnContext.TurnState.Get<ClaimsIdentity>(BotAdapter.BotIdentityKey);
                    var audience = callerInfo.Scope;
                    var connectorClient = await CreateConnectorClientAsync(turnContext, serviceUrl, claimsIdentity, audience, cancellationToken).ConfigureAwait(false);
                    if (turnContext.TurnState.Get<IConnectorClient>() != null)
                    {
                        turnContext.TurnState.Set(connectorClient);
                    }
                    else
                    {
                        turnContext.TurnState.Add(connectorClient);
                    }
                }
            }
            else if (IsTeamsVerificationInvoke(turnContext))
            {
                var magicCodeObject = turnContext.Activity.Value as JObject;
                var magicCode = magicCodeObject.GetValue("state", StringComparison.Ordinal)?.ToString();

                // Getting the token follows a different flow in Teams. At the signin completion, Teams
                // will send the bot an "invoke" activity that contains a "magic" code. This code MUST
                // then be used to try fetching the token from Botframework service within some time
                // period. We try here. If it succeeds, we return 200 with an empty body. If it fails
                // with a retriable error, we return 500. Teams will re-send another invoke in this case.
                // If it fails with a non-retriable error, we return 404. Teams will not (still work in
                // progress) retry in that case.
                try
                {
                    result = await GetUserTokenAsync(turnContext, settings, magicCode, cancellationToken).ConfigureAwait(false);

                    if (result != null)
                    {
                        await turnContext.SendActivityAsync(new Activity { Type = ActivityTypesEx.InvokeResponse }, cancellationToken).ConfigureAwait(false);
                    }
                    else
                    {
                        await SendInvokeResponseAsync(turnContext, HttpStatusCode.NotFound, null, cancellationToken).ConfigureAwait(false);
                    }
                }
#pragma warning disable CA1031 // Do not catch general exception types (ignoring exception for now and send internal server error, see comment above)
                catch
#pragma warning restore CA1031 // Do not catch general exception types
                {
                    await SendInvokeResponseAsync(turnContext, HttpStatusCode.InternalServerError, null, cancellationToken).ConfigureAwait(false);
                }
            }
            else if (IsTokenExchangeRequestInvoke(turnContext))
            {
                var tokenExchangeRequest = ((JObject)turnContext.Activity.Value)?.ToObject<TokenExchangeInvokeRequest>();

                if (tokenExchangeRequest == null)
                {
                    await SendInvokeResponseAsync(
                        turnContext,
                        HttpStatusCode.BadRequest,
                        new TokenExchangeInvokeResponse
                        {
                            Id = null,
                            ConnectionName = settings.ConnectionName,
                            FailureDetail = "The bot received an InvokeActivity that is missing a TokenExchangeInvokeRequest value. This is required to be sent with the InvokeActivity.",
                        }, cancellationToken).ConfigureAwait(false);
                }
                else if (tokenExchangeRequest.ConnectionName != settings.ConnectionName)
                {
                    await SendInvokeResponseAsync(
                        turnContext,
                        HttpStatusCode.BadRequest,
                        new TokenExchangeInvokeResponse
                        {
                            Id = tokenExchangeRequest.Id,
                            ConnectionName = settings.ConnectionName,
                            FailureDetail = "The bot received an InvokeActivity with a TokenExchangeInvokeRequest containing a ConnectionName that does not match the ConnectionName expected by the bot's active OAuthPrompt. Ensure these names match when sending the InvokeActivityInvalid ConnectionName in the TokenExchangeInvokeRequest",
                        }, cancellationToken).ConfigureAwait(false);
                }
                else
                {
                    TokenResponse tokenExchangeResponse = null;
                    try
                    {
                        tokenExchangeResponse = await ExchangeTokenAsync(turnContext, settings, new TokenExchangeRequest { Token = tokenExchangeRequest.Token }, cancellationToken).ConfigureAwait(false);
                    }
#pragma warning disable CA1031 // Do not catch general exception types (ignoring, see comment below)
                    catch
#pragma warning restore CA1031 // Do not catch general exception types
                    {
                        // Ignore Exceptions
                        // If token exchange failed for any reason, tokenExchangeResponse above stays null , and hence we send back a failure invoke response to the caller.
                        // This ensures that the caller shows 
                    }

                    if (tokenExchangeResponse == null || string.IsNullOrEmpty(tokenExchangeResponse.Token))
                    {
                        await SendInvokeResponseAsync(
                            turnContext,
                            HttpStatusCode.PreconditionFailed,
                            new TokenExchangeInvokeResponse
                            {
                                Id = tokenExchangeRequest.Id,
                                ConnectionName = settings.ConnectionName,
                                FailureDetail = "The bot is unable to exchange token. Proceed with regular login.",
                            }, cancellationToken).ConfigureAwait(false);
                    }
                    else
                    {
                        await SendInvokeResponseAsync(
                            turnContext,
                            HttpStatusCode.OK,
                            new TokenExchangeInvokeResponse
                            {
                                Id = tokenExchangeRequest.Id,
                                ConnectionName = settings.ConnectionName,
                            }, cancellationToken).ConfigureAwait(false);

                        result = new TokenResponse
                        {
                            ChannelId = tokenExchangeResponse.ChannelId,
                            ConnectionName = tokenExchangeResponse.ConnectionName,
                            Token = tokenExchangeResponse.Token,
                        };
                    }
                }
            }
            else if (turnContext.Activity.Type == ActivityTypes.Message)
            {
                if (!string.IsNullOrEmpty(turnContext.Activity.Text))
                {
                    // regex to check if code supplied is a 6 digit numerical code (hence, a magic code).
                    var magicCodeRegex = new Regex(@"(\d{6})");
                    var matched = magicCodeRegex.Match(turnContext.Activity.Text);
                    if (matched.Success)
                    {
                        result = await GetUserTokenAsync(turnContext, settings, magicCode: matched.Value, cancellationToken).ConfigureAwait(false);
                    }
                }
            }

            return result;
        }

        private static CallerInfo CreateCallerInfo(ITurnContext turnContext)
        {
            if (turnContext.TurnState.Get<ClaimsIdentity>(BotAdapter.BotIdentityKey) is ClaimsIdentity botIdentity && botIdentity.Claims.IsSkillClaim())
            {
                return new CallerInfo
                {
                    CallerServiceUrl = turnContext.Activity.ServiceUrl,
                    Scope = botIdentity.Claims.GetAppIdFromClaims(),
                };
            }

            return null;
        }

        private static bool IsTokenResponseEvent(ITurnContext turnContext)
        {
            var activity = turnContext.Activity;
            return activity.Type == ActivityTypes.Event && activity.Name == SignInConstants.TokenResponseEventName;
        }

        private static bool IsTeamsVerificationInvoke(ITurnContext turnContext)
        {
            var activity = turnContext.Activity;
            return activity.Type == ActivityTypes.Invoke && activity.Name == SignInConstants.VerifyStateOperationName;
        }

        private static bool IsTokenExchangeRequestInvoke(ITurnContext turnContext)
        {
            var activity = turnContext.Activity;
            return activity.Type == ActivityTypes.Invoke && activity.Name == SignInConstants.TokenExchangeOperationName;
        }

        private static bool ChannelSupportsOAuthCard(string channelId)
        {
            switch (channelId)
            {
                case Channels.Cortana:
                case Channels.Skype:
                case Channels.Skypeforbusiness:
                    return false;
            }

            return true;
        }

        private static bool ChannelRequiresSignInLink(string channelId)
        {
            switch (channelId)
            {
                case Channels.Msteams:
                    return true;
            }

            return false;
        }

        private static async Task SendInvokeResponseAsync(ITurnContext turnContext, HttpStatusCode statusCode, object body, CancellationToken cancellationToken)
        {
            await turnContext.SendActivityAsync(
                new Activity
                {
                    Type = ActivityTypesEx.InvokeResponse,
                    Value = new InvokeResponse
                    {
                        Status = (int)statusCode,
                        Body = body,
                    },
                }, cancellationToken).ConfigureAwait(false);
        }

        private static async Task<TokenResponse> GetUserTokenAsync(ITurnContext turnContext, OAuthSettings settings, string magicCode, CancellationToken cancellationToken)
        {
            var userTokenClient = turnContext.TurnState.Get<UserTokenClient>();
            if (userTokenClient != null)
            {
                return await userTokenClient.GetUserTokenAsync(turnContext.Activity.From.Id, settings.ConnectionName, turnContext.Activity.ChannelId, magicCode, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                throw new NotSupportedException("OAuth prompt is not supported by the current adapter");
            }
        }

        private static async Task<SignInResource> GetSignInResourceAsync(ITurnContext turnContext, OAuthSettings settings, CancellationToken cancellationToken)
        {
            var userTokenClient = turnContext.TurnState.Get<UserTokenClient>();
            if (userTokenClient != null)
            {
                return await userTokenClient.GetSignInResourceAsync(settings.ConnectionName, turnContext.Activity, null, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                throw new NotSupportedException("OAuth prompt is not supported by the current adapter");
            }
        }

        private static async Task SignOutUserAsync(ITurnContext turnContext, OAuthSettings settings, CancellationToken cancellationToken)
        {
            var userTokenClient = turnContext.TurnState.Get<UserTokenClient>();
            if (userTokenClient != null)
            {
                await userTokenClient.SignOutUserAsync(turnContext.Activity.From.Id, settings.ConnectionName, turnContext.Activity.ChannelId, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                throw new NotSupportedException("OAuth prompt is not supported by the current adapter");
            }
        }

        private static async Task<TokenResponse> ExchangeTokenAsync(ITurnContext turnContext, OAuthSettings settings, TokenExchangeRequest tokenExchangeRequest, CancellationToken cancellationToken)
        {
            var userTokenClient = turnContext.TurnState.Get<UserTokenClient>();
            if (userTokenClient != null)
            {
                var userId = turnContext.Activity.From.Id;
                var channelId = turnContext.Activity.ChannelId;
                return await userTokenClient.ExchangeTokenAsync(userId, settings.ConnectionName, channelId, tokenExchangeRequest, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                throw new NotSupportedException("OAuth prompt is not supported by the current adapter");
            }
        }

        private static async Task<IConnectorClient> CreateConnectorClientAsync(ITurnContext turnContext, string serviceUrl, ClaimsIdentity claimsIdentity, string audience, CancellationToken cancellationToken)
        {
            var connectorFactory = turnContext.TurnState.Get<ConnectorFactory>();
            if (connectorFactory != null)
            {
                return await connectorFactory.CreateAsync(serviceUrl, audience, cancellationToken).ConfigureAwait(false);
            }
            else if (turnContext.Adapter is IConnectorClientBuilder connectorClientProvider)
            {
                return await connectorClientProvider.CreateConnectorClientAsync(serviceUrl, claimsIdentity, audience, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                throw new NotSupportedException("OAuth prompt is not supported by the current adapter");
            }
        }

        private class CallerInfo
        {
            public string CallerServiceUrl { get; set; }

            public string Scope { get; set; }
        }
    }
}
