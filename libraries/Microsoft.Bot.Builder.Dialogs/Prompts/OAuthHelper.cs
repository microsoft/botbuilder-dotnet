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

namespace Microsoft.Bot.Builder.Dialogs
{
    /// <summary>
    /// Common code for the OauthPrompt and OauthInput implementation.
    /// </summary>
    public static class OAuthHelper
    {
        /// <summary>
        /// The name of the options in the persisted state.
        /// </summary>
        public const string PersistedOptions = "options";

        /// <summary>
        /// The name of the prompt state in the persisted state.
        /// </summary>
        public const string PersistedState = "state";

        private const string PersistedExpires = "expires";
        private const string PersistedCaller = "caller";

        /// <summary>
        /// Helper function used in BeginDialog.
        /// </summary>
        /// <param name="userTokenClient">The userTokenClient.</param>
        /// <param name="settings">The oauth settings.</param>
        /// <param name="turnContext">The turncontext.</param>
        /// <param name="prompt">A message activity for the prompt.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A Task.</returns>
        public static async Task SendOAuthCardAsync(UserTokenClient userTokenClient, OAuthPromptSettings settings, ITurnContext turnContext, IMessageActivity prompt, CancellationToken cancellationToken)
        {
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
                    var signInResource = await userTokenClient.GetSignInResourceAsync(settings.ConnectionName, turnContext.Activity, null, cancellationToken).ConfigureAwait(false);
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
                var signInResource = await userTokenClient.GetSignInResourceAsync(settings.ConnectionName, turnContext.Activity, null, cancellationToken).ConfigureAwait(false);
                var value = signInResource.SignInLink;

                // use the SignInLink when 
                //   in speech channel or
                //   bot is a skill or
                // TODO: the OauthPrompt code also checked for || settings.OAuthAppCredentials != null
                if (turnContext.Activity.IsFromStreamingConnection() ||
                    IsSkill(turnContext.TurnState.Get<ClaimsIdentity>(BotAdapter.BotIdentityKey)))
                {
                    if (turnContext.Activity.ChannelId == Channels.Emulator)
                    {
                        cardActionType = ActionTypes.OpenUrl;
                    }
                }
                else if (!ChannelRequiresSignInLink(turnContext.Activity.ChannelId))
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
        /// Method to process Event and Invoke activities that are part of the Bot Framework Protocol oauth support.
        /// </summary>
        /// <param name="dc">The DialogContext.</param>
        /// <param name="userTokenClient">The user token client.</param>
        /// <param name="connectionName">The ConnectionName.</param>
        /// <param name="cancellationToken">CancellationToken.</param>
        /// <returns>A Task of DialogTurnResult.</returns>
        public static Task<DialogTurnResult> OnContinueWithNonMessageActivityAsync(DialogContext dc, UserTokenClient userTokenClient, string connectionName, CancellationToken cancellationToken)
        {
            switch (dc.Context.Activity.Type)
            {
                case ActivityTypes.Event:
                    return OnContinueEventActivityAsync(dc, cancellationToken);

                case ActivityTypes.Invoke:
                    return OnContinueInvokeActivityAsync(dc, userTokenClient, connectionName, cancellationToken);

                default:
                    // unexpected Activity type
                    return Task.FromResult(Dialog.EndOfTurn);
            }
        }

        /// <summary>
        /// Initialize the persisted state of the prompt. This is used in Begin with the data being retrieved in Continue.
        /// </summary>
        /// <param name="timeout">The Timeout.</param>
        /// <param name="dc">The DialogContext.</param>
        /// <param name="options">Persisted options.</param>
        public static void InitializeState(int? timeout, DialogContext dc, object options)
        {
            var state = dc.ActiveDialog.State;
            state[PersistedOptions] = options;
            state[PersistedState] = new Dictionary<string, object>
            {
                { "AttemptCount", 0 },
            };

            state[PersistedExpires] = DateTime.UtcNow.AddMilliseconds(timeout ?? (int)TurnStateConstants.OAuthLoginTimeoutValue.TotalMilliseconds);
            state[PersistedCaller] = CreateCallerInfo(dc.Context);
        }

        /// <summary>
        /// Check whether the persisted timeout has now expired. This is used in the Continue function.
        /// </summary>
        /// <param name="dc">The DialogContext.</param>
        /// <returns>Boolean indicating whether the timeout has expired.</returns>
        public static bool HasTimeoutExpired(DialogContext dc)
        {
            var state = dc.ActiveDialog.State;
            var expires = (DateTime)state[PersistedExpires];
            return DateTime.Compare(DateTime.UtcNow, expires) > 0;
        }

        /// <summary>
        /// Uses a magic code in message text to create a TokenResponse.
        /// </summary>
        /// <param name="userTokenClient">The UserTokenClient to use.</param>
        /// <param name="connectionName">The Connection Name to use.</param>
        /// <param name="activity">A message activity.</param>
        /// <param name="cancellationToken">A CancellationToken.</param>
        /// <returns>A TokenResponse.</returns>
        public static async Task<TokenResponse> CreateTokenResponseFromMessageAsync(UserTokenClient userTokenClient, string connectionName, Activity activity, CancellationToken cancellationToken)
        {
            // Attempt to recognize a magic code in the message text.
            var magicCode = RecognizeMagicCode(activity);

            // If we have a magic code then call the user token service.
            var userId = activity.From.Id;
            var channelId = activity.ChannelId;
            return magicCode != null ? await userTokenClient.GetUserTokenAsync(userId, connectionName, channelId, magicCode, cancellationToken).ConfigureAwait(false) : null;
        }

        private static string RecognizeMagicCode(Activity activity)
        {
            var magicCodeRegex = new Regex(@"(\d{6})");
            var matched = magicCodeRegex.Match(activity?.Text ?? string.Empty);
            return matched.Success ? matched.Value : null;
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

        private static bool IsSkill(ClaimsIdentity botIdentity)
        {
            return botIdentity == null ? false : SkillValidation.IsSkillClaim(botIdentity.Claims);
        }

        private static async Task<DialogTurnResult> OnContinueEventActivityAsync(DialogContext dc, CancellationToken cancellationToken)
        {
            switch (dc.Context.Activity.Name)
            {
                case SignInConstants.TokenResponseEventName:
                    {
                        var tokenResponse = await OnContinueTokenResponseAsync(dc, cancellationToken).ConfigureAwait(false);
                        return await dc.EndDialogAsync(tokenResponse, cancellationToken).ConfigureAwait(false);
                    }

                default:
                    // unexpected Event Activity name
                    return Dialog.EndOfTurn;
            }
        }

        private static async Task<TokenResponse> OnContinueTokenResponseAsync(DialogContext dc, CancellationToken cancellationToken)
        {
            var tokenResponseObject = dc.Context.Activity.Value as JObject;
            var tokenResponse = tokenResponseObject?.ToObject<TokenResponse>();

            // callerInfo will be NULL if this is a regular non-skill scenario - the callerInfo logic was set in the "Send"
            var callerInfo = (CallerInfo)dc.ActiveDialog.State[PersistedCaller];
            if (callerInfo != null)
            {
                // in the case of Skills the event activity will still have come directly from the service and not via the parent bot
                // replace the connector client with one that points at the parent bot as it would have been set to the service with the arrival of the EventActivity
                var connectorFactory = dc.Context.TurnState.Get<ConnectorFactory>() ?? throw new InvalidOperationException("The ConnectorFactory is not supported by the current adapter.");

                // note the connectorFactory will give us connector clients tied to this particular identity - but for different endpoints and audiences
                var connectorClient = await connectorFactory.CreateAsync(callerInfo.CallerServiceUrl, callerInfo.Scope, cancellationToken).ConfigureAwait(false);
                dc.Context.TurnState.Get<IConnectorClient>().Dispose();
                dc.Context.TurnState.Set(connectorClient);
            }

            return tokenResponse;
        }

        private static async Task<DialogTurnResult> OnContinueInvokeActivityAsync(DialogContext dc, UserTokenClient userTokenClient, string connectionName, CancellationToken cancellationToken)
        {
            var turnContext = dc.Context;

            switch (turnContext.Activity.Name)
            {
                case SignInConstants.VerifyStateOperationName:
                    {
                        var tokenResponse = await OnContinueVerifyStateAsync(userTokenClient, connectionName, turnContext, cancellationToken).ConfigureAwait(false);
                        return await dc.EndDialogAsync(tokenResponse, cancellationToken).ConfigureAwait(false);
                    }

                case SignInConstants.TokenExchangeOperationName:
                    {
                        var tokenResponse = await OnContinueTokenExchangeAsync(userTokenClient, connectionName, turnContext, cancellationToken).ConfigureAwait(false);
                        return await dc.EndDialogAsync(tokenResponse, cancellationToken).ConfigureAwait(false);
                    }

                default:
                    throw new Exception($"unexpected Invoke Activity name {turnContext.Activity.Name}");
            }
        }

        private static async Task<TokenResponse> OnContinueVerifyStateAsync(UserTokenClient userTokenClient, string connectionName, ITurnContext turnContext, CancellationToken cancellationToken)
        {
            // Getting the token follows a different flow in Teams. At the signin completion, Teams
            // will send the bot an "invoke" activity that contains a "magic" code. This code MUST
            // then be used to try fetching the token from Botframework service within some time
            // period. We try here. If it succeeds, we return 200 with an empty body. If it fails
            // with a retriable error, we return 500. Teams will re-send another invoke in this case.
            // If it fails with a non-retriable error, we return 404. Teams will not (still work in
            // progress) retry in that case.
            var userId = turnContext.Activity.From.Id;
            var channelId = turnContext.Activity.ChannelId;
            var magicCode = (turnContext.Activity.Value as JObject)?.GetValue("state", StringComparison.Ordinal)?.ToString();
            var tokenResponse = await userTokenClient.GetUserTokenAsync(userId, connectionName, channelId, magicCode, cancellationToken).ConfigureAwait(false);
            await turnContext.SendActivityAsync(CreateInvokeResponseActivity(tokenResponse != null ? HttpStatusCode.OK : HttpStatusCode.NotFound), cancellationToken).ConfigureAwait(false);
            return tokenResponse;
        }

        private static async Task<TokenResponse> OnContinueTokenExchangeAsync(UserTokenClient userTokenClient, string connectionName, ITurnContext turnContext, CancellationToken cancellationToken)
        {
            var tokenExchangeInvokeRequest = ((JObject)turnContext.Activity.Value)?.ToObject<TokenExchangeInvokeRequest>();

            HttpStatusCode httpStatusCode;
            TokenExchangeInvokeResponse tokenExchangeInvokeResponse;
            TokenResponse tokenResponse;

            if (tokenExchangeInvokeRequest == null)
            {
                httpStatusCode = HttpStatusCode.BadRequest;
                tokenExchangeInvokeResponse = new TokenExchangeInvokeResponse
                {
                    ConnectionName = connectionName,
                    FailureDetail = "The bot received an InvokeActivity that is missing a TokenExchangeInvokeRequest value. This is required to be sent with the InvokeActivity.",
                };
                tokenResponse = null;
            }
            else if (tokenExchangeInvokeRequest.ConnectionName != connectionName)
            {
                httpStatusCode = HttpStatusCode.BadRequest;
                tokenExchangeInvokeResponse = new TokenExchangeInvokeResponse
                {
                    Id = tokenExchangeInvokeRequest.Id,
                    ConnectionName = connectionName,
                    FailureDetail = $"The bot received an InvokeActivity with a TokenExchangeInvokeRequest containing a ConnectionName {tokenExchangeInvokeRequest.ConnectionName} that does not match the ConnectionName {connectionName} expected by the bot's active OAuthPrompt. Ensure these names match when sending the InvokeActivityInvalid ConnectionName in the TokenExchangeInvokeRequest",
                };
                tokenResponse = null;
            }
            else
            {
                // inbound invoke activity appears valid so we will call the user token service

                var userId = turnContext.Activity.From.Id;
                var channelId = turnContext.Activity.ChannelId;
                var tokenExchangeResponse = await userTokenClient.ExchangeTokenAsync(userId, connectionName, channelId, new TokenExchangeRequest { Token = tokenExchangeInvokeRequest.Token }, cancellationToken).ConfigureAwait(false);
                if (tokenExchangeResponse == null || string.IsNullOrEmpty(tokenExchangeResponse.Token))
                {
                    httpStatusCode = HttpStatusCode.PreconditionFailed;
                    tokenExchangeInvokeResponse = new TokenExchangeInvokeResponse
                    {
                        Id = tokenExchangeInvokeRequest.Id,
                        ConnectionName = connectionName,
                        FailureDetail = "The bot is unable to exchange token. Proceed with regular login.",
                    };
                    tokenResponse = null;
                }
                else
                {
                    httpStatusCode = HttpStatusCode.OK;
                    tokenExchangeInvokeResponse = new TokenExchangeInvokeResponse
                    {
                        Id = tokenExchangeInvokeRequest.Id,
                        ConnectionName = connectionName,
                    };
                    tokenResponse = new TokenResponse
                    {
                        ChannelId = tokenExchangeResponse.ChannelId,
                        ConnectionName = tokenExchangeResponse.ConnectionName,
                        Token = tokenExchangeResponse.Token,
                    };
                }
            }

            await turnContext.SendActivityAsync(CreateInvokeResponseActivity(httpStatusCode, tokenExchangeInvokeResponse)).ConfigureAwait(false);
            return tokenResponse;
        }

        private static Activity CreateInvokeResponseActivity(HttpStatusCode statusCode, object body = null)
        {
            return new Activity { Type = ActivityTypesEx.InvokeResponse, Value = new InvokeResponse { Status = (int)statusCode, Body = body } };
        }

        private static CallerInfo CreateCallerInfo(ITurnContext turnContext)
        {
            if (turnContext.TurnState.Get<ClaimsIdentity>(BotAdapter.BotIdentityKey) is ClaimsIdentity botIdentity && SkillValidation.IsSkillClaim(botIdentity.Claims))
            {
                return new CallerInfo
                {
                    CallerServiceUrl = turnContext.Activity.ServiceUrl,
                    Scope = JwtTokenValidation.GetAppIdFromClaims(botIdentity.Claims),
                };
            }

            return null;
        }

        private class CallerInfo
        {
            public string CallerServiceUrl { get; set; }

            public string Scope { get; set; }
        }
    }
}
