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
using AdaptiveExpressions.Properties;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Input
{
    /// <summary>
    /// OAuthInput prompts user to login.
    /// </summary>
    public class OAuthInput : InputDialog
    {
        [JsonProperty("$kind")]
        public const string Kind = "Microsoft.OAuthInput";

        private const string PersistedOptions = "options";
        private const string PersistedState = "state";
        private const string PersistedExpires = "expires";
        private const string AttemptCountKey = "AttemptCount";

        // regex to check if code supplied is a 6 digit numerical code (hence, a magic code).
        private readonly Regex _magicCodeRegex = new Regex(@"(\d{6})");

        /// <summary>
        /// Gets or sets the name of the OAuth connection.
        /// </summary>
        /// <value>String or expression which evaluates to a string.</value>
        [JsonProperty("connectionName")]
        public StringExpression ConnectionName { get; set; }

        /// <summary>
        /// Gets or sets the title of the sign-in card.
        /// </summary>
        /// <value>String or expression which evaluates to string.</value>
        [JsonProperty("title")]
        public StringExpression Title { get; set; }

        /// <summary>
        /// Gets or sets any additional text to include in the sign-in card.
        /// </summary>
        /// <value>String or expression which evaluates to a string.</value>
        [JsonProperty("text")]
        public StringExpression Text { get; set; }

        /// <summary>
        /// Gets or sets the number of milliseconds the prompt waits for the user to authenticate.
        /// Default is 900,000 (15 minutes).
        /// </summary>
        /// <value>Int or expression which evaluates to int.</value>
        [JsonProperty("timeout")]
        public IntExpression Timeout { get; set; } = 900000;

        /// <summary>
        /// Called when a prompt dialog is pushed onto the dialog stack and is being activated.
        /// </summary>
        /// <param name="dc">The dialog context for the current turn of the conversation.</param>
        /// <param name="options">Optional, additional information to pass to the prompt being started.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        /// <remarks>If the task is successful, the result indicates whether the prompt is still
        /// active after the turn has been processed by the prompt.</remarks>
        public override async Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (dc == null)
            {
                throw new ArgumentNullException(nameof(dc));
            }

            if (options is CancellationToken)
            {
                throw new ArgumentException($"{nameof(options)} cannot be a cancellation token");
            }

            if (this.Disabled != null && this.Disabled.GetValue(dc.State))
            {
                return await dc.EndDialogAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            }

            PromptOptions opt = null;
            if (options != null)
            {
                if (options is PromptOptions)
                {
                    // Ensure prompts have input hint set
                    opt = options as PromptOptions;
                    if (opt.Prompt != null && string.IsNullOrEmpty(opt.Prompt.InputHint))
                    {
                        opt.Prompt.InputHint = InputHints.AcceptingInput;
                    }

                    if (opt.RetryPrompt != null && string.IsNullOrEmpty(opt.RetryPrompt.InputHint))
                    {
                        opt.RetryPrompt.InputHint = InputHints.AcceptingInput;
                    }
                }
            }

            var op = OnInitializeOptions(dc, options);
            dc.State.SetValue(ThisPath.Options, op);
            dc.State.SetValue(TURN_COUNT_PROPERTY, 0);

            // If AlwaysPrompt is set to true, then clear Property value for turn 0.
            if (this.Property != null && this.AlwaysPrompt != null && this.AlwaysPrompt.GetValue(dc.State))
            {
                dc.State.SetValue(this.Property.GetValue(dc.State), null);
            }

            // Initialize state
            var state = dc.ActiveDialog.State;
            state[PersistedOptions] = opt;
            state[PersistedState] = new Dictionary<string, object>
            {
                { AttemptCountKey, 0 },
            };

            state[PersistedExpires] = DateTime.Now.AddMilliseconds(Timeout.GetValue(dc.State));

            // Attempt to get the users token
            if (!(dc.Context.Adapter is IUserTokenProvider adapter))
            {
                throw new InvalidOperationException("OAuthPrompt.Recognize(): not supported by the current adapter");
            }

            var output = await adapter.GetUserTokenAsync(dc.Context, ConnectionName.GetValue(dc.State), null, cancellationToken).ConfigureAwait(false);
            if (output != null)
            {
                if (this.Property != null)
                {
                    dc.State.SetValue(this.Property.GetValue(dc.State), output);
                }

                // Return token
                return await dc.EndDialogAsync(output, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                dc.State.SetValue(TURN_COUNT_PROPERTY, 1);

                // Prompt user to login
                await SendOAuthCardAsync(dc, opt?.Prompt, cancellationToken).ConfigureAwait(false);
                return Dialog.EndOfTurn;
            }
        }

        /// <summary>
        /// Called when a prompt dialog is the active dialog and the user replied with a new activity.
        /// </summary>
        /// <param name="dc">The dialog context for the current turn of conversation.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        /// <remarks>If the task is successful, the result indicates whether the dialog is still
        /// active after the turn has been processed by the dialog.
        /// <para>The prompt generally continues to receive the user's replies until it accepts the
        /// user's reply as valid input for the prompt.</para></remarks>
        public override async Task<DialogTurnResult> ContinueDialogAsync(DialogContext dc, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (dc == null)
            {
                throw new ArgumentNullException(nameof(dc));
            }

            var interrupted = dc.State.GetValue<bool>(TurnPath.Interrupted, () => false);
            var turnCount = dc.State.GetValue<int>(TURN_COUNT_PROPERTY, () => 0);

            // Recognize token
            var recognized = await RecognizeTokenAsync(dc, cancellationToken).ConfigureAwait(false);

            // Check for timeout
            var state = dc.ActiveDialog.State;
            var expires = (DateTime)state[PersistedExpires];
            var isMessage = dc.Context.Activity.Type == ActivityTypes.Message;
            var hasTimedOut = isMessage && (DateTime.Compare(DateTime.Now, expires) > 0);

            if (hasTimedOut)
            {
                if (this.Property != null)
                {
                    dc.State.SetValue(this.Property.GetValue(dc.State), null);
                }

                // if the token fetch request times out, complete the prompt with no result.
                return await dc.EndDialogAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            }
            else
            {
                var promptState = (IDictionary<string, object>)state[PersistedState];
                var promptOptions = (PromptOptions)state[PersistedOptions];

                // Increment attempt count
                // Convert.ToInt32 For issue https://github.com/Microsoft/botbuilder-dotnet/issues/1859
                promptState[AttemptCountKey] = Convert.ToInt32(promptState[AttemptCountKey]) + 1;

                // Validate the return value
                var inputState = InputState.Invalid;
                if (recognized.Succeeded)
                {
                    inputState = InputState.Valid;
                }

                // Return recognized value or re-prompt
                if (inputState == InputState.Valid)
                {
                    if (this.Property != null)
                    {
                        dc.State.SetValue(this.Property.GetValue(dc.State), recognized.Value);
                    }

                    return await dc.EndDialogAsync(recognized.Value, cancellationToken).ConfigureAwait(false);
                }
                else if (this.MaxTurnCount == null || turnCount < this.MaxTurnCount.GetValue(dc.State))
                {
                    // increase the turnCount as last step
                    dc.State.SetValue(TURN_COUNT_PROPERTY, turnCount + 1);
                    var prompt = await this.OnRenderPrompt(dc, inputState).ConfigureAwait(false);
                    await dc.Context.SendActivityAsync(prompt).ConfigureAwait(false);
                    await SendOAuthCardAsync(dc, promptOptions?.Prompt, cancellationToken).ConfigureAwait(false);
                    return Dialog.EndOfTurn;
                }
                else
                {
                    if (this.DefaultValue != null)
                    {
                        var (value, _) = this.DefaultValue.TryGetValue(dc.State);
                        if (this.DefaultValueResponse != null)
                        {
                            var response = await this.DefaultValueResponse.BindAsync(dc).ConfigureAwait(false);
                            var properties = new Dictionary<string, string>()
                            {
                                { "template", JsonConvert.SerializeObject(this.DefaultValueResponse) },
                                { "result", response == null ? string.Empty : JsonConvert.SerializeObject(response, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore }) },
                            };
                            TelemetryClient.TrackEvent("GeneratorResult", properties);
                            await dc.Context.SendActivityAsync(response).ConfigureAwait(false);
                        }

                        // set output property
                        dc.State.SetValue(this.Property.GetValue(dc.State), value);
                        return await dc.EndDialogAsync(value).ConfigureAwait(false);
                    }
                }

                return await dc.EndDialogAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Attempts to get the user's token.
        /// </summary>
        /// <param name="dc">DialogContext for the current turn of conversation with the user.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <remarks>If the task is successful and user already has a token or the user successfully signs in,
        /// the result contains the user's token.</remarks>
        public async Task<TokenResponse> GetUserTokenAsync(DialogContext dc, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (!(dc.Context.Adapter is IUserTokenProvider adapter))
            {
                throw new InvalidOperationException("OAuthPrompt.GetUserToken(): not supported by the current adapter");
            }

            return await adapter.GetUserTokenAsync(dc.Context, ConnectionName.GetValue(dc.State), null, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Signs out the user.
        /// </summary>
        /// <param name="dc">DialogContext for the current turn of conversation with the user.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        public async Task SignOutUserAsync(DialogContext dc, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (!(dc.Context.Adapter is IUserTokenProvider adapter))
            {
                throw new InvalidOperationException("OAuthPrompt.SignOutUser(): not supported by the current adapter");
            }

            // Sign out user
            await adapter.SignOutUserAsync(dc.Context, ConnectionName.GetValue(dc.State), dc.Context.Activity?.From?.Id, cancellationToken).ConfigureAwait(false);
        }

        protected override Task<InputState> OnRecognizeInput(DialogContext dc)
        {
            throw new NotImplementedException();
        }

        private async Task SendOAuthCardAsync(DialogContext dc, IMessageActivity prompt, CancellationToken cancellationToken = default(CancellationToken))
        {
            var turnContext = dc.Context;

            BotAssert.ContextNotNull(turnContext);

            if (!(turnContext.Adapter is IExtendedUserTokenProvider adapter))
            {
                throw new InvalidOperationException("OAuthPrompt.Prompt(): not supported by the current adapter");
            }

            // Ensure prompt initialized
            if (prompt == null)
            {
                prompt = Activity.CreateMessageActivity();
            }

            if (prompt.Attachments == null)
            {
                prompt.Attachments = new List<Attachment>();
            }

            // Append appropriate card if missing
            if (!ChannelSupportsOAuthCard(turnContext.Activity.ChannelId))
            {
                if (!prompt.Attachments.Any(a => a.Content is SigninCard))
                {
                    var signInResource = await adapter.GetSignInResourceAsync(turnContext, null, ConnectionName?.GetValue(dc.State), turnContext.Activity.From.Id, null, cancellationToken).ConfigureAwait(false);
                    prompt.Attachments.Add(new Attachment
                    {
                        ContentType = SigninCard.ContentType,
                        Content = new SigninCard
                        {
                            Text = Text?.GetValue(dc.State),
                            Buttons = new[]
                            {
                                new CardAction
                                {
                                    Title = Title?.GetValue(dc.State),
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
                var signInResource = await adapter.GetSignInResourceAsync(turnContext, null, ConnectionName?.GetValue(dc.State), turnContext.Activity.From.Id, null, cancellationToken).ConfigureAwait(false);
                var value = signInResource.SignInLink;

                // use the SignInLink when 
                //   in speech channel or
                //   bot is a skill or
                //   an extra OAuthAppCredentials is being passed in
                if (turnContext.Activity.IsFromStreamingConnection() ||
                    (turnContext.TurnState.Get<ClaimsIdentity>(BotAdapter.BotIdentityKey) is ClaimsIdentity botIdentity && SkillValidation.IsSkillClaim(botIdentity.Claims)) ||
                    null != null)
                {
                    if (turnContext.Activity.ChannelId == Channels.Emulator)
                    {
                        cardActionType = ActionTypes.OpenUrl;
                    }
                }
                else
                {
                    value = null;
                }

                var text = Text?.GetValue(dc.State);
                var connectionName = ConnectionName?.GetValue(dc.State);
                var title = Title?.GetValue(dc.State);
                prompt.Attachments.Add(new Attachment
                {
                    ContentType = OAuthCard.ContentType,
                    Content = new OAuthCard
                    {
                        Text = text,
                        ConnectionName = connectionName,
                        Buttons = new[]
                        {
                            new CardAction
                            {
                                Title = title,
                                Text = text,
                                Type = cardActionType,
                                Value = value
                            },
                        },
                        TokenExchangeResource = signInResource.TokenExchangeResource,
                    },
                });
            }

            // Add the login timeout specified in OAuthPromptSettings to TurnState so it can be referenced if polling is needed
            if (!turnContext.TurnState.ContainsKey(TurnStateConstants.OAuthLoginTimeoutKey) && Timeout != null)
            {
                turnContext.TurnState.Add<object>(TurnStateConstants.OAuthLoginTimeoutKey, TimeSpan.FromMilliseconds(Timeout.GetValue(dc.State)));
            }

            // Set input hint
            if (string.IsNullOrEmpty(prompt.InputHint))
            {
                prompt.InputHint = InputHints.AcceptingInput;
            }

            await turnContext.SendActivityAsync(prompt, cancellationToken).ConfigureAwait(false);
        }

        private async Task<PromptRecognizerResult<TokenResponse>> RecognizeTokenAsync(DialogContext dc, CancellationToken cancellationToken = default(CancellationToken))
        {
            var turnContext = dc.Context;

            var result = new PromptRecognizerResult<TokenResponse>();
            if (IsTokenResponseEvent(turnContext))
            {
                var tokenResponseObject = turnContext.Activity.Value as JObject;
                var token = tokenResponseObject?.ToObject<TokenResponse>();
                result.Succeeded = true;
                result.Value = token;
            }
            else if (IsTeamsVerificationInvoke(turnContext))
            {
                var magicCodeObject = turnContext.Activity.Value as JObject;
                var magicCode = magicCodeObject.GetValue("state")?.ToString();

                if (!(turnContext.Adapter is IExtendedUserTokenProvider adapter))
                {
                    throw new InvalidOperationException("OAuthPrompt.Recognize(): not supported by the current adapter");
                }

                // Getting the token follows a different flow in Teams. At the signin completion, Teams
                // will send the bot an "invoke" activity that contains a "magic" code. This code MUST
                // then be used to try fetching the token from Botframework service within some time
                // period. We try here. If it succeeds, we return 200 with an empty body. If it fails
                // with a retriable error, we return 500. Teams will re-send another invoke in this case.
                // If it failes with a non-retriable error, we return 404. Teams will not (still work in
                // progress) retry in that case.
                try
                {
                    var token = await adapter.GetUserTokenAsync(turnContext, ConnectionName.GetValue(dc.State), magicCode, cancellationToken).ConfigureAwait(false);

                    if (token != null)
                    {
                        result.Succeeded = true;
                        result.Value = token;

                        await turnContext.SendActivityAsync(new Activity { Type = ActivityTypesEx.InvokeResponse }, cancellationToken).ConfigureAwait(false);
                    }
                    else
                    {
                        await this.SendInvokeResponseAsync(turnContext, cancellationToken, HttpStatusCode.NotFound).ConfigureAwait(false);
                    }
                }
                catch
                {
                    await this.SendInvokeResponseAsync(turnContext, cancellationToken, HttpStatusCode.InternalServerError).ConfigureAwait(false);
                }
            }
            else if (IsTokenExchangeRequestInvoke(turnContext))
            {
                var connectionName = ConnectionName.GetValue(dc.State);

                var tokenExchangeRequest = ((JObject)turnContext.Activity.Value)?.ToObject<TokenExchangeInvokeRequest>();

                if (tokenExchangeRequest == null)
                {
                    await this.SendInvokeResponseAsync(
                        turnContext,
                        cancellationToken,
                        HttpStatusCode.BadRequest,
                        new TokenExchangeInvokeResponse()
                        {
                            Id = null,
                            ConnectionName = connectionName,
                            FailureDetail = "The bot received an InvokeActivity that is missing a TokenExchangeInvokeRequest value. This is required to be sent with the InvokeActivity.",
                        }).ConfigureAwait(false);
                }
                else if (tokenExchangeRequest.ConnectionName != connectionName)
                {
                    await this.SendInvokeResponseAsync(
                        turnContext,
                        cancellationToken,
                        HttpStatusCode.BadRequest,
                        new TokenExchangeInvokeResponse()
                        {
                            Id = tokenExchangeRequest.Id,
                            ConnectionName = connectionName,
                            FailureDetail = "The bot received an InvokeActivity with a TokenExchangeInvokeRequest containing a ConnectionName that does not match the ConnectionName expected by the bot's active OAuthPrompt. Ensure these names match when sending the InvokeActivityInvalid ConnectionName in the TokenExchangeInvokeRequest",
                        }).ConfigureAwait(false);
                }
                else if (!(turnContext.Adapter is IExtendedUserTokenProvider adapter))
                {
                    await this.SendInvokeResponseAsync(
                           turnContext,
                           cancellationToken,
                           HttpStatusCode.BadGateway,
                           new TokenExchangeInvokeResponse()
                           {
                               Id = tokenExchangeRequest.Id,
                               ConnectionName = connectionName,
                               FailureDetail = $"The bot's BotAdapter does not support token exchange operations. Ensure the bot's Adapter supports the {nameof(IExtendedUserTokenProvider)} interface.",
                           }).ConfigureAwait(false);
                    throw new InvalidOperationException("OAuthPrompt.Recognize(): not supported by the current adapter");
                }
                else
                {
                    TokenResponse tokenExchangeResponse = null;
                    try
                    {
                        tokenExchangeResponse = await adapter.ExchangeTokenAsync(
                           turnContext,
                           connectionName,
                           turnContext.Activity.From.Id,
                           new TokenExchangeRequest()
                           {
                               Token = tokenExchangeRequest.Token,
                           },
                           cancellationToken).ConfigureAwait(false);
                    }
                    catch
                    {
                        // Ignore Exceptions
                        // If token exchange failed for any reason, tokenExchangeResponse above stays null , and hence we send back a failure invoke response to the caller.
                        // This ensures that the caller shows 
                    }

                    if (tokenExchangeResponse == null || string.IsNullOrEmpty(tokenExchangeResponse.Token))
                    {
                        await this.SendInvokeResponseAsync(
                           turnContext,
                           cancellationToken,
                           HttpStatusCode.Conflict,
                           new TokenExchangeInvokeResponse()
                           {
                               Id = tokenExchangeRequest.Id,
                               ConnectionName = connectionName,
                               FailureDetail = "The bot is unable to exchange token. Proceed with regular login.",
                           }).ConfigureAwait(false);
                    }
                    else
                    {
                        await this.SendInvokeResponseAsync(
                           turnContext,
                           cancellationToken,
                           HttpStatusCode.OK,
                           new TokenExchangeInvokeResponse()
                           {
                               Id = tokenExchangeRequest.Id,
                               ConnectionName = connectionName,
                           }).ConfigureAwait(false);

                        result.Succeeded = true;
                        result.Value = new TokenResponse()
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
                var matched = _magicCodeRegex.Match(turnContext.Activity.Text);
                if (matched.Success)
                {
                    if (!(turnContext.Adapter is IExtendedUserTokenProvider adapter))
                    {
                        throw new InvalidOperationException("OAuthPrompt.Recognize(): not supported by the current adapter");
                    }

                    var token = await adapter.GetUserTokenAsync(turnContext, ConnectionName.GetValue(dc.State), matched.Value, cancellationToken).ConfigureAwait(false);
                    if (token != null)
                    {
                        result.Succeeded = true;
                        result.Value = token;
                    }
                }
            }

            return result;
        }

        private bool IsTokenResponseEvent(ITurnContext turnContext)
        {
            var activity = turnContext.Activity;
            return activity.Type == ActivityTypes.Event && activity.Name == SignInConstants.TokenResponseEventName;
        }

        private bool IsTeamsVerificationInvoke(ITurnContext turnContext)
        {
            var activity = turnContext.Activity;
            return activity.Type == ActivityTypes.Invoke && activity.Name == SignInConstants.VerifyStateOperationName;
        }

        private bool IsTokenExchangeRequestInvoke(ITurnContext turnContext)
        {
            var activity = turnContext.Activity;
            return activity.Type == ActivityTypes.Invoke && activity.Name == SignInConstants.TokenExchangeOperationName;
        }

        private bool ChannelSupportsOAuthCard(string channelId)
        {
            switch (channelId)
            {
                case Channels.Msteams:
                case Channels.Cortana:
                case Channels.Skype:
                case Channels.Skypeforbusiness:
                    return false;
            }

            return true;
        }

        private async Task SendInvokeResponseAsync(ITurnContext turnContext, CancellationToken cancellationToken, HttpStatusCode statusCode, object body = null)
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
    }
}
