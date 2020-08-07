// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder
{
    /// <summary>
    /// A TurnContext with a strongly typed Activity property that wraps an untyped inner TurnContext.
    /// </summary>
    /// <typeparam name="T">An IActivity derived type, that is one of IMessageActivity, IConversationUpdateActivity etc.</typeparam>
    public class DelegatingTurnContext<T> : ITurnContext<T>
        where T : IActivity
    {
        private ITurnContext _innerTurnContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="DelegatingTurnContext{T}"/> class.
        /// </summary>
        /// <param name="innerTurnContext">The inner turn context.</param>
        public DelegatingTurnContext(ITurnContext innerTurnContext)
        {
            _innerTurnContext = innerTurnContext;
        }

        /// <summary>
        /// Gets the inner  context's activity, cast to the type parameter of this <see cref="DelegatingTurnContext{T}"/>.
        /// </summary>
        /// <value>The inner context's activity.</value>
        T ITurnContext<T>.Activity => (T)(IActivity)_innerTurnContext.Activity;

        /// <summary>
        /// Gets the bot adapter that created this context object.
        /// </summary>
        /// <value>The bot adapter that created this context object.</value>
        public BotAdapter Adapter => _innerTurnContext.Adapter;

        /// <summary>
        /// Gets the collection of values cached with the context object for the lifetime of the turn.
        /// </summary>
        /// <value>The collection of services registered on this context object.</value>
        public TurnContextStateCollection TurnState => _innerTurnContext.TurnState;

        /// <summary>
        /// Gets the activity for this turn of the bot.
        /// </summary>
        /// <value>The activity for this turn of the bot.</value>
        public Activity Activity => _innerTurnContext.Activity;

        /// <summary>
        /// Gets a value indicating whether at least one response was sent for the current turn.
        /// </summary>
        /// <value><c>true</c> if at least one response was sent for the current turn; otherwise, <c>false</c>.</value>
        /// <seealso cref="SendActivityAsync(IActivity, CancellationToken)"/>
        public bool Responded => _innerTurnContext.Responded;

        /// <summary>
        /// Deletes an existing activity.
        /// </summary>
        /// <param name="activityId">The ID of the activity to delete.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <para>Not all channels support this operation. For channels that don\'t, this call may throw an exception.</para>
        /// <seealso cref="OnDeleteActivity(DeleteActivityHandler)"/>
        /// <seealso cref="DeleteActivityAsync(ConversationReference, CancellationToken)"/>
        /// <seealso cref="SendActivitiesAsync(IActivity[], CancellationToken)"/>
        /// <seealso cref="UpdateActivityAsync(IActivity, CancellationToken)"/>
        public Task DeleteActivityAsync(string activityId, CancellationToken cancellationToken = default(CancellationToken))
            => _innerTurnContext.DeleteActivityAsync(activityId, cancellationToken);

        /// <summary>
        /// Deletes an existing activity.
        /// </summary>
        /// <param name="conversationReference">The conversation containing the activity to delete.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <remarks>The conversation reference's <see cref="ConversationReference.ActivityId"/>
        /// indicates the activity in the conversation to delete.
        /// <para>Not all channels support this operation. For channels that don't, this call may throw an exception.</para></remarks>
        /// <seealso cref="OnDeleteActivity(DeleteActivityHandler)"/>
        /// <seealso cref="DeleteActivityAsync(string, CancellationToken)"/>
        /// <seealso cref="SendActivitiesAsync(IActivity[], CancellationToken)"/>
        /// <seealso cref="UpdateActivityAsync(IActivity, CancellationToken)"/>
        public Task DeleteActivityAsync(ConversationReference conversationReference, CancellationToken cancellationToken = default(CancellationToken))
            => _innerTurnContext.DeleteActivityAsync(conversationReference, cancellationToken);

        /// <summary>
        /// Adds a response handler for delete activity operations.
        /// </summary>
        /// <param name="handler">The handler to add to the context object.</param>
        /// <returns>The updated context object.</returns>
        /// <remarks>When the context's <see cref="DeleteActivityAsync(string, CancellationToken)"/> is called,
        /// the adapter calls the registered handlers in the order in which they were
        /// added to the context object.
        /// </remarks>
        /// <seealso cref="DeleteActivityAsync(ConversationReference, CancellationToken)"/>
        /// <seealso cref="DeleteActivityAsync(string, CancellationToken)"/>
        /// <seealso cref="DeleteActivityHandler"/>
        /// <seealso cref="OnSendActivities(SendActivitiesHandler)"/>
        /// <seealso cref="OnUpdateActivity(UpdateActivityHandler)"/>
        public ITurnContext OnDeleteActivity(DeleteActivityHandler handler)
            => _innerTurnContext.OnDeleteActivity(handler);

        /// <summary>
        /// Adds a response handler for send activity operations.
        /// </summary>
        /// <param name="handler">The handler to add to the context object.</param>
        /// <returns>The updated context object.</returns>
        /// <remarks>When the context's <see cref="SendActivityAsync(IActivity, CancellationToken)"/>
        /// or <see cref="SendActivitiesAsync(IActivity[], CancellationToken)"/> method is called,
        /// the adapter calls the registered handlers in the order in which they were
        /// added to the context object.
        /// </remarks>
        /// <seealso cref="SendActivityAsync(string, string, string, CancellationToken)"/>
        /// <seealso cref="SendActivityAsync(IActivity, CancellationToken)"/>
        /// <seealso cref="SendActivitiesAsync(IActivity[], CancellationToken)"/>
        /// <seealso cref="SendActivitiesHandler"/>
        /// <seealso cref="OnUpdateActivity(UpdateActivityHandler)"/>
        /// <seealso cref="OnDeleteActivity(DeleteActivityHandler)"/>
        public ITurnContext OnSendActivities(SendActivitiesHandler handler)
            => _innerTurnContext.OnSendActivities(handler);

        /// <summary>
        /// Adds a response handler for update activity operations.
        /// </summary>
        /// <param name="handler">The handler to add to the context object.</param>
        /// <returns>The updated context object.</returns>
        /// <remarks>When the context's <see cref="UpdateActivityAsync(IActivity, CancellationToken)"/> is called,
        /// the adapter calls the registered handlers in the order in which they were
        /// added to the context object.
        /// </remarks>
        /// <seealso cref="UpdateActivityAsync(IActivity, CancellationToken)"/>
        /// <seealso cref="UpdateActivityHandler"/>
        /// <seealso cref="OnSendActivities(SendActivitiesHandler)"/>
        /// <seealso cref="OnDeleteActivity(DeleteActivityHandler)"/>
        public ITurnContext OnUpdateActivity(UpdateActivityHandler handler)
            => _innerTurnContext.OnUpdateActivity(handler);

        /// <summary>
        /// Sends a set of activities to the sender of the incoming activity.
        /// </summary>
        /// <param name="activities">The activities to send.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <remarks>If the activities are successfully sent, the task result contains
        /// an array of <see cref="ResourceResponse"/> objects containing the IDs that
        /// the receiving channel assigned to the activities.</remarks>
        /// <seealso cref="OnSendActivities(SendActivitiesHandler)"/>
        /// <seealso cref="SendActivityAsync(string, string, string, CancellationToken)"/>
        /// <seealso cref="SendActivityAsync(IActivity, CancellationToken)"/>
        /// <seealso cref="UpdateActivityAsync(IActivity, CancellationToken)"/>
        /// <seealso cref="DeleteActivityAsync(ConversationReference, CancellationToken)"/>
        public Task<ResourceResponse[]> SendActivitiesAsync(IActivity[] activities, CancellationToken cancellationToken = default(CancellationToken))
            => _innerTurnContext.SendActivitiesAsync(activities, cancellationToken);

        /// <summary>
        /// Sends a message activity to the sender of the incoming activity.
        /// </summary>
        /// <param name="textReplyToSend">The text of the message to send.</param>
        /// <param name="speak">Optional, text to be spoken by your bot on a speech-enabled
        /// channel.</param>
        /// <param name="inputHint">Optional, indicates whether your bot is accepting,
        /// expecting, or ignoring user input after the message is delivered to the client.
        /// <see cref="InputHints"/> defines the possible values.
        /// Default is <see cref="InputHints.AcceptingInput"/>.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <remarks>If the activity is successfully sent, the task result contains
        /// a <see cref="ResourceResponse"/> object that contains the ID that the receiving
        /// channel assigned to the activity.
        /// <para>See the channel's documentation for limits imposed upon the contents of
        /// <paramref name="textReplyToSend"/>.</para>
        /// <para>To control various characteristics of your bot's speech such as voice,
        /// rate, volume, pronunciation, and pitch, specify <paramref name="speak"/> in
        /// Speech Synthesis Markup Language (SSML) format.</para>
        /// </remarks>
        /// <seealso cref="OnSendActivities(SendActivitiesHandler)"/>
        /// <seealso cref="SendActivityAsync(IActivity, CancellationToken)"/>
        /// <seealso cref="SendActivitiesAsync(IActivity[], CancellationToken)"/>
        /// <seealso cref="UpdateActivityAsync(IActivity, CancellationToken)"/>
        /// <seealso cref="DeleteActivityAsync(ConversationReference, CancellationToken)"/>
        public Task<ResourceResponse> SendActivityAsync(string textReplyToSend, string speak = null, string inputHint = InputHints.AcceptingInput, CancellationToken cancellationToken = default(CancellationToken))
            => _innerTurnContext.SendActivityAsync(textReplyToSend, speak, inputHint, cancellationToken);

        /// <summary>
        /// Sends an activity to the sender of the incoming activity.
        /// </summary>
        /// <param name="activity">The activity to send.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <remarks>If the activity is successfully sent, the task result contains
        /// a <see cref="ResourceResponse"/> object containing the ID that the receiving
        /// channel assigned to the activity.</remarks>
        /// <seealso cref="OnSendActivities(SendActivitiesHandler)"/>
        /// <seealso cref="SendActivityAsync(string, string, string, CancellationToken)"/>
        /// <seealso cref="SendActivitiesAsync(IActivity[], CancellationToken)"/>
        /// <seealso cref="UpdateActivityAsync(IActivity, CancellationToken)"/>
        /// <seealso cref="DeleteActivityAsync(ConversationReference, CancellationToken)"/>
        public Task<ResourceResponse> SendActivityAsync(IActivity activity, CancellationToken cancellationToken = default(CancellationToken))
            => _innerTurnContext.SendActivityAsync(activity, cancellationToken);

        /// <summary>
        /// Replaces an existing activity.
        /// </summary>
        /// <param name="activity">New replacement activity.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <remarks>If the activity is successfully sent, the task result contains
        /// a <see cref="ResourceResponse"/> object containing the ID that the receiving
        /// channel assigned to the activity.
        /// <para>Before calling this, set the ID of the replacement activity to the ID
        /// of the activity to replace.</para>
        /// <para>Not all channels support this operation. For channels that don't, this call may throw an exception.</para></remarks>
        /// <seealso cref="OnUpdateActivity(UpdateActivityHandler)"/>
        /// <seealso cref="SendActivitiesAsync(IActivity[], CancellationToken)"/>
        /// <seealso cref="DeleteActivityAsync(ConversationReference, CancellationToken)"/>
        public Task<ResourceResponse> UpdateActivityAsync(IActivity activity, CancellationToken cancellationToken = default(CancellationToken))
            => _innerTurnContext.UpdateActivityAsync(activity, cancellationToken);
    }
}
