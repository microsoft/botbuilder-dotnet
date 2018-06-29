// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder
{
    /// <summary>
    /// Provides context for a turn of a bot.
    /// </summary>
    /// <remarks>Context provides information needed to process an incoming activity.
    /// The context object is created by a <see cref="BotAdapter"/> and persists for the 
    /// length of the turn.</remarks>
    /// <seealso cref="IBot"/>
    /// <seealso cref="IMiddleware"/>
    public class TurnContext : ITurnContext, IDisposable
    {
        private readonly IList<SendActivitiesHandler> _onSendActivities = new List<SendActivitiesHandler>();
        private readonly IList<UpdateActivityHandler> _onUpdateActivity = new List<UpdateActivityHandler>();
        private readonly IList<DeleteActivityHandler> _onDeleteActivity = new List<DeleteActivityHandler>();

        /// <summary>
        /// Creates a context object.
        /// </summary>
        /// <param name="adapter">The adapter creating the context.</param>
        /// <param name="activity">The incoming activity for the turn;
        /// or <c>null</c> for a turn for a proactive message.</param>
        /// <exception cref="ArgumentNullException"><paramref name="activity"/> or
        /// <paramref name="adapter"/> is <c>null</c>.</exception>
        /// <remarks>For use by bot adapter implementations only.</remarks>
        public TurnContext(BotAdapter adapter, Activity activity)
        {
            Adapter = adapter ?? throw new ArgumentNullException(nameof(adapter));
            Activity = activity ?? throw new ArgumentNullException(nameof(activity));
        }

        /// <summary>
        /// Adds a response handler for send activity operations.
        /// </summary>
        /// <param name="handler">The handler to add to the context object.</param>
        /// <returns>The updated context object.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="handler"/> is <c>null</c>.</exception>
        /// <remarks>When the context's <see cref="SendActivity(IActivity)"/> 
        /// or <see cref="SendActivities(IActivity[])"/> methods are called, 
        /// the adapter calls the registered handlers in the order in which they were 
        /// added to the context object.
        /// </remarks>
        public ITurnContext OnSendActivities(SendActivitiesHandler handler)
        {
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            _onSendActivities.Add(handler);
            return this;
        }

        /// <summary>
        /// Adds a response handler for update activity operations.
        /// </summary>
        /// <param name="handler">The handler to add to the context object.</param>
        /// <returns>The updated context object.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="handler"/> is <c>null</c>.</exception>
        /// <remarks>When the context's <see cref="UpdateActivity(IActivity)"/> is called, 
        /// the adapter calls the registered handlers in the order in which they were 
        /// added to the context object.
        /// </remarks>
        public ITurnContext OnUpdateActivity(UpdateActivityHandler handler)
        {
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            _onUpdateActivity.Add(handler);
            return this;
        }

        /// <summary>
        /// Adds a response handler for delete activity operations.
        /// </summary>
        /// <param name="handler">The handler to add to the context object.</param>
        /// <returns>The updated context object.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="handler"/> is <c>null</c>.</exception>
        /// <remarks>When the context's <see cref="DeleteActivity(string)"/> is called, 
        /// the adapter calls the registered handlers in the order in which they were 
        /// added to the context object.
        /// </remarks>
        public ITurnContext OnDeleteActivity(DeleteActivityHandler handler)
        {
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            _onDeleteActivity.Add(handler);
            return this;
        }

        /// <summary>
        /// Gets the bot adapter that created this context object.
        /// </summary>
        public BotAdapter Adapter { get; }

        /// <summary>
        /// Gets the services registered on this context object.
        /// </summary>
        public TurnContextServiceCollection Services { get; } = new TurnContextServiceCollection();

        /// <summary>
        /// Gets the activity associated with this turn; or <c>null</c> when processing
        /// a proactive message.
        /// </summary>
        public Activity Activity { get; }

        /// <summary>
        /// Indicates whether at least one response was sent for the current turn.
        /// </summary>
        /// <value><c>true</c> if at least one response was sent for the current turn.</value>
        public bool Responded
        {
            get;
            private set;
        }

        /// <summary>
        /// Sends a message activity to the sender of the incoming activity.
        /// </summary>
        /// <param name="textReplyToSend">The text of the message to send.</param>
        /// <param name="speak">Optional, text to be spoken by your bot on a speech-enabled 
        /// channel.</param>
        /// <param name="inputHint">Optional, indicates whether your bot is accepting, 
        /// expecting, or ignoring user input after the message is delivered to the client.
        /// One of: "acceptingInput", "ignoringInput", or "expectingInput".
        /// Default is null.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="textReplyToSend"/> is <c>null</c> or whitespace.</exception>
        /// <remarks>If the activity is successfully sent, the task result contains
        /// a <see cref="ResourceResponse"/> object containing the ID that the receiving 
        /// channel assigned to the activity.
        /// <para>See the channel's documentation for limits imposed upon the contents of 
        /// <paramref name="textReplyToSend"/>.</para>
        /// <para>To control various characteristics of your bot's speech such as voice, 
        /// rate, volume, pronunciation, and pitch, specify <paramref name="speak"/> in 
        /// Speech Synthesis Markup Language (SSML) format.</para>
        /// </remarks>
        public async Task<ResourceResponse> SendActivity(string textReplyToSend, string speak = null, string inputHint = null)
        {
            if (string.IsNullOrWhiteSpace(textReplyToSend))
                throw new ArgumentNullException(nameof(textReplyToSend));

            var activityToSend = new Activity(ActivityTypes.Message) { Text = textReplyToSend };

            if (!string.IsNullOrEmpty(speak))
                activityToSend.Speak = speak;

            if (!string.IsNullOrEmpty(inputHint))
                activityToSend.InputHint = inputHint;

            return await SendActivity(activityToSend).ConfigureAwait(false);
        }

        /// <summary>
        /// Sends an activity to the sender of the incoming activity.
        /// </summary>
        /// <param name="activity">The activity to send.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="activity"/> is <c>null</c>.</exception>
        /// <remarks>If the activity is successfully sent, the task result contains
        /// a <see cref="ResourceResponse"/> object containing the ID that the receiving 
        /// channel assigned to the activity.</remarks>
        public async Task<ResourceResponse> SendActivity(IActivity activity)
        {
            BotAssert.ActivityNotNull(activity);

            ResourceResponse[] responses = await SendActivities(new [] { activity }).ConfigureAwait(false);
            if (responses == null || responses.Length == 0)
            {
                // It's possible an interceptor prevented the activity from having been sent. 
                // Just return an empty response in that case. 
                return new ResourceResponse();
            }
            else
            {
                return responses[0];
            }
        }

        /// <summary>
        /// Sends a set of activities to the sender of the incoming activity.
        /// </summary>
        /// <param name="activities">The activities to send.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <remarks>If the activities are successfully sent, the task result contains
        /// an array of <see cref="ResourceResponse"/> objects containing the IDs that 
        /// the receiving channel assigned to the activities.</remarks>
        public Task<ResourceResponse[]> SendActivities(IActivity[] activities)
        {
            if (activities == null)
                throw new ArgumentNullException(nameof(activities));

            if (activities.Length == 0)
                throw new ArgumentException("Expecting one or more activities, but the array was empty.", nameof(activities));

            var conversationReference = this.Activity.GetConversationReference();

            var bufferedActivities = new List<Activity>(activities.Length);

            for (var index = 0; index < activities.Length; index++)
            {
                // Buffer the incoming activities into a List<T> since we allow the set to be manipulated by the callbacks
                // Bind the relevant Conversation Reference properties, such as URLs and 
                // ChannelId's, to the activity we're about to send
                bufferedActivities.Add(activities[index].ApplyConversationReference(conversationReference));
            }

            // If there are no callbacks registered, bypass the overhead of invoking them and send directly to the adapter
            if (_onSendActivities.Count == 0)
            {
                return SendActivitiesThroughAdapter();
            }

            // Send through the full callback pipeline
            return SendActivitiesThroughCallbackPipeline();

            Task<ResourceResponse[]> SendActivitiesThroughCallbackPipeline(int nextCallbackIndex = 0)
            {
                // If we've executed the last callback, we now send straight to the adapter
                if (nextCallbackIndex == _onSendActivities.Count)
                {
                    return SendActivitiesThroughAdapter();
                }

                return _onSendActivities[nextCallbackIndex].Invoke(this, bufferedActivities, () => SendActivitiesThroughCallbackPipeline(nextCallbackIndex + 1));
            }

            async Task<ResourceResponse[]> SendActivitiesThroughAdapter()
            {
                // Send from the list which may have been manipulated via the event handlers. 
                // Note that 'responses' was captured from the root of the call, and will be
                // returned to the original caller.
                var responses = await Adapter.SendActivities(this, bufferedActivities.ToArray()).ConfigureAwait(false);
                var sentNonTraceActivity = false;

                for (var index = 0; index < responses.Length; index++)
                {
                    var activity = bufferedActivities[index];

                    activity.Id = responses[index].Id;

                    sentNonTraceActivity |= activity.Type != ActivityTypes.Trace;
                }

                if (sentNonTraceActivity)
                {
                    Responded = true;
                }

                return responses;
            }
        }

        /// <summary>
        /// Replaces an existing activity. 
        /// </summary>
        /// <param name="activity">New replacement activity.</param>        
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <exception cref="Microsoft.Bot.Schema.ErrorResponseException">
        /// The HTTP operation failed and the response contained additional information.</exception>
        /// <exception cref="System.AggregateException">
        /// One or more exceptions occurred during the operation.</exception>
        /// <remarks>If the activity is successfully sent, the task result contains
        /// a <see cref="ResourceResponse"/> object containing the ID that the receiving 
        /// channel assigned to the activity.
        /// <para>Before calling this, set the ID of the replacement activity to the ID
        /// of the activity to replace.</para></remarks>
        public async Task<ResourceResponse> UpdateActivity(IActivity activity)
        {
            Activity a = (Activity)activity;

            async Task<ResourceResponse> ActuallyUpdateStuff()
            {
                return await Adapter.UpdateActivity(this, a).ConfigureAwait(false);
            }

            return await UpdateActivityInternal(a, _onUpdateActivity, ActuallyUpdateStuff).ConfigureAwait(false);
        }

        /// <summary>
        /// Deletes an existing activity.
        /// </summary>
        /// <param name="activityId">The ID of the activity to delete.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <exception cref="Microsoft.Bot.Schema.ErrorResponseException">
        /// The HTTP operation failed and the response contained additional information.</exception>
        public async Task DeleteActivity(string activityId)
        {
            if (string.IsNullOrWhiteSpace(activityId))
                throw new ArgumentNullException(nameof(activityId));

            ConversationReference cr = this.Activity.GetConversationReference();
            cr.ActivityId = activityId;

            async Task ActuallyDeleteStuff()
            {
                await Adapter.DeleteActivity(this, cr).ConfigureAwait(false);
            }

            await DeleteActivityInternal(cr, _onDeleteActivity, ActuallyDeleteStuff);
        }

        /// <summary>
        /// Deletes an existing activity.
        /// </summary>
        /// <param name="conversationReference">The conversation containing the activity to delete.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <exception cref="Microsoft.Bot.Schema.ErrorResponseException">
        /// The HTTP operation failed and the response contained additional information.</exception>
        /// <remarks>The conversation reference's <see cref="ConversationReference.ActivityId"/> 
        /// indicates the activity in the conversation to delete.</remarks>
        public async Task DeleteActivity(ConversationReference conversationReference)
        {
            if (conversationReference == null)
                throw new ArgumentNullException(nameof(conversationReference));

            async Task ActuallyDeleteStuff()
            {
                await Adapter.DeleteActivity(this, conversationReference).ConfigureAwait(false);
            }

            await DeleteActivityInternal(conversationReference, _onDeleteActivity, ActuallyDeleteStuff).ConfigureAwait(false);
        }

        private async Task<ResourceResponse> UpdateActivityInternal(Activity activity,
            IEnumerable<UpdateActivityHandler> updateHandlers,
            Func<Task<ResourceResponse>> callAtBottom)
        {
            BotAssert.ActivityNotNull(activity);
            if (updateHandlers == null)
                throw new ArgumentException(nameof(updateHandlers));

            if (updateHandlers.Count() == 0) // No middleware to run.
            {
                if (callAtBottom != null)
                {
                    return await callAtBottom().ConfigureAwait(false);
                }

                return null;
            }

            // Default to "No more Middleware after this".
            async Task<ResourceResponse> next()
            {
                // Remove the first item from the list of middleware to call,
                // so that the next call just has the remaining items to worry about. 
                IEnumerable<UpdateActivityHandler> remaining = updateHandlers.Skip(1);
                var result = await UpdateActivityInternal(activity, remaining, callAtBottom).ConfigureAwait(false);
                activity.Id = result.Id;
                return result;
            }

            // Grab the current middleware, which is the 1st element in the array, and execute it            
            UpdateActivityHandler toCall = updateHandlers.First();
            return await toCall(this, activity, next).ConfigureAwait(false);
        }

        private async Task DeleteActivityInternal(ConversationReference cr,
           IEnumerable<DeleteActivityHandler> updateHandlers,
           Func<Task> callAtBottom)
        {
            BotAssert.ConversationReferenceNotNull(cr);

            if (updateHandlers == null)
                throw new ArgumentException(nameof(updateHandlers));

            if (updateHandlers.Count() == 0) // No middleware to run.
            {
                if (callAtBottom != null)
                {
                    await callAtBottom().ConfigureAwait(false);
                }

                return;
            }

            // Default to "No more Middleware after this".
            async Task next()
            {
                // Remove the first item from the list of middleware to call,
                // so that the next call just has the remaining items to worry about. 
                IEnumerable<DeleteActivityHandler> remaining = updateHandlers.Skip(1);
                await DeleteActivityInternal(cr, remaining, callAtBottom).ConfigureAwait(false);
            }

            // Grab the current middleware, which is the 1st element in the array, and execute it.
            DeleteActivityHandler toCall = updateHandlers.First();
            await toCall(this, cr, next).ConfigureAwait(false);
        }


        public void Dispose()
        {
            Services.Dispose();
        }
    }
}
