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

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Input
{
    public class OAuthInput : Dialog
    {
        private const string PersistedOptions = "options";
        private const string PersistedState = "state";
        private const string PersistedExpires = "expires";
        private const string AttemptCountKey = "AttemptCount";

        // regex to check if code supplied is a 6 digit numerical code (hence, a magic code).
        private readonly Regex _magicCodeRegex = new Regex(@"(\d{6})");

        /// <summary>
        /// Gets or sets the name of the OAuth connection.
        /// </summary>
        /// <value>The name of the OAuth connection.</value>
        public string ConnectionName { get; set; }

        /// <summary>
        /// Gets or sets the title of the sign-in card.
        /// </summary>
        /// <value>The title of the sign-in card.</value>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets any additional text to include in the sign-in card.
        /// </summary>
        /// <value>Any additional text to include in the sign-in card.</value>
        public string Text { get; set; }

        /// <summary>
        /// Gets or sets the number of milliseconds the prompt waits for the user to authenticate.
        /// Default is 900,000 (15 minutes).
        /// </summary>
        /// <value>The number of milliseconds the prompt waits for the user to authenticate.</value>
        public int Timeout { get; set; } = 900000;

        /// <summary>
        /// Gets or sets the property from memory to pass to the calling dialog and to set the return value to.
        /// </summary>
        /// <value>
        /// The property from memory to pass to the calling dialog and to set the return value to.
        /// </value>
        public string Property
        {
            get
            {
                return OutputBinding;
            }

            set
            {
                InputBindings[DialogContextState.DIALOG_VALUE] = value;
                OutputBinding = value;
            }
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
            }

            // Initialize state
            var state = dc.ActiveDialog.State;
            state[PersistedOptions] = opt;
            state[PersistedState] = new Dictionary<string, object>
            {
                { AttemptCountKey, 0 },
            };

            state[PersistedExpires] = DateTime.Now.AddMilliseconds(Timeout);

            // Attempt to get the users token
            if (!(dc.Context.Adapter is IUserTokenProvider adapter))
            {
                throw new InvalidOperationException("OAuthPrompt.Recognize(): not supported by the current adapter");
            }

            var output = await adapter.GetUserTokenAsync(dc.Context, ConnectionName, null, cancellationToken).ConfigureAwait(false);
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
            else
            {
                var promptState = (IDictionary<string, object>)state[PersistedState];
                var promptOptions = (PromptOptions)state[PersistedOptions];

                // Increment attempt count
                // Convert.ToInt32 For issue https://github.com/Microsoft/botbuilder-dotnet/issues/1859
                promptState[AttemptCountKey] = Convert.ToInt32(promptState[AttemptCountKey]) + 1;

                // Validate the return value
                var isValid = false;
                if (recognized.Succeeded)
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
            if (!(turnContext.Adapter is IUserTokenProvider adapter))
            {
                throw new InvalidOperationException("OAuthPrompt.GetUserToken(): not supported by the current adapter");
            }

            return await adapter.GetUserTokenAsync(turnContext, ConnectionName, null, cancellationToken).ConfigureAwait(false);
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
            if (!(turnContext.Adapter is IUserTokenProvider adapter))
            {
                throw new InvalidOperationException("OAuthPrompt.SignOutUser(): not supported by the current adapter");
            }

            // Sign out user
            await adapter.SignOutUserAsync(turnContext, ConnectionName, turnContext.Activity?.From?.Id, cancellationToken).ConfigureAwait(false);
        }

        protected override string OnComputeId()
        {
            return $"OAuthPrompt[{this.BindingPath()}]";
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
                    var link = await adapter.GetOauthSignInLinkAsync(turnContext, ConnectionName, cancellationToken).ConfigureAwait(false);
                    prompt.Attachments.Add(new Attachment
                    {
                        ContentType = SigninCard.ContentType,
                        Content = new SigninCard
                        {
                            Text = Text,
                            Buttons = new[]
                            {
                                new CardAction
                                {
                                    Title = Title,
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
                        Text = Text,
                        ConnectionName = ConnectionName,
                        Buttons = new[]
                        {
                            new CardAction
                            {
                                Title = Title,
                                Text = Text,
                                Type = ActionTypes.Signin,
                            },
                        },
                    },
                });
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

                if (!(turnContext.Adapter is IUserTokenProvider adapter))
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
                    var token = await adapter.GetUserTokenAsync(turnContext, ConnectionName, magicCode, cancellationToken).ConfigureAwait(false);

                    if (token != null)
                    {
                        result.Succeeded = true;
                        result.Value = token;

                        await turnContext.SendActivityAsync(new Activity { Type = ActivityTypesEx.InvokeResponse }, cancellationToken).ConfigureAwait(false);
                    }
                    else
                    {
                        await turnContext.SendActivityAsync(new Activity { Type = ActivityTypesEx.InvokeResponse, Value = new InvokeResponse { Status = 404 } }, cancellationToken).ConfigureAwait(false);
                    }
                }
                catch
                {
                    await turnContext.SendActivityAsync(new Activity { Type = ActivityTypesEx.InvokeResponse, Value = new InvokeResponse { Status = 500 } }, cancellationToken).ConfigureAwait(false);
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

                    var token = await adapter.GetUserTokenAsync(turnContext, ConnectionName, matched.Value, cancellationToken).ConfigureAwait(false);
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
