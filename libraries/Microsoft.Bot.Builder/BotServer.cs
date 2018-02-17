// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Servers;
using Microsoft.Bot.Builder.Middleware;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder
{
    public abstract class BotServer
    {
        protected readonly Middleware.MiddlewareSet _middlewareSet = new Middleware.MiddlewareSet();

        public BotServer() : base()
        {
            this.RegisterMiddleware(new Middleware.BindOutoingResponsesMiddlware());
            this.RegisterMiddleware(new Middleware.TemplateManager());
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
        /// implement send an activity to the conversation
        /// </summary>
        /// <param name="context"></param>
        /// <param name=""></param>
        /// <returns></returns>
        protected abstract Task SendActivityImplementation(IBotContext context, IActivity activity);

        /// <summary>
        /// Implement updating an activity in the conversation
        /// </summary>
        /// <param name="context"></param>
        /// <param name="activity"></param>
        /// <returns></returns>
        protected abstract Task<ResourceResponse> UpdateActivityImplementation(IBotContext context, IActivity activity);

        /// <summary>
        /// Implement deleting an activity in the conversation
        /// </summary>
        /// <param name="context"></param>
        /// <param name="conversationId"></param>
        /// <param name="activityId"></param>
        /// <returns></returns>
        protected abstract Task DeleteActivityImplementation(IBotContext context, string conversationId, string activityId);


        /// <summary>
        /// Implement createconversation semantics
        /// </summary>
        /// <returns></returns>
        protected abstract Task CreateConversationImplementation();


        protected async Task RunPipeline(IBotContext context, Func<IBotContext, Task> callback = null)
        {
            BotAssert.ContextNotNull(context);

            System.Diagnostics.Trace.TraceInformation($"Middleware: Beginning Pipeline for {context.ConversationReference.ActivityId}");

            // Call any registered Middleware Components looking for ContextCreated()
            await _middlewareSet.ContextCreated(context).ConfigureAwait(false);

            // Call any registered Middleware Components looking for ReceiveActivity()
            if (context.Request != null)
            {
                await _middlewareSet.ReceiveActivity(context).ConfigureAwait(false);
            }

            // call back to caller
            if (callback != null)
            {
                await callback(context).ConfigureAwait(false);
            }

            // Call any registered Middleware Components looking for SendActivity()
            if (context.Responses != null && context.Responses.Any())
            {
                await _middlewareSet.SendActivity(context, context.Responses).ConfigureAwait(false);

                foreach (var response in context.Responses)
                {
                    await this.SendActivityImplementation(context, response).ConfigureAwait(false);
                }
            }

            System.Diagnostics.Trace.TraceInformation($"Middleware: Ending Pipeline for {context.ConversationReference.ActivityId}");
        }

        /// <summary>
        /// Process incoming activity (called by outer program
        /// </summary>
        /// <param name="activity"></param>
        /// <returns></returns>
        protected async Task ProcessActivityInternal(IActivity activity, Func<IBotContext, Task> callback = null)
        {
            BotAssert.ActivityNotNull(activity);

            var context = new BotContext(this, activity);

            await RunPipeline(context, callback).ConfigureAwait(false);
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

        /// <summary>
        /// Create proactive context around conversation reference
        /// All middleware pipelines will be processed
        /// </summary>
        /// <param name="reference">reference to create context around</param>
        /// <param name="callback">callback where you can continue the conversation</param>
        /// <returns>task when completed</returns>
        public virtual async Task CreateConversation(string channelId, Func<IBotContext, Task> callback)
        {
            //   var context = new BotContext(this, reference);
            //   await RunPipeline(context, proactiveCallback).ConfigureAwait(false);
        }

    }
}
