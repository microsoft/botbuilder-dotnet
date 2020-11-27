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
using AdaptiveExpressions.Properties;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Templates;
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
        /// <summary>
        /// Class identifier.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "Microsoft.OAuthInput";

        private const string PersistedOptions = "options";
        private const string PersistedState = "state";
        private const string PersistedExpires = "expires";
        private const string AttemptCountKey = "AttemptCount";

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

            state[PersistedExpires] = DateTime.UtcNow.AddMilliseconds(Timeout.GetValue(dc.State));
            OAuthPrompt.SetCallerInfoInDialogState(state, dc.Context);

            // Attempt to get the users token
            if (!(dc.Context.Adapter is IUserTokenProvider adapter))
            {
                throw new InvalidOperationException("OAuthInput.BeginDialog(): not supported by the current adapter");
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
            var isTimeoutActivityType = isMessage
                                        || IsTokenResponseEvent(dc.Context)
                                        || IsTeamsVerificationInvoke(dc.Context)
                                        || IsTokenExchangeRequestInvoke(dc.Context);
            var hasTimedOut = isTimeoutActivityType && (DateTime.Compare(DateTime.UtcNow, expires) > 0);

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
                promptState[AttemptCountKey] = Convert.ToInt32(promptState[AttemptCountKey], CultureInfo.InvariantCulture) + 1;

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
                    if (!interrupted)
                    { 
                        // increase the turnCount as last step
                        dc.State.SetValue(TURN_COUNT_PROPERTY, turnCount + 1);
                        var prompt = await this.OnRenderPromptAsync(dc, inputState, cancellationToken).ConfigureAwait(false);
                        await dc.Context.SendActivityAsync(prompt, cancellationToken).ConfigureAwait(false);
                    }

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
                            var response = await this.DefaultValueResponse.BindAsync(dc, cancellationToken: cancellationToken).ConfigureAwait(false);
                            var properties = new Dictionary<string, string>()
                            {
                                { "template", JsonConvert.SerializeObject(this.DefaultValueResponse) },
                                { "result", response == null ? string.Empty : JsonConvert.SerializeObject(response, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore }) },
                            };
                            TelemetryClient.TrackEvent("GeneratorResult", properties);
                            await dc.Context.SendActivityAsync(response, cancellationToken).ConfigureAwait(false);
                        }

                        // set output property
                        dc.State.SetValue(this.Property.GetValue(dc.State), value);
                        return await dc.EndDialogAsync(value, cancellationToken).ConfigureAwait(false);
                    }
                }

                return await dc.EndDialogAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
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

        /// <summary>
        /// Called when input has been received.
        /// </summary>
        /// <param name="dc">The <see cref="DialogContext"/> for the current turn of conversation.</param>
        /// <param name="cancellationToken">Optional, the <see cref="CancellationToken"/> that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>InputState which reflects whether input was recognized as valid or not.</returns>
        /// <remark>Method not implemented.</remark>
        protected override Task<InputState> OnRecognizeInputAsync(DialogContext dc, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        private Task SendOAuthCardAsync(DialogContext dc, IMessageActivity prompt, CancellationToken cancellationToken)
        {
            var settings = new OAuthPromptSettings { ConnectionName = ConnectionName?.GetValue(dc.State), Title = Title?.GetValue(dc.State), Text = Text?.GetValue(dc.State) };
            return OAuthPrompt.SendOAuthCardAsync(settings, dc.Context, prompt, cancellationToken);
        }

        private Task<PromptRecognizerResult<TokenResponse>> RecognizeTokenAsync(DialogContext dc, CancellationToken cancellationToken)
        {
            var settings = new OAuthPromptSettings { ConnectionName = ConnectionName.GetValue(dc.State) };
            return OAuthPrompt.RecognizeTokenAsync(settings, dc, cancellationToken);
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
    }
}
