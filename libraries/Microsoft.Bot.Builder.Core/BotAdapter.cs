// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Middleware;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder
{
    public abstract class BotAdapter
    {
        protected readonly Middleware.MiddlewareSet _middlewareSet = new Middleware.MiddlewareSet();

        public BotAdapter() : base()
        {
            this.RegisterMiddleware(new Middleware.BindOutoingResponsesMiddlware());
        }

        /// <summary>
        /// Register middleware with the bot
        /// </summary>
        /// <param name="middleware"></param>
        public void RegisterMiddleware(IMiddleware middleware)
        {
            _middlewareSet.Use(middleware);
        }

        /// <summary>
        /// implement send activities to the conversation
        /// </summary>
        /// <param name="context"></param>
        /// <param name=""></param>
        /// <returns></returns>
        public abstract Task SendActivities(IBotContext context, IEnumerable<Activity> activities);

        /// <summary>
        /// Implement updating an activity in the conversation
        /// </summary>
        /// <param name="context"></param>
        /// <param name="activity"></param>
        /// <returns></returns>
        public abstract Task<ResourceResponse> UpdateActivity(IBotContext context, Activity activity);

        /// <summary>
        /// Implement deleting an activity in the conversation
        /// </summary>
        /// <param name="context"></param>
        /// <param name="conversationId"></param>
        /// <param name="activityId"></param>
        /// <returns></returns>
        public abstract Task DeleteActivity(IBotContext context, string conversationId, string activityId);


        /// <summary>
        /// Called by base class to run pipeline around a context
        /// </summary>
        /// <param name="context"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        protected async Task RunPipeline(IBotContext context, Func<IBotContext, Task> callback = null, CancellationTokenSource cancelToken = null)
        {
            BotAssert.ContextNotNull(context);

            System.Diagnostics.Trace.TraceInformation($"Middleware: Beginning Pipeline for {context.ConversationReference.ActivityId}");

            // Call any registered Middleware Components looking for ContextCreated()
            await _middlewareSet.ContextCreated(context).ConfigureAwait(false);

            // Call any registered Middleware Components looking for ReceiveActivity()
            if (context.Request != null)
            {
                await _middlewareSet.ReceiveActivityWithStatus(context, callback).ConfigureAwait(false);
            }
            else
            {
                // call back to caller on proactive case
                if (callback != null)
                {
                    await callback(context).ConfigureAwait(false);
                }
            }

            // Call any registered Middleware Components looking for SendActivity()
            await _middlewareSet.SendActivity(context, context.Responses ?? new List<Activity>()).ConfigureAwait(false);

            if (context.Responses != null)
            {
                await this.SendActivities(context, context.Responses).ConfigureAwait(false);
            }

            System.Diagnostics.Trace.TraceInformation($"Middleware: Ending Pipeline for {context.ConversationReference.ActivityId}");
        }


        /// <summary>
        /// Create proactive context around conversation reference
        /// All middleware pipelines will be processed
        /// </summary>
        /// <param name="reference">reference to create context around</param>
        /// <param name="callback">callback where you can continue the conversation</param>
        /// <returns>task when completed</returns>
        public virtual async Task CreateConversation(string channelId, Func<IBotContext, Task> callback)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Create proactive context around conversation reference
        /// All middleware pipelines will be processed
        /// </summary>
        /// <param name="reference">reference to create context around</param>
        /// <param name="callback">callback where you can continue the conversation</param>
        /// <returns>task when completed</returns>
        public virtual async Task ContinueConversation(ConversationReference reference, Func<IBotContext, Task> callback)
        {
            var context = new BotContext(this, reference);
            await RunPipeline(context, callback).ConfigureAwait(false);
        }
    }
}
