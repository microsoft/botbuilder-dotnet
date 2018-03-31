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
    public class TurnContext : ITurnContext
    {
        private readonly BotAdapter _adapter;
        private readonly Activity _activity;
        private bool _responded = false;

        private readonly IList<SendActivitiesHandler> _onSendActivities = new List<SendActivitiesHandler>();
        private readonly IList<UpdateActivityHandler> _onUpdateActivity = new List<UpdateActivityHandler>();
        private readonly IList<DeleteActivityHandler> _onDeleteActivity = new List<DeleteActivityHandler>();

        private readonly TurnContextServiceCollection _services = new TurnContextServiceCollection();

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
            _adapter = adapter ?? throw new ArgumentNullException(nameof(adapter));
            _activity = activity ?? throw new ArgumentNullException(nameof(activity));
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
        public BotAdapter Adapter => _adapter;

        /// <summary>
        /// Gets the services registered on this context object.
        /// </summary>
        public ITurnContextServiceCollection Services => _services;

        /// <summary>
        /// Gets the activity associated with this turn; or <c>null</c> when processing
        /// a proactive message.
        /// </summary>
        public Activity Activity => _activity;

        /// <summary>
        /// Indicates whether at least one response was sent for the current turn.
        /// </summary>
        /// <value><c>true</c> if at least one response was sent for the current turn.</value>
        /// <exception cref="ArgumentException">You attempted to set the value to <c>false</c>.</exception>
        public bool Responded
        {
            get { return _responded; }
            set
            {
                if (value == false)
                {
                    throw new ArgumentException("TurnContext: cannot set 'responded' to a value of 'false'.");
                }
                _responded = true;
            }
        }

        /// <summary>
        /// Sends a message activity to the sender of the incoming activity.
        /// </summary>
        /// <param name="textReplyToSend">The text of the message to send.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="textReplyToSend"/> is <c>null</c> or whitespace.</exception>
        /// <remarks>If the activity is successfully sent, the task result contains
        /// a <see cref="ResourceResponse"/> object containing the ID that the receiving 
        /// channel assigned to the activity.</remarks>
        public async Task<ResourceResponse> SendActivity(string textReplyToSend, string speak = null, string inputHint = null)
        {
            if (string.IsNullOrWhiteSpace(textReplyToSend))
                throw new ArgumentNullException(nameof(textReplyToSend));

            var activityToSend = new Activity(ActivityTypes.Message) { Text = textReplyToSend };

            if (!string.IsNullOrEmpty(speak))
                activityToSend.Speak = speak;

            if (!string.IsNullOrEmpty(inputHint))
                activityToSend.InputHint = inputHint;

            return await SendActivity(activityToSend);
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
            if (activity == null)
                throw new ArgumentNullException(nameof(activity));

            ResourceResponse[] responses = await SendActivities(new IActivity[] { activity });
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
        /// Sends an activity to the sender of the incoming activity.
        /// </summary>
        /// <param name="activities">The activities to send.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <remarks>If the activities are successfully sent, the task result contains
        /// an array of <see cref="ResourceResponse"/> objects containing the IDs that 
        /// the receiving channel assigned to the activities.</remarks>
        public async Task<ResourceResponse[]> SendActivities(IActivity[] activities)
        {
            // Bind the relevant Conversation Reference properties, such as URLs and 
            // ChannelId's, to the activities we're about to send. 
            ConversationReference cr = GetConversationReference(this._activity);
            foreach (Activity a in activities)
            {
                ApplyConversationReference(a, cr);
            }

            // Convert the IActivities to Activies. 
            Activity[] activityArray = Array.ConvertAll(activities, (input) => (Activity)input);

            // Create the list used by the recursive methods. 
            List<Activity> activityList = new List<Activity>(activityArray);

            // provide a variable to capture the set of responses returned from the adapter.
            ResourceResponse[] responses = null;

            async Task ActuallySendStuff()
            {
                bool anythingToSend = false;
                if (activities.Count() > 0)
                    anythingToSend = true;

                // Send from the list, which may have been manipulated via the event handlers. 
                // Note that 'responses' was captured from the root of the call, and will be
                // returned to the original caller.
                responses = await this.Adapter.SendActivities(this, activityList.ToArray());

                // If we actually sent something, set the flag. 
                if (anythingToSend)
                    this.Responded = true;
            }

            await SendActivitiesInternal(activityList, _onSendActivities, ActuallySendStuff);

            return responses;
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
            ResourceResponse response = null;

            async Task ActuallyUpdateStuff()
            {
                response = await this.Adapter.UpdateActivity(this, a);
            }

            await UpdateActivityInternal(a, _onUpdateActivity, ActuallyUpdateStuff);

            return response;
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

            ConversationReference cr = GetConversationReference(this.Activity);
            cr.ActivityId = activityId;

            async Task ActuallyDeleteStuff()
            {
                await this.Adapter.DeleteActivity(this, cr);
            }

            await DeleteActivityInternal(cr, _onDeleteActivity, ActuallyDeleteStuff);
        }

        public async Task DeleteActivity(ConversationReference conversationReference)
        {
            if (conversationReference == null)
                throw new ArgumentNullException(nameof(conversationReference));
            
            async Task ActuallyDeleteStuff()
            {
                await this.Adapter.DeleteActivity(this, conversationReference);
            }

            await DeleteActivityInternal(conversationReference, _onDeleteActivity, ActuallyDeleteStuff);
        }

        private async Task SendActivitiesInternal(
            List<Activity> activities,
            IEnumerable<SendActivitiesHandler> sendHandlers,
            Func<Task> callAtBottom)
        {
            if (activities == null)
                throw new ArgumentException(nameof(activities));
            if (sendHandlers == null)
                throw new ArgumentException(nameof(sendHandlers));

            if (sendHandlers.Count() == 0) // No middleware to run.
            {
                if (callAtBottom != null)
                    await callAtBottom();

                return;
            }

            // Default to "No more Middleware after this".
            async Task next()
            {
                // Remove the first item from the list of middleware to call,
                // so that the next call just has the remaining items to worry about. 
                IEnumerable<SendActivitiesHandler> remaining = sendHandlers.Skip(1);
                await SendActivitiesInternal(activities, remaining, callAtBottom).ConfigureAwait(false);
            }

            // Grab the current middleware, which is the 1st element in the array, and execute it            
            SendActivitiesHandler caller = sendHandlers.First();
            await caller(this, activities, next);
        }

        private async Task UpdateActivityInternal(Activity activity,
            IEnumerable<UpdateActivityHandler> updateHandlers,
            Func<Task> callAtBottom)
        {
            BotAssert.ActivityNotNull(activity);
            if (updateHandlers == null)
                throw new ArgumentException(nameof(updateHandlers));

            if (updateHandlers.Count() == 0) // No middleware to run.
            {
                if (callAtBottom != null)
                {
                    await callAtBottom();
                }

                return;
            }

            // Default to "No more Middleware after this".
            async Task next()
            {
                // Remove the first item from the list of middleware to call,
                // so that the next call just has the remaining items to worry about. 
                IEnumerable<UpdateActivityHandler> remaining = updateHandlers.Skip(1);
                await UpdateActivityInternal(activity, remaining, callAtBottom).ConfigureAwait(false);
            }

            // Grab the current middleware, which is the 1st element in the array, and execute it            
            UpdateActivityHandler toCall = updateHandlers.First();
            await toCall(this, activity, next);
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
                    await callAtBottom();
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
            await toCall(this, cr, next);
        }

        /// <summary>
        /// Creates a conversation reference from an activity.
        /// </summary>
        /// <param name="activity">The activity.</param>
        /// <returns>A conversation reference for the conversation that contains the activity.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="activity"/> is <c>null</c>.</exception>
        public static ConversationReference GetConversationReference(Activity activity)
        {
            BotAssert.ActivityNotNull(activity);

            ConversationReference r = new ConversationReference
            {
                ActivityId = activity.Id,
                User = activity.From,
                Bot = activity.Recipient,
                Conversation = activity.Conversation,
                ChannelId = activity.ChannelId,
                ServiceUrl = activity.ServiceUrl
            };

            return r;
        }

        /// <summary>
        /// Updates an activity with the delivery information from an existing 
        /// conversation reference.
        /// </summary>
        /// <param name="activity">The activity to update.</param>
        /// <param name="reference">The conversation reference.</param>
        /// <param name="isIncoming">(Optional) <c>true</c> to treat the activity as an 
        /// incoming activity, where the bot is the recipient; otherwaire <c>false</c>.
        /// Default is <c>false</c>, and the activity will show the bot as the sender.</param>
        /// <remarks>Call <see cref="GetConversationReference(Activity)"/> on an incoming
        /// activity to get a conversation reference that you can then use to update an
        /// outgoing activity with the correct delivery information.
        /// <para>The <see cref="SendActivity(IActivity)"/> and <see cref="SendActivities(IActivity[])"/>
        /// methods do this for you.</para>
        /// </remarks>
        public static Activity ApplyConversationReference(Activity activity, ConversationReference reference, bool isIncoming = false)
        {
            activity.ChannelId = reference.ChannelId;
            activity.ServiceUrl = reference.ServiceUrl;
            activity.Conversation = reference.Conversation;

            if (isIncoming)
            {
                activity.From = reference.User;
                activity.Recipient = reference.Bot;
                if (reference.ActivityId != null)
                    activity.Id = reference.ActivityId;
            }
            else  // Outgoing
            {
                activity.From = reference.Bot;
                activity.Recipient = reference.User;
                if (reference.ActivityId != null)
                    activity.ReplyToId = reference.ActivityId;
            }
            return activity;
        }
    }
}
