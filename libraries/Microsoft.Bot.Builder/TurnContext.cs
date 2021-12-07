﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;
using Newtonsoft.Json.Linq;

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
        private const string Turn = "turn";

        private readonly IList<SendActivitiesHandler> _onSendActivities = new List<SendActivitiesHandler>();
        private readonly IList<UpdateActivityHandler> _onUpdateActivity = new List<UpdateActivityHandler>();
        private readonly IList<DeleteActivityHandler> _onDeleteActivity = new List<DeleteActivityHandler>();

        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="TurnContext"/> class.
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
        /// Initializes a new instance of the <see cref="TurnContext"/> class from another turncontext class to target an alternate Activity.
        /// </summary>
        /// <remarks>
        /// For supporting calling legacy systems that always assume turncontext.Activity is the activity should be processed.
        /// This class clones the turncontext and then replaces the original.activity with the passed in activity.
        /// </remarks>
        /// <param name="turnContext">context to clone.</param>
        /// <param name="activity">activity to put into the new turn context.</param>
        public TurnContext(ITurnContext turnContext, Activity activity)
        {
            if (turnContext == null)
            {
                throw new ArgumentNullException(nameof(turnContext));
            }

            Activity = activity ?? throw new ArgumentNullException(nameof(activity));

            // all properties should be copied over except for activity.
            Adapter = turnContext.Adapter;
            TurnState = turnContext.TurnState;
            Responded = turnContext.Responded;

            if (turnContext is TurnContext tc)
            {
                BufferedReplyActivities = tc.BufferedReplyActivities;

                // keep private middelware pipeline hooks.
                _onSendActivities = tc._onSendActivities;
                _onUpdateActivity = tc._onUpdateActivity;
                _onDeleteActivity = tc._onDeleteActivity;
            }
        }

        /// <summary>
        /// Gets the bot adapter that created this context object.
        /// </summary>
        /// <value>The bot adapter that created this context object.</value>
        public BotAdapter Adapter { get; }

        /// <summary>
        /// Gets the services registered on this context object.
        /// </summary>
        /// <value>The services registered on this context object.</value>
        public TurnContextStateCollection TurnState { get; } = new TurnContextStateCollection();

        /// <summary>
        /// Gets or sets the locale on this context object.
        /// </summary>
        /// <value>The string of locale on this context object.</value>
        public string Locale
        {
            get 
            { 
                var valueObj = this.TurnState.Get<JObject>(Turn);
                if (valueObj.TryGetValue(nameof(Locale).ToLowerInvariant(), out var locale))
                {
                    return locale.ToString();
                }
                else
                {
                    return null;
                }
            }

            set
            {
                var valueObj = this.TurnState.Get<JObject>(Turn);
                if (valueObj != null)
                {
                    valueObj[nameof(Locale).ToLowerInvariant()] = value;
                }
                else
                {
                    valueObj = new JObject(new JProperty(nameof(Locale).ToLowerInvariant(), value));
                    TurnState.Set(Turn, valueObj);
                }
            }
        }

        /// <summary>
        /// Gets the activity associated with this turn; or <c>null</c> when processing
        /// a proactive message.
        /// </summary>
        /// <value>The activity associated with this turn.</value>
        public Activity Activity { get; }

        /// <summary>
        /// Gets a value indicating whether at least one response was sent for the current turn.
        /// </summary>
        /// <value><c>true</c> if at least one response was sent for the current turn.</value>
        /// <remarks><see cref="ITraceActivity"/> activities on their own do not set this flag.</remarks>
        public bool Responded
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets a list of activities to send when `context.Activity.DeliveryMode == 'expectReplies'.
        /// </summary>
        /// <value>A list of activities.</value>
        public List<Activity> BufferedReplyActivities { get; } = new List<Activity>();

        /// <summary>
        /// Adds a response handler for send activity operations.
        /// </summary>
        /// <param name="handler">The handler to add to the context object.</param>
        /// <returns>The updated context object.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="handler"/> is <c>null</c>.</exception>
        /// <remarks>When the context's <see cref="SendActivityAsync(IActivity, CancellationToken)"/>
        /// or <see cref="SendActivitiesAsync(IActivity[], CancellationToken)"/> methods are called,
        /// the adapter calls the registered handlers in the order in which they were
        /// added to the context object.
        /// </remarks>
        public ITurnContext OnSendActivities(SendActivitiesHandler handler)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(OnSendActivities));
            }

            if (handler == null)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            _onSendActivities.Add(handler);
            return this;
        }

        /// <summary>
        /// Adds a response handler for update activity operations.
        /// </summary>
        /// <param name="handler">The handler to add to the context object.</param>
        /// <returns>The updated context object.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="handler"/> is <c>null</c>.</exception>
        /// <remarks>When the context's <see cref="UpdateActivityAsync(IActivity, CancellationToken)"/> is called,
        /// the adapter calls the registered handlers in the order in which they were
        /// added to the context object.
        /// </remarks>
        public ITurnContext OnUpdateActivity(UpdateActivityHandler handler)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(OnUpdateActivity));
            }

            if (handler == null)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            _onUpdateActivity.Add(handler);
            return this;
        }

        /// <summary>
        /// Adds a response handler for delete activity operations.
        /// </summary>
        /// <param name="handler">The handler to add to the context object.</param>
        /// <returns>The updated context object.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="handler"/> is <c>null</c>.</exception>
        /// <remarks>When the context's <see cref="DeleteActivityAsync(ConversationReference, CancellationToken)"/>
        /// or <see cref="DeleteActivityAsync(string, CancellationToken)"/> is called,
        /// the adapter calls the registered handlers in the order in which they were
        /// added to the context object.
        /// </remarks>
        public ITurnContext OnDeleteActivity(DeleteActivityHandler handler)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(OnDeleteActivity));
            }

            if (handler == null)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            _onDeleteActivity.Add(handler);
            return this;
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
        /// <param name="cancellationToken">The cancellation token.</param>
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
        public async Task<ResourceResponse> SendActivityAsync(string textReplyToSend, string speak = null, string inputHint = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(SendActivityAsync));
            }

            if (string.IsNullOrWhiteSpace(textReplyToSend))
            {
                throw new ArgumentNullException(nameof(textReplyToSend));
            }

            var activityToSend = new Activity(ActivityTypes.Message) { Text = textReplyToSend };

            if (!string.IsNullOrEmpty(speak))
            {
                activityToSend.Speak = speak;
            }

            if (!string.IsNullOrEmpty(inputHint))
            {
                activityToSend.InputHint = inputHint;
            }

            return await SendActivityAsync(activityToSend, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Sends an activity to the sender of the incoming activity.
        /// </summary>
        /// <param name="activity">The activity to send.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="activity"/> is <c>null</c>.</exception>
        /// <remarks>If the activity is successfully sent, the task result contains
        /// a <see cref="ResourceResponse"/> object containing the ID that the receiving
        /// channel assigned to the activity.</remarks>
        public async Task<ResourceResponse> SendActivityAsync(IActivity activity, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(SendActivityAsync));
            }

            BotAssert.ActivityNotNull(activity);

            ResourceResponse[] responses = await SendActivitiesAsync(new[] { activity }, cancellationToken).ConfigureAwait(false);
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
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <remarks>If the activities are successfully sent, the task result contains
        /// an array of <see cref="ResourceResponse"/> objects containing the IDs that
        /// the receiving channel assigned to the activities.</remarks>
        public Task<ResourceResponse[]> SendActivitiesAsync(IActivity[] activities, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(SendActivitiesAsync));
            }

            if (activities == null)
            {
                throw new ArgumentNullException(nameof(activities));
            }

            if (activities.Length == 0)
            {
                throw new ArgumentException("Expecting one or more activities, but the array was empty.", nameof(activities));
            }

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
                if (Activity.DeliveryMode == DeliveryModes.ExpectReplies)
                {
                    var responses = new ResourceResponse[bufferedActivities.Count];
                    var sentNonTraceActivity = false;

                    for (var index = 0; index < responses.Length; index++)
                    {
                        var activity = bufferedActivities[index];
                        BufferedReplyActivities.Add(activity);

                        // Ensure the TurnState has the InvokeResponseKey, since this activity
                        // is not being sent through the adapter, where it would be added to TurnState.
                        if (activity.Type == ActivityTypesEx.InvokeResponse)
                        {
                            TurnState.Add(BotAdapter.InvokeResponseKey, activity);
                        }

                        responses[index] = new ResourceResponse();

                        sentNonTraceActivity |= activity.Type != ActivityTypes.Trace;
                    }

                    if (sentNonTraceActivity)
                    {
                        Responded = true;
                    }

                    return responses;
                }
                else
                {
                    // Send from the list which may have been manipulated via the event handlers.
                    // Note that 'responses' was captured from the root of the call, and will be
                    // returned to the original caller.
                    var responses = await Adapter.SendActivitiesAsync(this, bufferedActivities.ToArray(), cancellationToken).ConfigureAwait(false);
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
        }

        /// <summary>
        /// Replaces an existing activity.
        /// </summary>
        /// <param name="activity">New replacement activity.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
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
        public async Task<ResourceResponse> UpdateActivityAsync(IActivity activity, CancellationToken cancellationToken = default)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(UpdateActivityAsync));
            }

            BotAssert.ActivityNotNull(activity);

            var conversationReference = Activity.GetConversationReference();
            var a = activity.ApplyConversationReference(conversationReference);

            async Task<ResourceResponse> ActuallyUpdateStuff()
            {
                return await Adapter.UpdateActivityAsync(this, a, cancellationToken).ConfigureAwait(false);
            }

            return await UpdateActivityInternalAsync(a, _onUpdateActivity, ActuallyUpdateStuff, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Deletes an existing activity.
        /// </summary>
        /// <param name="activityId">The ID of the activity to delete.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <exception cref="Microsoft.Bot.Schema.ErrorResponseException">
        /// The HTTP operation failed and the response contained additional information.</exception>
        public async Task DeleteActivityAsync(string activityId, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(DeleteActivityAsync));
            }

            if (string.IsNullOrWhiteSpace(activityId))
            {
                throw new ArgumentNullException(nameof(activityId));
            }

            var cr = Activity.GetConversationReference();
            cr.ActivityId = activityId;

            async Task ActuallyDeleteStuff()
            {
                await Adapter.DeleteActivityAsync(this, cr, cancellationToken).ConfigureAwait(false);
            }

            await DeleteActivityInternalAsync(cr, _onDeleteActivity, ActuallyDeleteStuff, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Deletes an existing activity.
        /// </summary>
        /// <param name="conversationReference">The conversation containing the activity to delete.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <exception cref="Microsoft.Bot.Schema.ErrorResponseException">
        /// The HTTP operation failed and the response contained additional information.</exception>
        /// <remarks>The conversation reference's <see cref="ConversationReference.ActivityId"/>
        /// indicates the activity in the conversation to delete.</remarks>
        public async Task DeleteActivityAsync(ConversationReference conversationReference, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(DeleteActivityAsync));
            }

            if (conversationReference == null)
            {
                throw new ArgumentNullException(nameof(conversationReference));
            }

            async Task ActuallyDeleteStuff()
            {
                await Adapter.DeleteActivityAsync(this, conversationReference, cancellationToken).ConfigureAwait(false);
            }

            await DeleteActivityInternalAsync(conversationReference, _onDeleteActivity, ActuallyDeleteStuff, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Frees resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <param name="disposing">Boolean value that determines whether to free resources or not.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                TurnState.Dispose();
            }

            _disposed = true;
        }

        private async Task<ResourceResponse> UpdateActivityInternalAsync(
            Activity activity,
            IEnumerable<UpdateActivityHandler> updateHandlers,
            Func<Task<ResourceResponse>> callAtBottom,
            CancellationToken cancellationToken)
        {
            BotAssert.ActivityNotNull(activity);
            if (updateHandlers == null)
            {
                throw new ArgumentException($"{nameof(updateHandlers)} is null.", nameof(updateHandlers));
            }

            // No middleware to run.
            if (!updateHandlers.Any())
            {
                if (callAtBottom != null)
                {
                    return await callAtBottom().ConfigureAwait(false);
                }

                return null;
            }

            // Default to "No more Middleware after this".
            async Task<ResourceResponse> Next()
            {
                // Remove the first item from the list of middleware to call,
                // so that the next call just has the remaining items to worry about.
                IEnumerable<UpdateActivityHandler> remaining = updateHandlers.Skip(1);
                var result = await UpdateActivityInternalAsync(activity, remaining, callAtBottom, cancellationToken).ConfigureAwait(false);
                activity.Id = result.Id;
                return result;
            }

            // Grab the current middleware, which is the 1st element in the array, and execute it
            UpdateActivityHandler toCall = updateHandlers.First();
            return await toCall(this, activity, Next).ConfigureAwait(false);
        }

        private async Task DeleteActivityInternalAsync(
            ConversationReference cr,
            IEnumerable<DeleteActivityHandler> deleteHandlers,
            Func<Task> callAtBottom,
            CancellationToken cancellationToken)
        {
            BotAssert.ConversationReferenceNotNull(cr);

            if (deleteHandlers == null)
            {
                throw new ArgumentException($"{nameof(deleteHandlers)} is null", nameof(deleteHandlers));
            }

            // No middleware to run.
            if (!deleteHandlers.Any())
            {
                if (callAtBottom != null)
                {
                    await callAtBottom().ConfigureAwait(false);
                }

                return;
            }

            // Default to "No more Middleware after this".
            async Task Next()
            {
                // Remove the first item from the list of middleware to call,
                // so that the next call just has the remaining items to worry about.
                IEnumerable<DeleteActivityHandler> remaining = deleteHandlers.Skip(1);
                await DeleteActivityInternalAsync(cr, remaining, callAtBottom, cancellationToken).ConfigureAwait(false);
            }

            // Grab the current middleware, which is the 1st element in the array, and execute it.
            DeleteActivityHandler toCall = deleteHandlers.First();
            await toCall(this, cr, Next).ConfigureAwait(false);
        }
    }
}
