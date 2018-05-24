// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Core.Extensions;
using Microsoft.Bot.Schema;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static Microsoft.Bot.Builder.Prompts.PromptValidatorEx;

namespace Microsoft.Bot.Builder.Prompts
{
    public class OAuthPromptSettings
    {
        public string ConnectionName { get; set; }
        public string Title { get; set; }
        public string Text { get; set; }
    }

    public class TokenResult : PromptResult
    {
        public TokenResponse Value { get; set; }
    }

    public class OAuthPrompt
    {
        // regex to check if code supplied is a 6 digit numerical code (hence, a magic code).
        private readonly Regex magicCodeRegex = new Regex(@"(\d{6})");

        private readonly OAuthPromptSettings _settings;
        private readonly PromptValidator<TokenResult> _promptValidator;

        public OAuthPrompt(OAuthPromptSettings settings, PromptValidator<TokenResult> validator = null)
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
        public async Task Prompt(ITurnContext context, IMessageActivity activity)
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
            await context.SendActivity(replyActivity).ConfigureAwait(false);
        }

        /// <summary>
        /// Prompt the User to signin if not already signed in for the given connection name.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="text"></param>
        /// <param name="speak"></param>
        /// <returns></returns>
        public async Task Prompt(ITurnContext context, string text, string speak = null)
        {
            BotAssert.ContextNotNull(context);
            if (string.IsNullOrWhiteSpace(text))
                throw new ArgumentNullException(nameof(text));

            await context.SendActivity(text, speak).ConfigureAwait(false);

            var adapter = context.Adapter as BotFrameworkAdapter;
            if (adapter == null)
                throw new InvalidOperationException("OAuthPrompt.Prompt(): not supported by the current adapter");

            Attachment cardAttachment = null;

            if (ChannelSupportsOAuthCard(context.Activity.ChannelId))
            {
                var link = await adapter.GetOauthSignInLink(context, _settings.ConnectionName).ConfigureAwait(false);
                cardAttachment = new Attachment()
                {
                    ContentType = SigninCard.ContentType,
                    Content = new SigninCard()
                    {
                        Text = _settings.Text,
                        Buttons = new CardAction[]
                        {
                        new CardAction() {Title = _settings.Title, Value = link, Type = ActionTypes.Signin }
                        }
                    }
                };
            }
            else
            {
                cardAttachment = new Attachment()
                {
                    ContentType = OAuthCard.ContentType,
                    Content = new OAuthCard()
                    {
                        Text = _settings.Text,
                        ConnectionName = _settings.ConnectionName,
                        Buttons = new CardAction[]
                        {
                        new CardAction() {Title = _settings.Title, Text = _settings.Text, Type = ActionTypes.Signin }
                        }
                    }
                };
            }
            var replyActivity = MessageFactory.Attachment(cardAttachment);
            await context.SendActivity(replyActivity).ConfigureAwait(false);
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
                var tokenResponse = context.Activity.Value as TokenResponse;
                return new TokenResult() { Status = PromptStatus.Recognized, Value = tokenResponse };
            }
            else if (context.Activity.Type == ActivityTypes.Message)
            {
                var matched = magicCodeRegex.Match(context.Activity.Text);
                if (matched.Success)
                {
                    var adapter = context.Adapter as BotFrameworkAdapter;
                    if (adapter == null)
                        throw new InvalidOperationException("OAuthPrompt.Recognize(): not supported by the current adapter");
                    var token = await adapter.GetUserToken(context, _settings.ConnectionName, matched.Value).ConfigureAwait(false);
                    var tokenResult = new TokenResult() { Status = PromptStatus.Recognized, Value = token };
                    if (_promptValidator != null)
                        await _promptValidator(context, tokenResult).ConfigureAwait(false);
                    return tokenResult;
                }
            }
            return new TokenResult() { Status = PromptStatus.NotRecognized };
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

            var token = await adapter.GetUserToken(context, _settings.ConnectionName, null).ConfigureAwait(false);
            TokenResult tokenResult = null;
            if (token == null)
            {
                tokenResult = new TokenResult()
                {
                    Status = PromptStatus.NotRecognized
                };
            }
            else
            {
                tokenResult = new TokenResult()
                {
                    Status = PromptStatus.Recognized,
                    Value = token
                };
            }
            if (_promptValidator != null)
                await _promptValidator(context, tokenResult).ConfigureAwait(false);
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
            await adapter.SignOutUser(context, _settings.ConnectionName).ConfigureAwait(false);
        }

        private bool IsTokenResponseEvent(ITurnContext context)
        {
            var activity = context.Activity;
            return (activity.Type == ActivityTypes.Event && activity.Name == "tokens/response");
        }

        private bool ChannelSupportsOAuthCard(string channelId)
        {
            switch (channelId)
            {
                case "msteams":
                case "cortana":
                case "skype":
                case "skypeforbusiness":
                    return true;
            }

            return false;
        }
    }
}