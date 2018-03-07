// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder
{
    public abstract class BotAdapter
    {
        protected readonly MiddlewareSet _middlewareSet = new MiddlewareSet();

        public BotAdapter() : base()
        {
            //this.RegisterMiddleware(new Middleware.BindOutoingResponsesMiddlware());
        }

        /// <summary>
        /// Register middleware with the bot
        /// </summary>
        /// <param name="middleware"></param>
        public BotAdapter Use(IMiddleware middleware)
        {
            _middlewareSet.Use(middleware);
            return this;
        }

        /// <summary>
        /// implement send activities to the conversation
        /// </summary>        
        /// <param name="activities">Set of activities being sent</param>
        /// <returns></returns>
        public abstract Task SendActivity(params Activity[] activities);

        /// <summary>
        /// Implement updating an activity in the conversation
        /// </summary>        
        /// <param name="activity">New replacement activity. The activity should already have it's ID information populated. </param>
        /// <returns></returns>
        public abstract Task<ResourceResponse> UpdateActivity(Activity activity);

        /// <summary>
        /// Implement deleting an activity in the conversation
        /// </summary>
        /// <param name="reference">Conversation reference of the activity being deleted.  </param>
        /// <returns></returns>
        public abstract Task DeleteActivity(ConversationReference reference);


        /// <summary>
        /// Called by base class to run pipeline around a context
        /// </summary>
        /// <param name="context"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        protected async Task RunPipeline(IBotContext context, Func<IBotContext, Task> callback = null)
        {
            BotAssert.ContextNotNull(context);
            
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
        }


        /// <summary>
        /// Create proactive context around conversation reference
        /// All middleware pipelines will be processed
        /// </summary>
        /// <param name="reference">reference to create context around</param>
        /// <param name="callback">callback where you can continue the conversation</param>
        /// <returns>task when completed</returns>
        //public virtual async Task CreateConversation(string channelId, Func<IBotContext, Task> callback)
        //{
        //    throw new NotImplementedException();
        //}

        /// <summary>
        /// Create proactive context around conversation reference
        /// All middleware pipelines will be processed
        /// </summary>
        /// <param name="reference">reference to create context around</param>
        /// <param name="callback">callback where you can continue the conversation</param>
        /// <returns>task when completed</returns>
        //public virtual async Task ContinueConversation(ConversationReference reference, Func<IBotContext, Task> callback)
        //{
        //    var context = new BotContext(this, reference);
        //    await RunPipeline(context, callback).ConfigureAwait(false);
        //}
    }
}
