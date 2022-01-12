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
using Microsoft.Bot.Builder.OAuth;
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
        
        private readonly OAuthPromptSettings _settings;
        private readonly PromptValidator<TokenResponse> _validator;
        private readonly UserAuthActivityFactory _cardProvider;
        private readonly UserTokenResponseClient _userTokenResponseClient;

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
            _cardProvider = new UserAuthActivityFactory(_settings);
            _userTokenResponseClient = new UserTokenResponseClient(_settings.ConnectionName);
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
        public override async Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default)
        {
            if (dc == null)
            {
                throw new ArgumentNullException(nameof(dc));
            }

            if (options is CancellationToken)
            {
                throw new ArgumentException($"{nameof(options)} cannot be a cancellation token", nameof(options));
            }

            if (options != null && !(options is PromptOptions))
            {
                throw new ArgumentException($"Parameter {nameof(options)} should be an instance of to {nameof(PromptOptions)} if provided", nameof(options));
            }

            var opt = (PromptOptions)options;
            if (opt != null)
            {
                // Ensure prompts have input hint set
                if (opt.Prompt != null && string.IsNullOrEmpty(opt.Prompt.InputHint))
                {
                    opt.Prompt.InputHint = InputHints.AcceptingInput;
                }

                if (opt.RetryPrompt != null && string.IsNullOrEmpty(opt.RetryPrompt.InputHint))
                {
                    opt.RetryPrompt.InputHint = InputHints.AcceptingInput;
                }
            }

            // Initialize state
            var timeout = _settings.Timeout <= 0 ? (int)TurnStateConstants.OAuthLoginTimeoutValue.TotalMilliseconds : _settings.Timeout;
            var state = dc.ActiveDialog.State;
            state[PersistedOptions] = opt;
            state[PersistedState] = new Dictionary<string, object>
            {
                { Prompt<int>.AttemptCountKey, 0 },
            };

            state[PersistedExpires] = DateTime.UtcNow.AddMilliseconds(timeout);
            UserTokenResponseClient.SetCallerInfoInState(state, dc.Context);

            // Attempt to get the users token
            var userTokenClient = dc.Context.TurnState.Get<UserTokenClient>();
            if (userTokenClient != null)
            {
                var output = await userTokenClient.GetUserTokenAsync(dc.Context.Activity.From.Id, _settings.ConnectionName, dc.Context.Activity.ChannelId, magicCode: null, cancellationToken).ConfigureAwait(false);
                if (output != null)
                {
                    // Return token
                    return await dc.EndDialogAsync(output, cancellationToken).ConfigureAwait(false);
                }
            }
            else
            {
                throw new NotSupportedException("OAuth prompt is not supported by the current adapter");
            }

            // Prompt user to login
            var prompt = await _cardProvider.CreateOAuthActivityAsync(dc.Context.Activity, userTokenClient, _settings, promptActivity: opt?.Prompt, cancellationToken: cancellationToken).ConfigureAwait(false);
            await dc.Context.SendActivityAsync(prompt, cancellationToken).ConfigureAwait(false);
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
        public override async Task<DialogTurnResult> ContinueDialogAsync(DialogContext dc, CancellationToken cancellationToken = default)
        {
            if (dc == null)
            {
                throw new ArgumentNullException(nameof(dc));
            }

            // Check for timeout
            var state = dc.ActiveDialog.State;
            var expires = (DateTime)state[PersistedExpires];
            var isMessage = dc.Context.Activity.Type == ActivityTypes.Message;

            // If the incoming Activity is a message, or an Activity Type normally handled by OAuthPrompt,
            // check to see if this OAuthPrompt Expiration has elapsed, and end the dialog if so.
            var isTimeoutActivityType = isMessage
                            || UserTokenResponseClient.IsOAuthResponseActivity(dc.Context.Activity);
            var hasTimedOut = isTimeoutActivityType && DateTime.Compare(DateTime.UtcNow, expires) > 0;

            if (hasTimedOut)
            {
                // if the token fetch request times out, complete the prompt with no result.
                return await dc.EndDialogAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            }

            // Recognize token
            var tokenResponse = await _userTokenResponseClient.RecognizeTokenAsync(dc.Context, _settings.ConnectionName, state, cancellationToken: cancellationToken).ConfigureAwait(false);
            
            var promptState = state[PersistedState].CastTo<IDictionary<string, object>>();
            var promptOptions = state[PersistedOptions].CastTo<PromptOptions>();

            // Increment attempt count
            // Convert.ToInt32 For issue https://github.com/Microsoft/botbuilder-dotnet/issues/1859
            promptState[Prompt<int>.AttemptCountKey] = Convert.ToInt32(promptState[Prompt<int>.AttemptCountKey], CultureInfo.InvariantCulture) + 1;

            // Validate the return value
            var isValid = false;
            if (_validator != null)
            {
                var recognizerResult = new PromptRecognizerResult<TokenResponse>() 
                { 
                    Succeeded = !string.IsNullOrEmpty(tokenResponse?.Token), 
                    Value = tokenResponse 
                };
                var promptContext = new PromptValidatorContext<TokenResponse>(dc.Context, recognizerResult, promptState, promptOptions);
                isValid = await _validator(promptContext, cancellationToken).ConfigureAwait(false);
            }
            else if (!string.IsNullOrEmpty(tokenResponse?.Token))
            {
                isValid = true;
            }

            // Return recognized value or re-prompt
            if (isValid)
            {
                return await dc.EndDialogAsync(tokenResponse, cancellationToken).ConfigureAwait(false);
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
    }
}
