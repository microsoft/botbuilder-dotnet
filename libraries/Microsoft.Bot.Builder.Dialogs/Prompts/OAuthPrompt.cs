// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;
using Newtonsoft.Json.Linq;

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
    /// .
    /// </summary>
    public class OAuthPrompt : Dialog
    {
        private const string PersistedOptions = "options";
        private const string PersistedState = "state";
        private const string PersistedExpires = "expires";

            // Default prompt timeout of 15 minutes (in ms)
        private const int DefaultPromptTimeout = 54000000;

        // regex to check if code supplied is a 6 digit numerical code (hence, a magic code).
        private readonly Regex magicCodeRegex = new Regex(@"(\d{6})");

        private OAuthPromptSettings _settings;
        private PromptValidator<TokenResponse> _validator;

        public OAuthPrompt(string dialogId, OAuthPromptSettings settings, PromptValidator<TokenResponse> validator = null)
            : base(dialogId)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _validator = validator;
        }

        public override async Task<DialogTurnResult> DialogBeginAsync(DialogContext dc, DialogOptions options = null)
        {
            if (dc == null)
            {
                throw new ArgumentNullException(nameof(dc));
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
                        opt.Prompt.InputHint = InputHints.ExpectingInput;
                    }

                    if (opt.RetryPrompt != null && string.IsNullOrEmpty(opt.RetryPrompt.InputHint))
                    {
                        opt.RetryPrompt.InputHint = InputHints.ExpectingInput;
                    }
                }
                else
                {
                    throw new ArgumentException(nameof(options));
                }
            }

            // Initialize state
            var timeout = _settings.Timeout ?? DefaultPromptTimeout;
            var state = dc.ActiveDialog.State;
            state[PersistedOptions] = opt;
            state[PersistedState] = new Dictionary<string, object>();
            state[PersistedExpires] = DateTime.Now.AddMilliseconds(timeout);

            // Attempt to get the users token
            var output = await GetUserTokenAsync(dc.Context).ConfigureAwait(false);
            if (output != null)
            {
                // Return token
                return await dc.EndAsync(output).ConfigureAwait(false);
            }
            else
            {
                // Prompt user to login
                await SendOAuthCardAsync(dc.Context, opt?.Prompt).ConfigureAwait(false);
                return Dialog.EndOfTurn;
            }
        }

        public override async Task<DialogTurnResult> DialogContinueAsync(DialogContext dc)
        {
            if (dc == null)
            {
                throw new ArgumentNullException(nameof(dc));
            }

            // Recognize token
            var recognized = await RecognizeTokenAsync(dc.Context).ConfigureAwait(false);

            // Check for timeout
            var state = dc.ActiveDialog.State;
            var expires = (DateTime)state[PersistedExpires];
            var isMessage = dc.Context.Activity.Type == ActivityTypes.Message;
            var hasTimedOut = isMessage && (DateTime.Compare(DateTime.Now, expires) > 0);

            if (hasTimedOut)
            {
                // if the token fetch request timesout, complete the prompt with no result.
                return await dc.EndAsync().ConfigureAwait(false);
            }
            else
            {
                var promptState = (IDictionary<string, object>)state[PersistedState];
                var promptOptions = (PromptOptions)state[PersistedOptions];

                // Validate the return value
                var end = false;
                object endResult = null;
                if (_validator != null)
                {
                    var prompt = new PromptValidatorContext<TokenResponse>(dc, promptState, promptOptions, recognized);
                    await _validator(dc.Context, prompt).ConfigureAwait(false);
                    end = prompt.HasEnded;
                    endResult = prompt.EndResult;
                }
                else if (recognized.Succeeded)
                {
                    end = true;
                    endResult = recognized.Value;
                }

                // Return recognized value or re-prompt
                if (end)
                {
                    return await dc.EndAsync(endResult).ConfigureAwait(false);
                }
                else
                {
                    if (!dc.Context.Responded && isMessage && promptOptions != null && promptOptions.RetryPrompt != null)
                    {
                        await dc.Context.SendActivityAsync(promptOptions.RetryPrompt).ConfigureAwait(false);
                    }

                    return Dialog.EndOfTurn;
                }
            }
        }

        /// <summary>
        /// Get a token for a user signed in.
        /// </summary>
        /// <param name="context">Context for the current turn of the conversation with the user.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<TokenResponse> GetUserTokenAsync(ITurnContext context)
        {
            if (!(context.Adapter is BotFrameworkAdapter adapter))
            {
                throw new InvalidOperationException("OAuthPrompt.GetUserToken(): not supported by the current adapter");
            }

            return await adapter.GetUserTokenAsync(context, _settings.ConnectionName, null, default(CancellationToken)).ConfigureAwait(false);
        }

        /// <summary>
        /// Sign Out the User.
        /// </summary>
        /// <param name="context">Context for the current turn of the conversation with the user.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task SignOutUserAsync(ITurnContext context)
        {
            if (!(context.Adapter is BotFrameworkAdapter adapter))
            {
                throw new InvalidOperationException("OAuthPrompt.SignOutUser(): not supported by the current adapter");
            }

            // Sign out user
            await adapter.SignOutUserAsync(context, _settings.ConnectionName, default(CancellationToken)).ConfigureAwait(false);
        }

        private async Task SendOAuthCardAsync(ITurnContext context, IMessageActivity prompt)
        {
            BotAssert.ContextNotNull(context);

            if (!(context.Adapter is BotFrameworkAdapter adapter))
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
            if (!ChannelSupportsOAuthCard(context.Activity.ChannelId))
            {
                if (!prompt.Attachments.Any(a => a.Content is SigninCard))
                {
                    var link = await adapter.GetOauthSignInLinkAsync(context, _settings.ConnectionName, default(CancellationToken)).ConfigureAwait(false);
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
                                    Value = link,
                                    Type = ActionTypes.Signin,
                                },
                            },
                        },
                    });
                }
            }
            else if (!prompt.Attachments.Any(a => a.Content is OAuthCard))
            {
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
                                Type = ActionTypes.Signin,
                            },
                        },
                    },
                });
            }

            // Set input hint
            if (string.IsNullOrEmpty(prompt.InputHint))
            {
                prompt.InputHint = InputHints.ExpectingInput;
            }

            await context.SendActivityAsync(prompt).ConfigureAwait(false);
        }

        private async Task<PromptRecognizerResult<TokenResponse>> RecognizeTokenAsync(ITurnContext context)
        {
            var result = new PromptRecognizerResult<TokenResponse>();
            if (IsTokenResponseEvent(context))
            {
                var tokenResponseObject = context.Activity.Value as JObject;
                var token = tokenResponseObject?.ToObject<TokenResponse>();
                result.Succeeded = true;
                result.Value = token;
            }
            else if (IsTeamsVerificationInvoke(context))
            {
                // TODO: add missing code
            }
            else if (context.Activity.Type == ActivityTypes.Message)
            {
                var matched = magicCodeRegex.Match(context.Activity.Text);
                if (matched.Success)
                {
                    if (!(context.Adapter is BotFrameworkAdapter adapter))
                    {
                        throw new InvalidOperationException("OAuthPrompt.Recognize(): not supported by the current adapter");
                    }

                    var token = await adapter.GetUserTokenAsync(context, _settings.ConnectionName, matched.Value, default(CancellationToken)).ConfigureAwait(false);
                    if (token != null)
                    {
                        result.Succeeded = true;
                        result.Value = token;
                    }
                }
            }

            return result;
        }

        private bool IsTokenResponseEvent(ITurnContext context)
        {
            var activity = context.Activity;
            return activity.Type == ActivityTypes.Event && activity.Name == "tokens/response";
        }

        private bool IsTeamsVerificationInvoke(ITurnContext context)
        {
            var activity = context.Activity;
            return activity.Type == ActivityTypes.Invoke && activity.Name == "signin/verifyState";
        }

        private bool ChannelSupportsOAuthCard(string channelId)
        {
            switch (channelId)
            {
                case "msteams":
                case "cortana":
                case "skype":
                case "skypeforbusiness":
                    return false;
            }

            return true;
        }
    }
}
