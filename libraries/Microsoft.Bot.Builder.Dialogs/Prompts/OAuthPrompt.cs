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
    /// Creates a new prompt that asks the user to sign in using the Bot Frameworks Single Sign On (SSO)
    /// service.
    /// </summary>
    /// <remarks>
    /// The prompt will attempt to retrieve the users current token and if the user isn't signed in, it
    /// will send them an `OAuthCard` containing a button they can press to signin. Depending on the
    /// channel, the user will be sent through one of two possible signin flows:
    ///
    /// - The automatic signin flow where once the user signs in and the SSO service will forward the bot
    /// the users access token using either an `event` or `invoke` activity.
    /// - The "magic code" flow where once the user signs in they will be prompted by the SSO
    /// service to send the bot a six digit code confirming their identity. This code will be sent as a
    /// standard `message` activity.
    ///
    /// Both flows are automatically supported by the `OAuthPrompt` and the only thing you need to be
    /// careful of is that you don't block the `event` and `invoke` activities that the prompt might
    /// be waiting on.
    ///
    /// <blockquote>
    /// **Note**:
    /// You should avoid persisting the access token with your bots other state. The Bot Frameworks
    /// SSO service will securely store the token on your behalf. If you store it in your bots state
    /// it could expire or be revoked in between turns.
    ///
    /// When calling the prompt from within a waterfall step you should use the token within the step
    /// following the prompt and then let the token go out of scope at the end of your function.
    /// </blockquote>
    ///
    /// ## Prompt Usage
    ///
    /// When used with your bot's <see cref="DialogSet"/> you can simply add a new instance of the prompt as a named
    /// dialog using <see cref="DialogSet.Add(Dialog)"/>. You can then start the prompt from a waterfall step using either
    /// <see cref="DialogContext.BeginDialogAsync(string, object, CancellationToken)"/> or
    /// <see cref="DialogContext.PromptAsync(string, PromptOptions, CancellationToken)"/>. The user
    /// will be prompted to signin as needed and their access token will be passed as an argument to
    /// the callers next waterfall step.
    /// </remarks>
    public class OAuthPrompt : Dialog
    {
        private const string PersistedOptions = "options";
        private const string PersistedState = "state";
        private const string PersistedExpires = "expires";

        // regex to check if code supplied is a 6 digit numerical code (hence, a magic code).
        private readonly Regex _magicCodeRegex = new Regex(@"(\d{6})");

        private readonly OAuthPromptSettings _settings;
        private readonly PromptValidator<TokenResponse> _validator;

        /// <summary>
        /// Initializes a new instance of the <see cref="OAuthPrompt"/> class.
        /// </summary>
        /// <param name="dialogId">The ID to assign to this prompt.</param>
        /// <param name="settings">Additional OAuth settings to use with this instance of the prompt.</param>
        /// <param name="validator">Optional, a <see cref="PromptValidator{FoundChoice}"/> that contains additional,
        /// custom validation for this prompt.</param>
        /// <remarks>The value of <paramref name="dialogId"/> must be unique within the
        /// <see cref="DialogSet"/> or <see cref="ComponentDialog"/> to which the prompt is added.</remarks>
        public OAuthPrompt(string dialogId, OAuthPromptSettings settings, PromptValidator<TokenResponse> validator = null)
            : base(dialogId)
        {
            if (string.IsNullOrWhiteSpace(dialogId))
            {
                throw new ArgumentNullException(nameof(dialogId));
            }

            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _validator = validator;
        }

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
                else
                {
                    throw new ArgumentException(nameof(options));
                }
            }

            // Initialize state
            var timeout = _settings.Timeout ?? (int)TurnStateConstants.OAuthLoginTimeoutValue.TotalMilliseconds;
            var state = dc.ActiveDialog.State;
            state[PersistedOptions] = opt;
            state[PersistedState] = new Dictionary<string, object>
            {
                { Prompt<int>.AttemptCountKey, 0 },
            };

            state[PersistedExpires] = DateTime.Now.AddMilliseconds(timeout);

            // Attempt to get the users token
            if (!(dc.Context.Adapter is IExtendedUserTokenProvider adapter))
            {
                throw new InvalidOperationException("OAuthPrompt.Recognize(): not supported by the current adapter");
            }

            var output = await adapter.GetUserTokenAsync(dc.Context, _settings.OAuthAppCredentials, _settings.ConnectionName, null, cancellationToken).ConfigureAwait(false);
            if (output != null)
            {
                // Return token
                return await dc.EndDialogAsync(output, cancellationToken).ConfigureAwait(false);
            }

            // Prompt user to login
            await SendOAuthCardAsync(dc.Context, opt?.Prompt, cancellationToken).ConfigureAwait(false);
            return EndOfTurn;
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

            // Recognize token
            var recognized = await RecognizeTokenAsync(dc.Context, cancellationToken).ConfigureAwait(false);

            // Check for timeout
            var state = dc.ActiveDialog.State;
            var expires = (DateTime)state[PersistedExpires];
            var isMessage = dc.Context.Activity.Type == ActivityTypes.Message;
            var hasTimedOut = isMessage && (DateTime.Compare(DateTime.Now, expires) > 0);

            if (hasTimedOut)
            {
                // if the token fetch request times out, complete the prompt with no result.
                return await dc.EndDialogAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            }

            var promptState = state[PersistedState].CastTo<IDictionary<string, object>>();
            var promptOptions = state[PersistedOptions].CastTo<PromptOptions>();

            // Increment attempt count
            // Convert.ToInt32 For issue https://github.com/Microsoft/botbuilder-dotnet/issues/1859
            promptState[Prompt<int>.AttemptCountKey] = Convert.ToInt32(promptState[Prompt<int>.AttemptCountKey]) + 1;

            // Validate the return value
            var isValid = false;
            if (_validator != null)
            {
                var promptContext = new PromptValidatorContext<TokenResponse>(dc.Context, recognized, promptState, promptOptions);
                isValid = await _validator(promptContext, cancellationToken).ConfigureAwait(false);
            }
            else if (recognized.Succeeded)
            {
                isValid = true;
            }

            // Return recognized value or re-prompt
            if (isValid)
            {
                return await dc.EndDialogAsync(recognized.Value, cancellationToken).ConfigureAwait(false);
            }

            if (!dc.Context.Responded && isMessage && promptOptions?.RetryPrompt != null)
            {
                await dc.Context.SendActivityAsync(promptOptions.RetryPrompt, cancellationToken).ConfigureAwait(false);
            }

            return EndOfTurn;
        }

        /// <summary>
        /// Attempts to get the user's token.
        /// </summary>
        /// <param name="turnContext">Context for the current turn of conversation with the user.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <remarks>If the task is successful and user already has a token or the user successfully signs in,
        /// the result contains the user's token.</remarks>
        public async Task<TokenResponse> GetUserTokenAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (!(turnContext.Adapter is IExtendedUserTokenProvider adapter))
            {
                throw new InvalidOperationException("OAuthPrompt.GetUserToken(): not supported by the current adapter");
            }

            return await adapter.GetUserTokenAsync(turnContext, _settings.OAuthAppCredentials, _settings.ConnectionName, null, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Signs out the user.
        /// </summary>
        /// <param name="turnContext">Context for the current turn of conversation with the user.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        public async Task SignOutUserAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (!(turnContext.Adapter is IExtendedUserTokenProvider adapter))
            {
                throw new InvalidOperationException("OAuthPrompt.SignOutUser(): not supported by the current adapter");
            }

            // Sign out user
            await adapter.SignOutUserAsync(turnContext, _settings.OAuthAppCredentials, _settings.ConnectionName, turnContext.Activity?.From?.Id, cancellationToken).ConfigureAwait(false);
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
                case Channels.Msteams:
                case Channels.Cortana:
                case Channels.Skype:
                case Channels.Skypeforbusiness:
                    return false;
            }

            return true;
        }

        private async Task SendOAuthCardAsync(ITurnContext turnContext, IMessageActivity prompt, CancellationToken cancellationToken = default(CancellationToken))
        {
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
                    var signInResource = await adapter.GetSignInResourceAsync(turnContext, _settings.OAuthAppCredentials, _settings.ConnectionName, turnContext.Activity.From.Id, null, cancellationToken).ConfigureAwait(false);
                    prompt.Attachments.Add(new Attachment
                    {
                        ContentType = SigninCard.ContentType,
                        Content = new SigninCard
                        {
                            Text = _settings.Text,
                            Buttons = new[]
                            {
                                new CardAction
                                {
                                    Title = _settings.Title,
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
                var signInResource = await adapter.GetSignInResourceAsync(turnContext, _settings.OAuthAppCredentials, _settings.ConnectionName, turnContext.Activity.From.Id, null, cancellationToken).ConfigureAwait(false);
                var value = signInResource.SignInLink;

                // use the SignInLink when 
                //   in speech channel or
                //   bot is a skill or
                //   an extra OAuthAppCredentials is being passed in
                if (turnContext.Activity.IsFromStreamingConnection() ||
                    (turnContext.TurnState.Get<ClaimsIdentity>(BotAdapter.BotIdentityKey) is ClaimsIdentity botIdentity && SkillValidation.IsSkillClaim(botIdentity.Claims)) ||
                    _settings.OAuthAppCredentials != null)
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

                prompt.Attachments.Add(new Attachment
                {
                    ContentType = OAuthCard.ContentType,
                    Content = new OAuthCard
                    {
                        Text = _settings.Text,
                        ConnectionName = _settings.ConnectionName,
                        Buttons = new[]
                        {
                            new CardAction
                            {
                                Title = _settings.Title,
                                Text = _settings.Text,
                                Type = cardActionType,
                                Value = value
                            },
                        },
                        TokenExchangeResource = signInResource.TokenExchangeResource,
                    },
                });
            }

            // Add the login timeout specified in OAuthPromptSettings to TurnState so it can be referenced if polling is needed
            if (!turnContext.TurnState.ContainsKey(TurnStateConstants.OAuthLoginTimeoutKey) && _settings.Timeout.HasValue)
            {
                turnContext.TurnState.Add<object>(TurnStateConstants.OAuthLoginTimeoutKey, TimeSpan.FromMilliseconds(_settings.Timeout.Value));
            }

            // Set input hint
            if (string.IsNullOrEmpty(prompt.InputHint))
            {
                prompt.InputHint = InputHints.AcceptingInput;
            }

            await turnContext.SendActivityAsync(prompt, cancellationToken).ConfigureAwait(false);
        }

        private async Task<PromptRecognizerResult<TokenResponse>> RecognizeTokenAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
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
                    var token = await adapter.GetUserTokenAsync(turnContext, _settings.OAuthAppCredentials, _settings.ConnectionName, magicCode, cancellationToken).ConfigureAwait(false);

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
                            ConnectionName = _settings.ConnectionName,
                            FailureDetail = "The bot received an InvokeActivity that is missing a TokenExchangeInvokeRequest value. This is required to be sent with the InvokeActivity.",
                        }).ConfigureAwait(false);
                }
                else if (tokenExchangeRequest.ConnectionName != _settings.ConnectionName)
                {
                    await this.SendInvokeResponseAsync(
                        turnContext,
                        cancellationToken,
                        HttpStatusCode.BadRequest,
                        new TokenExchangeInvokeResponse()
                        {
                            Id = tokenExchangeRequest.Id,
                            ConnectionName = _settings.ConnectionName,
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
                               ConnectionName = _settings.ConnectionName,
                               FailureDetail = "The bot's BotAdapter does not support token exchange operations. Ensure the bot's Adapter supports the ITokenExchangeProvider interface.",
                           }).ConfigureAwait(false);
                    throw new InvalidOperationException("OAuthPrompt.Recognize(): not supported by the current adapter");
                }
                else
                {
                    var tokenExchangeResponse = await adapter.ExchangeTokenAsync(
                        turnContext,
                        _settings.ConnectionName,
                        turnContext.Activity.From.Id,
                        new TokenExchangeRequest()
                        {
                            Token = tokenExchangeRequest.Token,
                        },
                        cancellationToken).ConfigureAwait(false);

                    if (tokenExchangeResponse == null || string.IsNullOrEmpty(tokenExchangeResponse.Token))
                    {
                        await this.SendInvokeResponseAsync(
                           turnContext,
                           cancellationToken,
                           HttpStatusCode.Conflict,
                           new TokenExchangeInvokeResponse()
                           {
                               Id = tokenExchangeRequest.Id,
                               ConnectionName = _settings.ConnectionName,
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
                               ConnectionName = _settings.ConnectionName,
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

                    var token = await adapter.GetUserTokenAsync(turnContext, _settings.OAuthAppCredentials, _settings.ConnectionName, matched.Value, cancellationToken).ConfigureAwait(false);
                    if (token != null)
                    {
                        result.Succeeded = true;
                        result.Value = token;
                    }
                }
            }

            return result;
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
