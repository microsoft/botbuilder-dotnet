// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.BotFramework;
using Microsoft.Bot.Builder.Middleware;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Samples.Simplified.Asp
{
    public class BotController : Controller
    {
        BotFrameworkAdapter _adapter;

        public BotController(Builder.Bot bot)
        {
            _adapter = (BotFrameworkAdapter)bot.Adapter;
            bot.OnReceive(BotReceiveHandler);
        }

        private async Task BotReceiveHandler(IBotContext context)
        {
            switch (context.Request.Type)
            {
                case ActivityTypes.Message:
                    await ReceiveMessage(context, context.Request.AsMessageActivity());
                    break;
                case ActivityTypes.ConversationUpdate:
                    await ReceiveConversationUpdate(context, context.Request.AsConversationUpdateActivity());
                    break;
                case ActivityTypes.ContactRelationUpdate:
                    await ReceiveContactRelationUpdate(context, context.Request.AsContactRelationUpdateActivity());
                    break;
                case ActivityTypes.InstallationUpdate:
                    await ReceiveInstallationUpdate(context, context.Request.AsInstallationUpdateActivity());
                    break;
                case ActivityTypes.Typing:
                    await ReceiveTyping(context, context.Request.AsTypingActivity());
                    break;
                case ActivityTypes.EndOfConversation:
                    await ReceiveEndOfConversation(context, context.Request.AsEndOfConversationActivity());
                    break;
                case ActivityTypes.Event:
                    await ReceiveEvent(context, context.Request.AsEventActivity());
                    break;
                case ActivityTypes.Invoke:
                    await ReceiveInvoke(context, context.Request.AsInvokeActivity());
                    break;
                case ActivityTypes.MessageUpdate:
                    await ReceiveMessageUpdate(context, context.Request.AsMessageUpdateActivity());
                    break;
                case ActivityTypes.MessageDelete:
                    await ReceiveMessageDelete(context, context.Request.AsMessageDeleteActivity());
                    break;
                case ActivityTypes.MessageReaction:
                    await ReceiveMessageReaction(context, context.Request.AsMessageReactionActivity());
                    break;
                case ActivityTypes.Suggestion:
                    await ReceiveSuggestion(context, context.Request.AsSuggestionActivity());
                    break;
                default:
                    await ReceiveUnknown(context);
                    break;
            }
        }

        protected virtual Task ReceiveSuggestion(IBotContext context, ISuggestionActivity suggestionActivity)
        {
            return Task.CompletedTask;
        }

        protected virtual Task ReceiveMessageReaction(IBotContext context, IMessageReactionActivity messageReactionActivity)
        {
            return Task.CompletedTask;
        }

        protected virtual Task ReceiveMessageDelete(IBotContext context, IMessageDeleteActivity messageDeleteActivity)
        {
            return Task.CompletedTask;
        }

        protected virtual Task ReceiveMessageUpdate(IBotContext context, IMessageUpdateActivity messageUpdateActivity)
        {
            return Task.CompletedTask;
        }

        protected virtual Task ReceiveInvoke(IBotContext context, IInvokeActivity invokeActivity)
        {
            return Task.CompletedTask;
        }

        protected virtual Task ReceiveEvent(IBotContext context, IEventActivity eventActivity)
        {
            return Task.CompletedTask;
        }

        protected virtual Task ReceiveEndOfConversation(IBotContext context, IEndOfConversationActivity endOfConversationActivity)
        {
            return Task.CompletedTask;
        }

        protected virtual Task ReceiveTyping(IBotContext context, ITypingActivity typingActivity)
        {
            return Task.CompletedTask;
        }

        protected virtual Task ReceiveInstallationUpdate(IBotContext context, IInstallationUpdateActivity installationUpdateActivity)
        {
            return Task.CompletedTask;
        }

        protected virtual Task ReceiveContactRelationUpdate(IBotContext context, IContactRelationUpdateActivity contactRelationUpdateActivity)
        {
            return Task.CompletedTask;
        }

        protected virtual Task ReceiveMessage(IBotContext context, IMessageActivity activity)
        {
            return Task.CompletedTask;
        }

        protected virtual Task ReceiveConversationUpdate(IBotContext context, IConversationUpdateActivity activity)
        {
            return Task.CompletedTask;
        }

        protected virtual Task ReceiveUnknown(IBotContext context)
        {
            return Task.CompletedTask;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody]Activity activity)
        {
            try
            {
                await _adapter.Receive(this.Request.Headers["Authorization"].FirstOrDefault(), activity);
                return this.Ok();
            }
            catch (UnauthorizedAccessException)
            {
                return this.Unauthorized();
            }
        }
    }
}
