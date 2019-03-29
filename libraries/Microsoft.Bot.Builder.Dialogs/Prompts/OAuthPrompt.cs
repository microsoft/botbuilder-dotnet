// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Connector;
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
    /// needed and their access token will be passed as an argument to the callers next waterfall step.
    /// </summary>
    public class OAuthPrompt : Dialog
    {
        private const string PersistedOptions = "options";
        private const string PersistedState = "state";
        private const string PersistedExpires = "expires";

        // Default prompt timeout of 15 minutes (in ms)
        private const int DefaultPromptTimeout = 900000;

        // regex to check if code supplied is a 6 digit numerical code (hence, a magic code).
        private readonly Regex _magicCodeRegex = new Regex(@"(\d{6})");

        private OAuthPromptSettings _settings;
        private PromptValidator<TokenResponse> _validator;

        public OAuthPrompt(string dialogId, OAuthPromptSettings settings, PromptValidator<TokenResponse> validator = null)
            : base(dialogId)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _validator = validator;
        }

        public override async Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default(CancellationToken))
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
            var output = await GetUserTokenAsync(dc.Context, cancellationToken).ConfigureAwait(false);
            if (output != null)
            {
                // Return token
                return await dc.EndDialogAsync(output, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                // Prompt user to login
                await SendOAuthCardAsync(dc.Context, opt?.Prompt, cancellationToken).ConfigureAwait(false);
                return Dialog.EndOfTurn;
            }
        }

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
                // if the token fetch request timesout, complete the prompt with no result.
                return await dc.EndDialogAsync(cancellationToken).ConfigureAwait(false);
            }
            else
            {
                var promptState = (IDictionary<string, object>)state[PersistedState];
                var promptOptions = (PromptOptions)state[PersistedOptions];

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
                else
                {
                    if (!dc.Context.Responded && isMessage && promptOptions != null && promptOptions.RetryPrompt != null)
                    {
                        await dc.Context.SendActivityAsync(promptOptions.RetryPrompt, cancellationToken).ConfigureAwait(false);
                    }

                    return Dialog.EndOfTurn;
                }
            }
        }

        /// <summary>
        /// Get a token for a user signed in.
        /// </summary>
        /// <param name="turnContext">Context for the current turn of the conversation with the user.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<TokenResponse> GetUserTokenAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            string magicCode = null;
            if (!(turnContext.Adapter is IUserTokenProvider adapter))
            {
                throw new InvalidOperationException("OAuthPrompt.GetUserToken(): not supported by the current adapter");
            }

            if (IsTeamsVerificationInvoke(turnContext))
            {
                var value = turnContext.Activity.Value as JObject;
                magicCode = value.GetValue("state")?.ToString();
            }

            if (turnContext.Activity.Type == ActivityTypes.Message && turnContext.Activity.Text != null && _magicCodeRegex.IsMatch(turnContext.Activity.Text))
            {
                magicCode = turnContext.Activity.Text;
            }

            return await adapter.GetUserTokenAsync(turnContext, _settings.ConnectionName, magicCode, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Sign Out the User.
        /// </summary>
        /// <param name="turnContext">Context for the current turn of the conversation with the user.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task SignOutUserAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (!(turnContext.Adapter is IUserTokenProvider adapter))
            {
                throw new InvalidOperationException("OAuthPrompt.SignOutUser(): not supported by the current adapter");
            }

            // Sign out user
            await adapter.SignOutUserAsync(turnContext, _settings.ConnectionName, turnContext.Activity?.From?.Id, cancellationToken).ConfigureAwait(false);
        }

        private async Task SendOAuthCardAsync(ITurnContext turnContext, IMessageActivity prompt, CancellationToken cancellationToken = default(CancellationToken))
        {
            BotAssert.ContextNotNull(turnContext);

            if (!(turnContext.Adapter is IUserTokenProvider adapter))
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
                    var link = await adapter.GetOauthSignInLinkAsync(turnContext, _settings.ConnectionName, cancellationToken).ConfigureAwait(false);
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
                                    Type = turnContext.Activity.ChannelId == "msteams" ? ActionTypes.Signin : ActionTypes.OpenUrl,
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

                if (!(turnContext.Adapter is IUserTokenProvider adapter))
                {
                    throw new InvalidOperationException("OAuthPrompt.Recognize(): not supported by the current adapter");
                }

                var token = await adapter.GetUserTokenAsync(turnContext, _settings.ConnectionName, magicCode, cancellationToken).ConfigureAwait(false);
                if (token != null)
                {
                    result.Succeeded = true;
                    result.Value = token;
                }
            }
            else if (turnContext.Activity.Type == ActivityTypes.Message)
            {
                var matched = _magicCodeRegex.Match(turnContext.Activity.Text);
                if (matched.Success)
                {
                    if (!(turnContext.Adapter is IUserTokenProvider adapter))
                    {
                        throw new InvalidOperationException("OAuthPrompt.Recognize(): not supported by the current adapter");
                    }

                    var token = await adapter.GetUserTokenAsync(turnContext, _settings.ConnectionName, matched.Value, cancellationToken).ConfigureAwait(false);
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
            return activity.Type == ActivityTypes.Event && activity.Name == "tokens/response";
        }

        private bool IsTeamsVerificationInvoke(ITurnContext turnContext)
        {
            var activity = turnContext.Activity;
            return activity.Type == ActivityTypes.Invoke && activity.Name == "signin/verifyState";
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
    }
}
