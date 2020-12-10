// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Newtonsoft.Json.Linq;

namespace AuthenticationBot
{
    /// <summary>
    /// This is test code for the UserTokenClient - it is basically the OAuthPrompt code cloned and fixed up
    /// to call the UserTokenClient rather than the IExtendedUserTokenProvider interface.
    /// The plan is to make the new SignInPrompt both more generic and significantly cleaned up.
    /// </summary>
    public class Test_SignInPrompt : Dialog
    {
        private const string PersistedOptions = "options";
        private const string PersistedState = "state";
        private const string PersistedExpires = "expires";
        private const string PersistedCaller = "caller";

        private readonly Test_SignInPromptSettings _settings;

        public Test_SignInPrompt(Test_SignInPromptSettings settings)
        {
            _settings = settings;
        }

        public override async Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default)
        {
            var userTokenClient = dc.Context.TurnState.Get<UserTokenClient>() ?? throw new InvalidOperationException("The UserTokenClient is not supported by the current adapter.");

            var tokenResponse = await userTokenClient.GetUserTokenAsync(dc.Context.Activity.From.Id, _settings.ConnectionName, dc.Context.Activity.ChannelId, null, cancellationToken).ConfigureAwait(false);
            if (tokenResponse != null)
            {
                return await dc.EndDialogAsync(tokenResponse, cancellationToken).ConfigureAwait(false);
            }

            // Initialize state
            var timeout = _settings.Timeout ?? (int)TurnStateConstants.OAuthLoginTimeoutValue.TotalMilliseconds;
            var state = dc.ActiveDialog.State;
            state[PersistedOptions] = options;
            state[PersistedState] = new Dictionary<string, object>
            {
                { "AttemptCount", 0 },
            };

            state[PersistedExpires] = DateTime.UtcNow.AddMilliseconds(timeout);
            SetCallerInfoInDialogState(state, dc.Context);

            // Prompt user to login
            await SendOAuthCardAsync(userTokenClient, _settings, dc.Context, null, cancellationToken).ConfigureAwait(false);
            return EndOfTurn;
        }

        public override async Task<DialogTurnResult> ContinueDialogAsync(DialogContext dc, CancellationToken cancellationToken = default)
        {
            // Check for timeout
            if (HasTimeoutExpired(dc))
            {
                return await dc.EndDialogAsync(cancellationToken).ConfigureAwait(false);
            }

            // This isn't really "recognize" because most of the cases are actually Event or Invoke activities
            // the only time anything is really recognized is when a magic code is passed in the Text of a Message Activity. 
            var recognized = await RecognizeTokenAsync(_settings, dc, cancellationToken).ConfigureAwait(false);

            var state = dc.ActiveDialog.State;
            var isMessage = dc.Context.Activity.Type == ActivityTypes.Message;

            var promptState = state[PersistedState].CastTo<IDictionary<string, object>>();
            var promptOptions = state[PersistedOptions].CastTo<PromptOptions>();

            // Increment attempt count
            // Convert.ToInt32 For issue https://github.com/Microsoft/botbuilder-dotnet/issues/1859
            promptState["AttemptCount"] = Convert.ToInt32(promptState["AttemptCount"], CultureInfo.InvariantCulture) + 1;

            if (recognized.Succeeded)
            {
                return await dc.EndDialogAsync(recognized.Value, cancellationToken).ConfigureAwait(false);
            }
            else if (isMessage && _settings.EndOnInvalidMessage)
            {
                // If EndOnInvalidMessage is set, complete the prompt with no result.
                return await dc.EndDialogAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            }

            if (!dc.Context.Responded && isMessage && promptOptions?.RetryPrompt != null)
            {
                await dc.Context.SendActivityAsync(promptOptions.RetryPrompt, cancellationToken).ConfigureAwait(false);
            }

            return EndOfTurn;
        }

        private static async Task SendOAuthCardAsync(UserTokenClient userTokenClient, Test_SignInPromptSettings settings, ITurnContext turnContext, IMessageActivity prompt, CancellationToken cancellationToken)
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

