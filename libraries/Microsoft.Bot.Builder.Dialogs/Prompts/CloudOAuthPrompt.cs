// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Dialogs
{
    /// <summary>
    /// This is replacement for the OauthPrompt that replies on the UserTokenClient it finds in the TurnState.
    /// </summary>
    internal class CloudOAuthPrompt : Dialog
    {
        private readonly OAuthPromptSettings _settings;
        private readonly PromptValidator<TokenResponse> _validator;

        /// <summary>
        /// Initializes a new instance of the <see cref="CloudOAuthPrompt"/> class.
        /// </summary>
        /// <param name="settings">Additional OAuth settings to use with this instance of the prompt.</param>
        /// <param name="validator">A custom validator that can be used against Message activities.</param>
        public CloudOAuthPrompt(OAuthPromptSettings settings, PromptValidator<TokenResponse> validator = null)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _validator = validator;
        }

        /// <summary>
        /// Called when a prompt dialog is pushed onto the dialog stack and is being activated.
        /// </summary>
        /// <param name="dc">The dialog context for the current turn of the conversation.</param>
        /// <param name="options">Optional, additional information to pass to the prompt being started.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        /// <remarks>If the task is successful, the result indicates whether the prompt is still
        /// active after the turn has been processed by the prompt.</remarks>
        public override async Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default)
        {
            var userTokenClient = dc.Context.TurnState.Get<UserTokenClient>() ?? throw new InvalidOperationException("The UserTokenClient is not supported by the current adapter.");

            var tokenResponse = await userTokenClient.GetUserTokenAsync(dc.Context.Activity.From.Id, _settings.ConnectionName, dc.Context.Activity.ChannelId, null, cancellationToken).ConfigureAwait(false);
            if (tokenResponse != null)
            {
                // We already have a token, no need to show a prompt.
                return await dc.EndDialogAsync(tokenResponse, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                OAuthHelper.InitializeState(_settings.Timeout, dc, options);
                await OAuthHelper.SendOAuthCardAsync(userTokenClient, _settings, dc.Context, null, cancellationToken).ConfigureAwait(false);
                return EndOfTurn;
            }
        }

        /// <summary>
        /// Called when a prompt dialog is the active dialog and the user replied with a new activity.
        /// </summary>
        /// <param name="dc">The dialog context for the current turn of conversation.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        /// <remarks>If the task is successful, the result indicates whether the prompt is still
        /// active after the turn has been processed by the prompt.</remarks>
        public override Task<DialogTurnResult> ContinueDialogAsync(DialogContext dc, CancellationToken cancellationToken = default)
        {
            var userTokenClient = dc.Context.TurnState.Get<UserTokenClient>() ?? throw new InvalidOperationException("The UserTokenClient is not supported by the current adapter.");

            // Check for timeout
            if (OAuthHelper.HasTimeoutExpired(dc))
            {
                return dc.EndDialogAsync(null, cancellationToken);
            }

            if (dc.Context.Activity.Type == ActivityTypes.Message)
            {
                return OnContinueWithMessageActivityAsync(dc, userTokenClient, _settings.ConnectionName, _validator, _settings.EndOnInvalidMessage, cancellationToken);
            }
            else
            {
                return OAuthHelper.OnContinueWithNonMessageActivityAsync(dc, userTokenClient, _settings.ConnectionName, cancellationToken);
            }
        }

        private static async Task<DialogTurnResult> OnContinueWithMessageActivityAsync(DialogContext dc, UserTokenClient userTokenClient, string connectionName, PromptValidator<TokenResponse> validator, bool endOnInvalidMessage, CancellationToken cancellationToken)
        {
            var tokenResponse = await OAuthHelper.CreateTokenResponseFromMessageAsync(userTokenClient, connectionName, dc.Context.Activity, cancellationToken).ConfigureAwait(false);

            // Call any custom validation we might have.
            if (await CheckValidatorAsync(dc, validator, tokenResponse, cancellationToken).ConfigureAwait(false))
            {
                return await dc.EndDialogAsync(tokenResponse, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                // If EndOnInvalidMessage is set, complete the prompt with no result.
                if (endOnInvalidMessage)
                {
                    return await dc.EndDialogAsync(null, cancellationToken).ConfigureAwait(false);
                }
                else
                {
                    // If this was a message Activity we can use the retry prompt
                    var promptOptions = dc.ActiveDialog.State[OAuthHelper.PersistedOptions].CastTo<PromptOptions>();
                    if (!dc.Context.Responded && promptOptions?.RetryPrompt != null)
                    {
                        await dc.Context.SendActivityAsync(promptOptions.RetryPrompt, cancellationToken).ConfigureAwait(false);
                    }

                    return EndOfTurn;
                }
            }
        }

        private static async Task<bool> CheckValidatorAsync(DialogContext dc, PromptValidator<TokenResponse> validator, TokenResponse tokenResponse, CancellationToken cancellationToken)
        {
            const string AttemptCount = "AttemptCount";

            if (validator != null)
            {
                // Increment attempt count.
                var promptState = dc.ActiveDialog.State[OAuthHelper.PersistedState].CastTo<IDictionary<string, object>>();
                promptState[AttemptCount] = Convert.ToInt32(promptState[AttemptCount], CultureInfo.InvariantCulture) + 1;

                // Call the custom validator.
                var recognized = new PromptRecognizerResult<TokenResponse> { Succeeded = tokenResponse != null, Value = tokenResponse };
                var promptOptions = dc.ActiveDialog.State[OAuthHelper.PersistedOptions].CastTo<PromptOptions>();
                var promptContext = new PromptValidatorContext<TokenResponse>(dc.Context, recognized, promptState, promptOptions);
                return await validator(promptContext, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                return tokenResponse != null;
            }
        }
    }
}
