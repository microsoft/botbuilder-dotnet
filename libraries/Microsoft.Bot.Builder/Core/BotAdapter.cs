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
    /// <summary>
    /// Represents a bot adapter that can connect a bot to a service endpoint. 
    /// This class is abstract.
    /// </summary>
    /// <remarks>The bot adapter encapsulates authentication processes and sends 
    /// activities to and receives activities from the Bot Connector Service. When your 
    /// bot receives an activity, the adapter creates a context object, passes it to your 
    /// bot's application logic, and sends responses back to the user's channel.
    /// <para>Use <see cref="Use(IMiddleware)"/> to add <see cref="IMiddleware"/> objects 
    /// to your adapter’s middleware collection. The adapter processes and directs 
    /// incoming activities in through the bot middleware pipeline to your bot’s logic 
    /// and then back out again. As each activity flows in and out of the bot, each piece 
    /// of middleware can inspect or act upon the activity, both before and after the bot 
    /// logic runs.</para>
    /// </remarks>
    /// <seealso cref="ITurnContext"/>
    /// <seealso cref="IActivity"/>
    /// <seealso cref="IBot"/>
    /// <seealso cref="IMiddleware"/>
    public abstract class BotAdapter
    {
        /// <summary>
        /// The collection of middleware in the adapter's pipeline.
        /// </summary>
        protected readonly MiddlewareSet _middlewareSet = new MiddlewareSet();

        /// <summary>
        /// Creates a default adapter.
        /// </summary>
        public BotAdapter() : base()
        {
        }

        /// <summary>
        /// Adds middleware to the adapter's pipeline.
        /// </summary>
        /// <param name="middleware">The middleware to add.</param>
        /// <returns>The updated adapter object.</returns>
        /// <remarks>Middleware is added to the adapter at initialization time.
        /// For each turn, the adapter calls middleware in the order in which you added it.
        /// </remarks>
        public BotAdapter Use(IMiddleware middleware)
        {
            _middlewareSet.Use(middleware);
            return this;
        }

        /// <summary>
        /// When overridden in a derived class, sends activities to the conversation.
        /// </summary>       
        /// <param name="context">The context object for the turn.</param>
        /// <param name="activities">The activities to send.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <remarks>If the activities are successfully sent, the task result contains
        /// an array of <see cref="ResourceResponse"/> objects containing the IDs that 
        /// the receiving channel assigned to the activities.</remarks>
        /// <seealso cref="ITurnContext.OnSendActivities(SendActivitiesHandler)"/>
        public abstract Task<ResourceResponse[]> SendActivities(ITurnContext context, Activity[] activities);

        /// <summary>
        /// When overridden in a derived class, replaces an existing activity in the 
        /// conversation.
        /// </summary>
        /// <param name="context">The context object for the turn.</param>
        /// <param name="activity">New replacement activity.</param>        
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <remarks>If the activity is successfully sent, the task result contains
        /// a <see cref="ResourceResponse"/> object containing the ID that the receiving 
        /// channel assigned to the activity.
        /// <para>Before calling this, set the ID of the replacement activity to the ID
        /// of the activity to replace.</para></remarks>
        /// <seealso cref="ITurnContext.OnUpdateActivity(UpdateActivityHandler)"/>
        public abstract Task<ResourceResponse> UpdateActivity(ITurnContext context, Activity activity);

        /// <summary>
        /// When overridden in a derived class, deletes an existing activity in the 
        /// conversation.
        /// </summary>
        /// <param name="context">The context object for the turn.</param>
        /// <param name="reference">Conversation reference for the activity to delete.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <remarks>The <see cref="ConversationReference.ActivityId"/> of the conversation
        /// reference identifies the activity to delete.</remarks>
        /// <seealso cref="ITurnContext.OnDeleteActivity(DeleteActivityHandler)"/>
        public abstract Task DeleteActivity(ITurnContext context, ConversationReference reference);


        /// <summary>
        /// Starts activity processing for the current bot turn.
        /// </summary>
        /// <param name="context">The turn's context object.</param>
        /// <param name="callback">A callback method to run at the end of the pipeline.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="context"/> is null.</exception>
        /// <remarks>The adapter calls middleware in the order in which you added it. 
        /// The adapter passes in the context object for the turn and a next delegate, 
        /// and the middleware calls the delegate to pass control to the next middleware 
        /// in the pipeline. Once control reaches the end of the pipeline, the adapter calls 
        /// the <paramref name="callback"/> method. If a middleware component doesn’t call 
        /// the next delegate, the adapter does not call  any of the subsequent middleware’s 
        /// <see cref="IMiddleware.OnTurn(ITurnContext, MiddlewareSet.NextDelegate)"/> 
        /// methods or the callback method, and the pipeline short circuits.
        /// <para>When the turn is initiated by a user activity (reactive messaging), the
        /// callback method will be a reference to the bot's 
        /// <see cref="IBot.OnTurn(ITurnContext)"/> method. When the turn is
        /// initiated by a call to <see cref="ContinueConversation(string, ConversationReference, Func{ITurnContext, Task})"/>
        /// (proactive messaging), the callback method is the callback method that was provided in the call.</para>
        /// </remarks>
        protected async Task RunPipeline(ITurnContext context, Func<ITurnContext, Task> callback = null)
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
        /// Creates a conversation on the specified channel.
        /// </summary>
        /// <param name="channelId">The ID of the channel.</param>
        /// <param name="callback">A method to call when the new conversation is available.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <exception cref="NotImplementedException"></exception>
        /// <remarks>No base implementation is provided.</remarks>
        public virtual async Task CreateConversation(string channelId, Func<ITurnContext, Task> callback)
        {
            throw new NotImplementedException("Adapter does not support CreateConversation with this arguments");
        }

        /// <summary>
        /// Sends a proactive message to a conversation.
        /// </summary>
        /// <param name="botId">The application ID of the bot. This paramter is ignored in 
        /// single tenant the Adpters (Console, Test, etc) but is critical to the BotFrameworkAdapter
        /// which is multi-tenant aware. </param>    
        /// <param name="reference">A reference to the conversation to continue.</param>
        /// <param name="callback">The method to call for the resulting bot turn.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <remarks>Call this method to proactively send a message to a conversation.
        /// Most channels require a user to initaiate a conversation with a bot
        /// before the bot can send activities to the user.</remarks>
        /// <seealso cref="RunPipeline(ITurnContext, Func{ITurnContext, Task})"/>
        public virtual Task ContinueConversation(string botId, ConversationReference reference, Func<ITurnContext, Task> callback)
        {
            using (var context = new TurnContext(this, reference.GetPostToBotMessage()))
            {
                return RunPipeline(context, callback);
            }
        }
    }
}