        private static async Task<PromptRecognizerResult<TokenResponse>> RecognizeTokenAsync(Test_SignInPromptSettings settings, DialogContext dc, CancellationToken cancellationToken)
        {
            var turnContext = dc.Context;
            var result = new PromptRecognizerResult<TokenResponse>();
            if (IsTokenResponseEvent(turnContext))
            {
                var tokenResponseObject = turnContext.Activity.Value as JObject;
                var token = tokenResponseObject?.ToObject<TokenResponse>();
                result.Succeeded = true;
                result.Value = token;

                // callerInfo will be NULL if this is a regular non-skill scenario - the callerInfo logic was set in the "Send"

                var callerInfo = (CallerInfo)dc.ActiveDialog.State[PersistedCaller];
                if (callerInfo != null)
                {
                    // in the case of Skills the event activity will still have come directly from the service and not via the parent bot

                    // replace the connector client with one that points at the parent bot as it would have been set to the service with the arrival of the EventActivity
                    var connectorFactory = dc.Context.TurnState.Get<ConnectorFactory>() ?? throw new InvalidOperationException("The ConnectorFactory is not supported by the current adapter.");

                    // note the connectorFactory will give us connector clients tighted to this particular identity - but for different endpoints and audiences
                    var connectorClient = await connectorFactory.CreateAsync(callerInfo.CallerServiceUrl, callerInfo.Scope, cancellationToken).ConfigureAwait(false);
                    turnContext.TurnState.Get<IConnectorClient>().Dispose();
                    turnContext.TurnState.Set(connectorClient);
                }
            }
            else if (IsTeamsVerificationInvoke(turnContext))
            {
                var magicCodeObject = turnContext.Activity.Value as JObject;
                var magicCode = magicCodeObject.GetValue("state", StringComparison.Ordinal)?.ToString();

                var userTokenClient = dc.Context.TurnState.Get<UserTokenClient>() ?? throw new InvalidOperationException("The UserTokenClient is not supported by the current adapter.");

                // Getting the token follows a different flow in Teams. At the signin completion, Teams
                // will send the bot an "invoke" activity that contains a "magic" code. This code MUST
                // then be used to try fetching the token from Botframework service within some time
                // period. We try here. If it succeeds, we return 200 with an empty body. If it fails
                // with a retriable error, we return 500. Teams will re-send another invoke in this case.
                // If it fails with a non-retriable error, we return 404. Teams will not (still work in
                // progress) retry in that case.
                try
                {
                    var token = await userTokenClient.GetUserTokenAsync(dc.Context.Activity.From.Id, settings.ConnectionName, dc.Context.Activity.ChannelId, magicCode, cancellationToken).ConfigureAwait(false);

                    if (token != null)
                    {
                        result.Succeeded = true;
                        result.Value = token;

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
                else if (!(turnContext.Adapter is IExtendedUserTokenProvider adapter))
                {
                    await SendInvokeResponseAsync(
                        turnContext,
                        HttpStatusCode.BadGateway,
                        new TokenExchangeInvokeResponse
                        {
                            Id = tokenExchangeRequest.Id,
                            ConnectionName = settings.ConnectionName,
                            FailureDetail = $"The bot's BotAdapter does not support token exchange operations. Ensure the bot's Adapter supports the {nameof(IExtendedUserTokenProvider)} interface.",
                        }, cancellationToken).ConfigureAwait(false);
                    throw new InvalidOperationException("OAuthPrompt.Recognize(): not supported by the current adapter");
                }
                else
                {
                    var userTokenClient = dc.Context.TurnState.Get<UserTokenClient>() ?? throw new InvalidOperationException("The UserTokenClient is not supported by the current adapter.");

                    TokenResponse tokenExchangeResponse = null;
                    try
                    {
                        tokenExchangeResponse = await userTokenClient.ExchangeTokenAsync(
                            turnContext.Activity.From.Id,
                            settings.ConnectionName,
                            turnContext.Activity.ChannelId,
                            new TokenExchangeRequest
                            {
                                Token = tokenExchangeRequest.Token,
                            },
                            cancellationToken).ConfigureAwait(false);
                    }
#pragma warning disable CA1031 // Do not catch general exception types (ignoring, see comment below)
                    catch
#pragma warning restore CA1031 // Do not catch general exception types
                    {
                        // TODO: this reasoning seems very weak.

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

                        result.Succeeded = true;
                        result.Value = new TokenResponse
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
                    var magicCode = magicCodeRegex.Match(turnContext.Activity.Text);
                    if (magicCode.Success)
                    {
                        var userTokenClient = dc.Context.TurnState.Get<UserTokenClient>() ?? throw new InvalidOperationException("The UserTokenClient is not supported by the current adapter.");

                        var token = await userTokenClient.GetUserTokenAsync(
                            turnContext.Activity.From.Id,
                            settings.ConnectionName,
                            turnContext.Activity.ChannelId,
                            magicCode.Value,
                            cancellationToken).ConfigureAwait(false);
                        if (token != null)
                        {
                            result.Succeeded = true;
                            result.Value = token;
                        }
                    }
                }
            }

            return result;
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

        private static void SetCallerInfoInDialogState(IDictionary<string, object> state, ITurnContext context)
        {
            state[PersistedCaller] = CreateCallerInfo(context);
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

        private static bool IsSkill(ClaimsIdentity botIdentity)
        {
            return botIdentity == null ? false : SkillValidation.IsSkillClaim(botIdentity.Claims);
        }

        private static bool HasTimeoutExpired(DialogContext dc)
        {
            // Check for timeout
            var state = dc.ActiveDialog.State;
            var expires = (DateTime)state[PersistedExpires];
            var isMessage = dc.Context.Activity.Type == ActivityTypes.Message;

            // If the incoming Activity is a message, or an Activity Type normally handled by OAuthPrompt,
            // check to see if this OAuthPrompt Expiration has elapsed, and end the dialog if so.
            var isTimeoutActivityType = isMessage
                            || IsTokenResponseEvent(dc.Context)
                            || IsTeamsVerificationInvoke(dc.Context)
                            || IsTokenExchangeRequestInvoke(dc.Context);

            var hasTimedOut = isTimeoutActivityType && DateTime.Compare(DateTime.UtcNow, expires) > 0;

            return hasTimedOut;
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

        private class CallerInfo
        {
            public string CallerServiceUrl { get; set; }

            public string Scope { get; set; }
        }
    }
}
