// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Globalization;
using System.Security.Claims;
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
        /// The key value for any InvokeResponseActivity that would be on the TurnState.
        /// </summary>
        public const string InvokeResponseKey = "BotFrameworkAdapter.InvokeResponse";

        /// <summary>
        /// The string value for the bot identity key.
        /// </summary>
        public const string BotIdentityKey = "BotIdentity";

        /// <summary>
        /// The string value for the OAuth scope key.
        /// </summary>
        public const string OAuthScopeKey = "Microsoft.Bot.Builder.BotAdapter.OAuthScope";

        /// <summary>
        /// Initializes a new instance of the <see cref="BotAdapter"/> class.
        /// </summary>
        public BotAdapter()
            : base()
        {
        }

        /// <summary>
        /// Gets or sets an error handler that can catch exceptions in the middleware or application.
        /// </summary>
        /// <value>An error handler that can catch exceptions in the middleware or application.</value>
        public Func<ITurnContext, Exception, Task> OnTurnError { get; set; }

        /// <summary>
        /// Gets the collection of middleware in the adapter's pipeline.
        /// </summary>
        /// <value>The middleware collection for the pipeline.</value>
        public MiddlewareSet MiddlewareSet { get; } = new MiddlewareSet();

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
            MiddlewareSet.Use(middleware);
            return this;
        }

        /// <summary>
        /// When overridden in a derived class, sends activities to the conversation.
        /// </summary>
        /// <param name="turnContext">The context object for the turn.</param>
        /// <param name="activities">The activities to send.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <remarks>If the activities are successfully sent, the task result contains
        /// an array of <see cref="ResourceResponse"/> objects containing the IDs that
        /// the receiving channel assigned to the activities.</remarks>
        /// <seealso cref="ITurnContext.OnSendActivities(SendActivitiesHandler)"/>
        public abstract Task<ResourceResponse[]> SendActivitiesAsync(ITurnContext turnContext, Activity[] activities, CancellationToken cancellationToken);

        /// <summary>
        /// When overridden in a derived class, replaces an existing activity in the
        /// conversation.
        /// </summary>
        /// <param name="turnContext">The context object for the turn.</param>
        /// <param name="activity">New replacement activity.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <remarks>If the activity is successfully sent, the task result contains
        /// a <see cref="ResourceResponse"/> object containing the ID that the receiving
        /// channel assigned to the activity.
        /// <para>Before calling this, set the ID of the replacement activity to the ID
        /// of the activity to replace.</para></remarks>
        /// <seealso cref="ITurnContext.OnUpdateActivity(UpdateActivityHandler)"/>
        public abstract Task<ResourceResponse> UpdateActivityAsync(ITurnContext turnContext, Activity activity, CancellationToken cancellationToken);

        /// <summary>
        /// When overridden in a derived class, deletes an existing activity in the
        /// conversation.
        /// </summary>
        /// <param name="turnContext">The context object for the turn.</param>
        /// <param name="reference">Conversation reference for the activity to delete.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <remarks>The <see cref="ConversationReference.ActivityId"/> of the conversation
        /// reference identifies the activity to delete.</remarks>
        /// <seealso cref="ITurnContext.OnDeleteActivity(DeleteActivityHandler)"/>
        public abstract Task DeleteActivityAsync(ITurnContext turnContext, ConversationReference reference, CancellationToken cancellationToken);

        /// <summary>
        /// Sends a proactive message to a conversation.
        /// </summary>
        /// <param name="botId">The application ID of the bot. This parameter is ignored in
        /// single tenant the Adapters (Console, Test, etc) but is critical to the BotFrameworkAdapter
        /// which is multi-tenant aware. </param>
        /// <param name="reference">A reference to the conversation to continue.</param>
        /// <param name="callback">The method to call for the resulting bot turn.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <remarks>Call this method to proactively send a message to a conversation.
        /// Most _channels require a user to initiate a conversation with a bot
        /// before the bot can send activities to the user.</remarks>
        /// <seealso cref="RunPipelineAsync(ITurnContext, BotCallbackHandler, CancellationToken)"/>
        public virtual Task ContinueConversationAsync(string botId, ConversationReference reference, BotCallbackHandler callback, CancellationToken cancellationToken)
        {
            using (var context = new TurnContext(this, reference.GetContinuationActivity()))
            {
                return RunPipelineAsync(context, callback, cancellationToken);
            }
        }

        /// <summary>
        /// Sends a proactive message to a conversation.
        /// </summary>
        /// <param name="claimsIdentity">A <see cref="ClaimsIdentity"/> for the conversation.</param>
        /// <param name="reference">A reference to the conversation to continue.</param>
        /// <param name="callback">The method to call for the resulting bot turn.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <remarks>Call this method to proactively send a message to a conversation.
        /// Most _channels require a user to initiate a conversation with a bot
        /// before the bot can send activities to the user.</remarks>
        /// <seealso cref="RunPipelineAsync(ITurnContext, BotCallbackHandler, CancellationToken)"/>
        public virtual Task ContinueConversationAsync(ClaimsIdentity claimsIdentity, ConversationReference reference, BotCallbackHandler callback, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Sends a proactive message to a conversation.
        /// </summary>
        /// <param name="claimsIdentity">A <see cref="ClaimsIdentity"/> for the conversation.</param>
        /// <param name="reference">A reference to the conversation to continue.</param>
        /// <param name="audience">A value signifying the recipient of the proactive message.</param>
        /// <param name="callback">The method to call for the resulting bot turn.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <remarks>Call this method to proactively send a message to a conversation.
        /// Most _channels require a user to initiate a conversation with a bot
        /// before the bot can send activities to the user.</remarks>
        /// <seealso cref="RunPipelineAsync(ITurnContext, BotCallbackHandler, CancellationToken)"/>
        public virtual Task ContinueConversationAsync(ClaimsIdentity claimsIdentity, ConversationReference reference, string audience, BotCallbackHandler callback, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Sends a proactive message to a conversation.
        /// </summary>
        /// <param name="botId">The application ID of the bot. This parameter is ignored in
        /// single tenant the Adapters (Console, Test, etc) but is critical to the BotFrameworkAdapter
        /// which is multi-tenant aware. </param>
        /// <param name="continuationActivity">An <see cref="Activity"/> with the appropriate <see cref="ConversationReference"/> with which to continue the conversation.</param>
        /// <param name="callback">The method to call for the resulting bot turn.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <remarks>Call this method to proactively send a message to a conversation.
        /// Most _channels require a user to initiate a conversation with a bot
        /// before the bot can send activities to the user.</remarks>
        /// <seealso cref="RunPipelineAsync(ITurnContext, BotCallbackHandler, CancellationToken)"/>
        public virtual Task ContinueConversationAsync(string botId, Activity continuationActivity, BotCallbackHandler callback, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Sends a proactive message to a conversation.
        /// </summary>
        /// <param name="claimsIdentity">A <see cref="ClaimsIdentity"/> for the conversation.</param>
        /// <param name="continuationActivity">An <see cref="Activity"/> with the appropriate <see cref="ConversationReference"/> with which to continue the conversation.</param>
        /// <param name="callback">The method to call for the resulting bot turn.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <remarks>Call this method to proactively send a message to a conversation.
        /// Most _channels require a user to initiate a conversation with a bot
        /// before the bot can send activities to the user.</remarks>
        /// <seealso cref="RunPipelineAsync(ITurnContext, BotCallbackHandler, CancellationToken)"/>
        public virtual Task ContinueConversationAsync(ClaimsIdentity claimsIdentity, Activity continuationActivity, BotCallbackHandler callback, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Sends a proactive message to a conversation.
        /// </summary>
        /// <param name="claimsIdentity">A <see cref="ClaimsIdentity"/> for the conversation.</param>
        /// <param name="continuationActivity">An <see cref="Activity"/> with the appropriate <see cref="ConversationReference"/> with which to continue the conversation.</param>
        /// <param name="audience">A value signifying the recipient of the proactive message.</param>
        /// <param name="callback">The method to call for the resulting bot turn.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <remarks>Call this method to proactively send a message to a conversation.
        /// Most _channels require a user to initiate a conversation with a bot
        /// before the bot can send activities to the user.</remarks>
        /// <seealso cref="RunPipelineAsync(ITurnContext, BotCallbackHandler, CancellationToken)"/>
        public virtual Task ContinueConversationAsync(ClaimsIdentity claimsIdentity, Activity continuationActivity, string audience, BotCallbackHandler callback, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Creates a turn context and runs the middleware pipeline for an incoming TRUSTED activity.
        /// </summary>
        /// <param name="claimsIdentity">A <see cref="ClaimsIdentity"/> for the request.</param>
        /// <param name="activity">The incoming activity.</param>
        /// <param name="callback">The code to run at the end of the adapter's middleware pipeline.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        public virtual Task<InvokeResponse> ProcessActivityAsync(ClaimsIdentity claimsIdentity, Activity activity, BotCallbackHandler callback, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Starts activity processing for the current bot turn.
        /// </summary>
        /// <param name="turnContext">The turn's context object.</param>
        /// <param name="callback">A callback method to run at the end of the pipeline.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="turnContext"/> is null.</exception>
        /// <remarks>The adapter calls middleware in the order in which you added it.
        /// The adapter passes in the context object for the turn and a next delegate,
        /// and the middleware calls the delegate to pass control to the next middleware
        /// in the pipeline. Once control reaches the end of the pipeline, the adapter calls
        /// the <paramref name="callback"/> method. If a middleware component doesn’t call
        /// the next delegate, the adapter does not call  any of the subsequent middleware’s
        /// <see cref="IMiddleware.OnTurnAsync(ITurnContext, NextDelegate, CancellationToken)"/>
        /// methods or the callback method, and the pipeline short circuits.
        /// <para>When the turn is initiated by a user activity (reactive messaging), the
        /// callback method will be a reference to the bot's
        /// <see cref="IBot.OnTurnAsync(ITurnContext, CancellationToken)"/> method. When the turn is
        /// initiated by a call to <see cref="ContinueConversationAsync(string, ConversationReference, BotCallbackHandler, CancellationToken)"/>
        /// (proactive messaging), the callback method is the callback method that was provided in the call.</para>
        /// </remarks>
        protected async Task RunPipelineAsync(ITurnContext turnContext, BotCallbackHandler callback, CancellationToken cancellationToken)
        {
            BotAssert.ContextNotNull(turnContext);

            // Call any registered Middleware Components looking for ReceiveActivityAsync()
            if (turnContext.Activity != null)
            {
                if (turnContext.Activity.Locale != null)
                {
                    try
                    {
                        Thread.CurrentThread.CurrentCulture = new CultureInfo(turnContext.Activity.Locale);
                        if (turnContext is TurnContext ctx)
                        {
                            ctx.Locale = turnContext.Activity.Locale;
                        }
                    }
                    catch (CultureNotFoundException)
                    {
                        // if turnContext.Activity.Locale is invalid, then TurnContext.Locale will set to Thread.CurrentThread.CurrentCulture.Name as default. 
                        (turnContext as TurnContext).Locale = Thread.CurrentThread.CurrentCulture.Name;
                    }
                }

                try
                {
                    await MiddlewareSet.ReceiveActivityWithStatusAsync(turnContext, callback, cancellationToken).ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    if (OnTurnError != null)
                    {
                        await OnTurnError.Invoke(turnContext, e).ConfigureAwait(false);
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            else
            {
                // call back to caller on proactive case
                if (callback != null)
                {
                    await callback(turnContext, cancellationToken).ConfigureAwait(false);
                }
            }
        }
    }
}
