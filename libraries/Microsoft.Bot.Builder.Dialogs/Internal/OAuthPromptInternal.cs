// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;
using Newtonsoft.Json.Linq;
using static Microsoft.Bot.Builder.Dialogs.PromptValidatorEx;

namespace Microsoft.Bot.Builder.Dialogs
{
    internal class OAuthPromptInternal
    {
        // regex to check if code supplied is a 6 digit numerical code (hence, a magic code).
        private static readonly Regex MagicCodeRegex = new Regex(@"(\d{6})");

        private readonly OAuthPromptSettings _settings;
        private readonly PromptValidator<TokenResult> _promptValidator;

        public OAuthPromptInternal(OAuthPromptSettings settings, PromptValidator<TokenResult> validator = null)
        {
            _settings = settings ?? throw new ArgumentException(nameof(settings));
            _promptValidator = validator;
        }

        /// <summary>
        /// Prompt the User to signin if not already signed in for the given connection name.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="activity"></param>
        /// <returns></returns>
        public async Task Prompt(ITurnContext context, MessageActivity activity)
        {
            BotAssert.ContextNotNull(context);
            BotAssert.ActivityNotNull(activity);

            var adapter = context.Adapter as BotFrameworkAdapter;
            if (adapter == null)
                throw new InvalidOperationException("OAuthPrompt.Prompt(): not supported by the current adapter");

            if (activity.Attachments == null || activity.Attachments.Count == 0)
                throw new InvalidOperationException("OAuthPrompt.Prompt(): length of attachments cannot be null");

            var cards = activity.Attachments.Where(a => a.Content is OAuthCard);
            if (cards.Count() == 0)
                throw new InvalidOperationException("OAuthPrompt.Prompt(): at least one of the cards should be an oauth card");

            var replyActivity = MessageFactory.Attachment(cards.First());//todo:send an oauth or signin card based on channel id
            await context.SendActivityAsync(replyActivity).ConfigureAwait(false);
        }

        /// <summary>
        /// Prompt the User to signin if not already signed in for the given connection name.
        /// </summary>
        /// <param name="context">The current turn context.</param>
        /// <returns></returns>
        public async Task Prompt(ITurnContext context)
        {
            BotAssert.ContextNotNull(context);

            var adapter = context.Adapter as BotFrameworkAdapter;
            if (adapter == null)
                throw new InvalidOperationException("OAuthPrompt.Prompt(): not supported by the current adapter");

            Attachment cardAttachment = null;

            if (!ChannelSupportsOAuthCard(context.Activity.ChannelId))
            {
                var link = await adapter.GetOauthSignInLinkAsync(context, _settings.ConnectionName, default(CancellationToken)).ConfigureAwait(false);
                cardAttachment = new Attachment
                {
                    ContentType = SigninCard.ContentType,
                    Content = new SigninCard
                    {
                        Text = _settings.Text,
                        Buttons = new []
                        {
                            new CardAction
                            {
                                Title = _settings.Title,
                                Value = link,
                                Type = ActionTypes.Signin
                            }
                        }
                    }
                };
            }
            else
            {
                cardAttachment = new Attachment
                {
                    ContentType = OAuthCard.ContentType,
                    Content = new OAuthCard
                    {
                        Text = _settings.Text,
                        ConnectionName = _settings.ConnectionName,
                        Buttons = new []
                        {
                            new CardAction
                            {
                                Title = _settings.Title,
                                Text = _settings.Text,
                                Type = ActionTypes.Signin
                            }
                        }
                    }
                };
            }
            var replyActivity = MessageFactory.Attachment(cardAttachment);
            await context.SendActivityAsync(replyActivity).ConfigureAwait(false);
        }

        /// <summary>
        /// If user is signed in get token, and optionally run validations on the Token.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task<TokenResult> Recognize(ITurnContext context)
        {
            if (IsTokenResponseEvent(context))
            {
                var tokenResponseObject = (context.Activity as ActivityWithValue).Value as JObject;
                var tokenResponse = tokenResponseObject?.ToObject<TokenResponse>();
                return new TokenResult
                {
                    Status = PromptStatus.Recognized,
                    TokenResponse = tokenResponse
                };
            }
            else if (context.Activity is MessageActivity messageActivity)
            {
                var matched = MagicCodeRegex.Match(messageActivity.Text);
                if (matched.Success)
                {
                    var adapter = context.Adapter as BotFrameworkAdapter;
                    if (adapter == null)
                        throw new InvalidOperationException("OAuthPrompt.Recognize(): not supported by the current adapter");

                    var token = await adapter.GetUserTokenAsync(context, _settings.ConnectionName, matched.Value, default(CancellationToken)).ConfigureAwait(false);
                    var tokenResult = new TokenResult
                    {
                        Status = PromptStatus.Recognized,
                        TokenResponse = token
                    };

                    if (_promptValidator != null)
                    {
                        await _promptValidator(context, tokenResult).ConfigureAwait(false);
                    }

                    return tokenResult;
                }
            }
            return new TokenResult { Status = PromptStatus.NotRecognized };
        }

        /// <summary>
        /// Get a token for a user signed in.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task<TokenResult> GetUserToken(ITurnContext context)
        {
            var adapter = context.Adapter as BotFrameworkAdapter;
            if (adapter == null)
                throw new InvalidOperationException("OAuthPrompt.GetUserToken(): not supported by the current adapter");

            var token = await adapter.GetUserTokenAsync(context, _settings.ConnectionName, null, default(CancellationToken)).ConfigureAwait(false);
            TokenResult tokenResult = null;
            if (token == null)
            {
                tokenResult = new TokenResult
                {
                    Status = PromptStatus.NotRecognized
                };
            }
            else
            {
                tokenResult = new TokenResult
                {
                    Status = PromptStatus.Recognized,
                    TokenResponse = token
                };
            }

            if (_promptValidator != null)
            {
                await _promptValidator(context, tokenResult).ConfigureAwait(false);
            }

            return tokenResult;
        }

        /// <summary>
        /// Sign Out the User.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task SignOutUser(ITurnContext context)
        {
            var adapter = context.Adapter as BotFrameworkAdapter;
            if (adapter == null)
                throw new InvalidOperationException("OAuthPrompt.SignOutUser(): not supported by the current adapter");

            // Sign out user
            await adapter.SignOutUserAsync(context, _settings.ConnectionName, default(CancellationToken)).ConfigureAwait(false);
        }

        private static bool IsTokenResponseEvent(ITurnContext context) =>
            (context.Activity as EventActivity)?.Name == "tokens/response";

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
