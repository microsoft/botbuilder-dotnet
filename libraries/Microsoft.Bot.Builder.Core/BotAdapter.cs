// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder
{
    public abstract class BotAdapter
    {
        protected readonly MiddlewareSet _middlewareSet = new MiddlewareSet();

        public BotAdapter() : base()
        {
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
        /// Implement send activities to the conversation
        /// </summary>        
        /// <param name="activities">Set of activities being sent</param>
        /// <returns>Array of ResourcesResponse containing the Ids of the sent activities. For
        /// most bots, these Ids are server-generated and enable Update and Delete to be 
        /// called against the remote resources.</returns>
        public abstract Task<ResourceResponse[]> SendActivities(ITurnContext context, Activity[] activities);

        /// <summary>
        /// Implement updating an activity in the conversation
        /// </summary>        
        /// <param name="activity">New replacement activity. The activity should already have it's ID information populated. </param>
        /// <returns></returns>
        /// <returns>ResourcesResponses containing the Id of the sent activity. For
        /// most bots, this Id is server-generated and enables future Update and Delete calls
        /// against the remote resources.</returns>
        public abstract Task<ResourceResponse> UpdateActivity(ITurnContext context, Activity activity);

        /// <summary>
        /// Implement deleting an activity in the conversation
        /// </summary>
        /// <param name="reference">Conversation reference of the activity being deleted.  </param>
        /// <returns></returns>
        public abstract Task DeleteActivity(ITurnContext context, ConversationReference reference);


        /// <summary>
        /// Called by base class to run pipeline around a context
        /// </summary>
        /// <param name="context">Turn context.</param>
        /// <param name="callback">Callback to execute after middlewares are called. </param>
        /// <param name="cancelToken">Cancellation token.</param>
        /// <returns>Task tracking processing.</returns>
        protected async Task RunPipeline(ITurnContext context, Func<ITurnContext, Task> callback = null, CancellationTokenSource cancelToken = null)
        {
            BotAssert.ContextNotNull(context);
            
            // Call any registered Middleware Components looking for ReceiveActivity()
            if (context.Activity != null)
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
        /// Processes the activity.
        /// </summary>
        /// <param name="activity">The activity.</param>
        /// <param name="callback">Callback to execute after middlewares are called. </param>
        /// <param name="cancelToken">Cancellation token.</param>
        /// <returns>Task tracking processing.</returns>
        public abstract Task ProcessActivity(Activity activity, Func<ITurnContext, Task> callback, CancellationTokenSource cancelToken = null);

        /// <summary>
        /// Create proactive context around conversation reference
        /// All middleware pipelines will be processed
        /// </summary>
        /// <param name="channelId">Id of the channel over which the conversation is created.</param>
        /// <param name="conversationParameters">Conversation parameters.</param>
        /// <param name="callback">callback where you can continue the conversation</param>
        /// <returns>task when completed</returns>
        public virtual Task CreateConversation(string channelId, ConversationParameters conversationParameters, Func<ITurnContext, Task> callback)
        {
            throw new NotImplementedException("Adapter does not support CreateConversation with this arguments");
        }

        /// <summary>
        /// Create proactive context around conversation reference
        /// All middleware pipelines will be processed
        /// </summary>
        /// <param name="reference">reference to create context around</param>
        /// <param name="callback">callback where you can continue the conversation</param>
        /// <returns>task when completed</returns>
        public virtual Task ContinueConversation(ConversationReference reference, Func<ITurnContext, Task> callback)
        {
            var context = new TurnContext(this, reference.GetPostToBotMessage());
            return RunPipeline(context, callback);
        }
    }
}
