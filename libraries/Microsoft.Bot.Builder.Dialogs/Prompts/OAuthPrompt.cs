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
    /// Creates a new prompt that asks the user to sign in using the Bot Frameworks Single Sign On (SSO) 
    /// service. 
    /// 
    /// @remarks
    /// The prompt will attempt to retrieve the users current token and if the user isn't signed in, it 
    /// will send them an `OAuthCard` containing a button they can press to signin. Depending on the 
    /// channel, the user will be sent through one of two possible signin flows:
    /// 
    /// - The automatic signin flow where once the user signs in and the SSO service will forward the bot 
    /// the users access token using either an `event` or `invoke` activity.
    /// - The "magic code" flow where where once the user signs in they will be prompted by the SSO 
    /// service to send the bot a six digit code confirming their identity. This code will be sent as a 
    /// standard `message` activity.
    /// 
    /// Both flows are automatically supported by the `OAuthPrompt` and the only thing you need to be 
    /// careful of is that you don't block the `event` and `invoke` activities that the prompt might
    /// be waiting on.
    /// 
    /// > [!NOTE]
    /// > You should avoid persisting the access token with your bots other state. The Bot Frameworks 
    /// > SSO service will securely store the token on your behalf. If you store it in your bots state
    /// > it could expire or be revoked in between turns. 
    /// >
    /// > When calling the prompt from within a waterfall step you should use the token within the step
    /// > following the prompt and then let the token go out of scope at the end of your function.
    /// 
    /// #### Prompt Usage
    /// 
    /// When used with your bots `DialogSet` you can simply add a new instance of the prompt as a named
    /// dialog using `DialogSet.add()`. You can then start the prompt from a waterfall step using either
    /// `DialogContext.begin()` or `DialogContext.prompt()`. The user will be prompted to signin as 
    /// needed and their access token will be passed as an argument to the callers next waterfall step: 
    /// 
    /// ```JavaScript
    /// const { DialogSet, OAuthPrompt } = require('botbuilder-dialogs');
    /// 
    /// const dialogs = new DialogSet();
    /// 
    /// dialogs.add('loginPrompt', new OAuthPrompt({
    ///    connectionName: 'GitConnection',
    ///    title: 'Login To GitHub',
    ///    timeout: 300000   // User has 5 minutes to login
    /// }));
    /// 
    /// dialogs.add('taskNeedingLogin', [
    ///      async function (dc) {
    ///          await dc.begin('loginPrompt');
    ///      },
    ///      async function (dc, token) {
    ///          if (token) {
    ///              // Continue with task needing access token
    ///          } else {
    ///              await dc.context.sendActivity(`Sorry... We couldn't log you in. Try again later.`);
    ///              await dc.end();
    ///          }
    ///      }
    ///
    /// </summary>
    public class OAuthPrompt : Dialog
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
            var instance = dc.ActiveDialog;
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
            var state = dc.ActiveDialog.State as OAuthPromptOptions;
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