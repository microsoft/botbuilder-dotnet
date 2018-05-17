// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Prompts;
using Microsoft.Bot.Schema;
using static Microsoft.Bot.Builder.Prompts.PromptValidatorEx;

namespace Microsoft.Bot.Builder.Dialogs
{
    /// <summary>
    /// Prompt that supports management of auth tokens for various service providers.
    /// </summary>
    public class OAuthPrompt : Control, IDialog, IDialogContinue
    {
        private Prompts.OAuthPrompt _prompt;
        private OAuthPromptSettingsWithTimeout _settings;

        // Default prompt timeout of 15 minutes (in ms)
        private const int DefaultPromptTimeout = 54000000;

        public OAuthPrompt(OAuthPromptSettingsWithTimeout settings, PromptValidator<TokenResult> validator = null)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _prompt = new Prompts.OAuthPrompt(settings, validator);
        }

        public async Task DialogBegin(DialogContext dc, object dialogArgs = null)
        {
            if (dc == null)
                throw new ArgumentNullException(nameof(dc));

            PromptOptions promptOptions = null;
            if (dialogArgs != null)
            {
                if (dialogArgs is PromptOptions)
                    promptOptions = dialogArgs as PromptOptions;
                else
                    throw new ArgumentException(nameof(dialogArgs));
            }

            //persist options and state
            var timeout = _settings.Timeout.HasValue ? _settings.Timeout.Value : DefaultPromptTimeout;
            var instance = dc.Instance;
            instance.State = new OAuthPromptOptions(promptOptions);

            var tokenResult = await _prompt.GetUserToken(dc.Context).ConfigureAwait(false);

            if (tokenResult != null && tokenResult.Value != null)
            {
                // end the prompt, since a token is available.
                await dc.End(tokenResult).ConfigureAwait(false);
            }
            else if (!string.IsNullOrEmpty(promptOptions.PromptString))
            {
                // if no token is avaialable, display an oauth/signin card for the user to enter credentials.
                await _prompt.Prompt(dc.Context, promptOptions.PromptString, promptOptions.Speak).ConfigureAwait(false);
            }
            else if (promptOptions.PromptActivity != null)
            {
                // if the bot developer has supplied an activity to show the user for signin, use that.
                await _prompt.Prompt(dc.Context, promptOptions.PromptActivity).ConfigureAwait(false);
            }
            else
            {
                // no suitable promptactivity or message provided. Hence ignoring this prompt.
            }
        }

        public async Task DialogContinue(DialogContext dc)
        {
            if (dc == null)
                throw new ArgumentNullException(nameof(dc));
            //Recognize token
            var tokenResult = await _prompt.Recognize(dc.Context).ConfigureAwait(false);
            //Check for timeout
            var state = dc.Instance.State as OAuthPromptOptions;
            var isMessage = dc.Context.Activity.Type == ActivityTypes.Message;
            var hasTimedOut = isMessage && (DateTime.Compare(DateTime.Now, state.Expires) > 0);

            if (tokenResult == null || hasTimedOut)
            {
                // if the token fetch request timeouts or doesn't return a token, complete the prompt with no result.
                await dc.End(tokenResult).ConfigureAwait(false);
            }
            else if (isMessage && !string.IsNullOrEmpty(state.RetryPromptString))
            {
                // if this is a retry, then retry getting user credentials by resending the activity.
                await dc.Context.SendActivity(state.RetryPromptString, state.RetrySpeak).ConfigureAwait(false);
            }
        }
    }
}