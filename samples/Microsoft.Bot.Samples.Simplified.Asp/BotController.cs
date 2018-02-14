// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.BotFramework;
using Microsoft.Bot.Builder.Middleware;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Bot.Samples.Simplified.Asp
{
    public class BotController : Controller
    {
        BotFrameworkAdapter _adapter;

        public BotController(IConfiguration configuration)
        {
            var bot = new Builder.Bot(new BotFrameworkAdapter(configuration));
            _adapter = (BotFrameworkAdapter)bot.Adapter;
            bot.OnReceive(BotReceiveHandler);
        }

        private async Task BotReceiveHandler(IBotContext context, MiddlewareSet.NextDelegate next)
        {
            switch (context.Request.Type)
            {
                case ActivityTypes.Message:
                    await CallMessageReceive(context);
                    break;
                case ActivityTypes.ConversationUpdate:
                    await CallConversationUpdateReceive(context);
                    break;
                // etc
                default:
                    break;
            }
        }

        protected virtual async Task CallMessageReceive(IBotContext context)
        {
            var inboundActivity = context.Request.AsMessageActivity();
            foreach (var outboundActivity in await Receive(inboundActivity))
            {
                // ref to https://github.com/Microsoft/botbuilder-dotnet/issues/96
                var t = context.ConversationReference.GetPostToUserMessage();
                t.Text = ((IMessageActivity)outboundActivity).Text;
                context.Reply(t);
            }
        }
        protected virtual async Task CallConversationUpdateReceive(IBotContext context)
        {
            var inboundActivity = context.Request.AsConversationUpdateActivity();
            foreach (var outboundActivity in await Receive(inboundActivity))
            {
                context.Reply(outboundActivity);
            }
        }

        protected virtual Task<List<IActivity>> Receive(IMessageActivity activity)
        {
            return Task.FromResult(new List<IActivity>());
        }
        protected virtual Task<List<IActivity>> Receive(IConversationUpdateActivity activity)
        {
            return Task.FromResult(new List<IActivity>());
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
