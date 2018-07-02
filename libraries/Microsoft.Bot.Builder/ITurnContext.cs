// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder
{
    /// <summary>
    /// A method that can participate in send activity events for the current turn.
    /// </summary>
    /// <param name="context">The context object for the turn.</param>
    /// <param name="activities">The activities to send.</param>
    /// <param name="next">The delegate to call to continue event processing.</param>
    /// <returns>A task that represents the work queued to execute.</returns>
    /// <remarks>A handler calls the <paramref name="next"/> delegate to pass control to
    /// the next registered handler. If a handler doesn’t call the next delegate,
    /// the adapter does not call any of the subsequent handlers and does not send the
    /// <paramref name="activities"/>.
    /// <para>If the activities are successfully sent, the <paramref name="next"/> delegate returns
    /// an array of <see cref="ResourceResponse"/> objects containing the IDs that
    /// the receiving channel assigned to the activities. Use this array as the return value of this handler.</para>
    /// </remarks>
    /// <seealso cref="BotAdapter"/>
    /// <seealso cref="UpdateActivityHandler"/>
    /// <seealso cref="DeleteActivityHandler"/>
    /// <seealso cref="ITurnContext.OnSendActivities(SendActivitiesHandler)"/>
    public delegate Task<ResourceResponse[]> SendActivitiesHandler(ITurnContext context, List<Activity> activities, Func<Task<ResourceResponse[]>> next);

    /// <summary>
    /// A method that can participate in update activity events for the current turn.
    /// </summary>
    /// <param name="context">The context object for the turn.</param>
    /// <param name="activity">The replacement activity.</param>
    /// <param name="next">The delegate to call to continue event processing.</param>
    /// <returns>A task that represents the work queued to execute.</returns>
    /// <remarks>A handler calls the <paramref name="next"/> delegate to pass control to
    /// the next registered handler. If a handler doesn’t call the next delegate,
    /// the adapter does not call any of the subsequent handlers and does not update the
    /// activity.
    /// <para>The activity's <see cref="IActivity.Id"/> indicates the activity in the
    /// conversation to replace.</para>
    /// <para>If the activity is successfully sent, the <paramref name="next"/> delegater returns
    /// a <see cref="ResourceResponse"/> object containing the ID that the receiving
    /// channel assigned to the activity. Use this response object as the return value of this handler.</para>
    /// </remarks>
    /// <seealso cref="BotAdapter"/>
    /// <seealso cref="SendActivitiesHandler"/>
    /// <seealso cref="DeleteActivityHandler"/>
    /// <seealso cref="ITurnContext.OnUpdateActivity(UpdateActivityHandler)"/>
    public delegate Task<ResourceResponse> UpdateActivityHandler(ITurnContext context, Activity activity, Func<Task<ResourceResponse>> next);

    /// <summary>
    /// A method that can participate in delete activity events for the current turn.
    /// </summary>
    /// <param name="context">The context object for the turn.</param>
    /// <param name="reference">The conversation containing the activity.</param>
    /// <param name="next">The delegate to call to continue event processing.</param>
    /// <returns>A task that represents the work queued to execute.</returns>
    /// <remarks>A handler calls the <paramref name="next"/> delegate to pass control to
    /// the next registered handler. If a handler doesn’t call the next delegate,
    /// the adapter does not call any of the subsequent handlers and does not delete the
    ///activity.
    /// <para>The conversation reference's <see cref="ConversationReference.ActivityId"/>
    /// indicates the activity in the conversation to replace.</para>
    /// </remarks>
    /// <seealso cref="BotAdapter"/>
    /// <seealso cref="SendActivitiesHandler"/>
    /// <seealso cref="UpdateActivityHandler"/>
    /// <seealso cref="ITurnContext.OnDeleteActivity(DeleteActivityHandler)"/>
    public delegate Task DeleteActivityHandler(ITurnContext context, ConversationReference reference, Func<Task> next);

    /// <summary>
    /// Provides context for a turn of a bot.
    /// </summary>
    /// <remarks>Context provides information needed to process an incoming activity.
    /// The context object is created by a <see cref="BotAdapter"/> and persists for the
    /// length of the turn.</remarks>
    /// <seealso cref="IBot"/>
    /// <seealso cref="IMiddleware"/>
    public interface ITurnContext
    {
        /// <summary>
        /// Gets the bot adapter that created this context object.
        /// </summary>
        BotAdapter Adapter { get; }

        /// <summary>
        /// Gets the services registered on this context object.
        /// </summary>
        TurnContextServiceCollection Services { get; }

        /// <summary>
        /// Incoming request
        /// </summary>
        Activity Activity { get; }

        /// <summary>
        /// Indicates whether at least one response was sent for the current turn.
        /// </summary>
        /// <value><c>true</c> if at least one response was sent for the current turn.</value>
        bool Responded { get; }

        /// <summary>
        /// Sends a message activity to the sender of the incoming activity.
        /// </summary>
        /// <param name="textReplyToSend">The text of the message to send.</param>
        /// <param name="speak">Optional, text to be spoken by your bot on a speech-enabled
        /// channel.</param>
        /// <param name="inputHint">Optional, indicates whether your bot is accepting,
        /// expecting, or ignoring user input after the message is delivered to the client.
        /// One of: "acceptingInput", "ignoringInput", or "expectingInput".
        /// Default is "acceptingInput".</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <remarks>If the activity is successfully sent, the task result contains
        /// a <see cref="ResourceResponse"/> object containing the ID that the receiving
        /// channel assigned to the activity.
        /// <para>See the channel's documentation for limits imposed upon the contents of
        /// <paramref name="textReplyToSend"/>.</para>
        /// <para>To control various characteristics of your bot's speech such as voice,
        /// rate, volume, pronunciation, and pitch, specify <paramref name="speak"/> in 
        /// Speech Synthesis Markup Language (SSML) format.</para>
        /// </remarks>
        /// <seealso cref="OnSendActivities(SendActivitiesHandler)"/>
        /// <seealso cref="SendActivity(IActivity)"/>
        /// <seealso cref="SendActivities(IActivity[])"/>
        /// <seealso cref="UpdateActivity(IActivity)"/>
        /// <seealso cref="DeleteActivity(ConversationReference)"/>
        Task<ResourceResponse> SendActivity(string textReplyToSend, string speak = null, string inputHint = InputHints.AcceptingInput, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Sends an activity to the sender of the incoming activity.
        /// </summary>
        /// <param name="activity">The activity to send.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <remarks>If the activity is successfully sent, the task result contains
        /// a <see cref="ResourceResponse"/> object containing the ID that the receiving 
        /// channel assigned to the activity.</remarks>
        /// <seealso cref="OnSendActivities(SendActivitiesHandler)"/>
        /// <seealso cref="SendActivity(string, string, string)"/>
        /// <seealso cref="SendActivities(IActivity[])"/>
        /// <seealso cref="UpdateActivity(IActivity)"/>
        /// <seealso cref="DeleteActivity(ConversationReference)"/>
        Task<ResourceResponse> SendActivity(IActivity activity, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Sends a set of activities to the sender of the incoming activity.
        /// </summary>
        /// <param name="activities">The activities to send.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <remarks>If the activities are successfully sent, the task result contains
        /// an array of <see cref="ResourceResponse"/> objects containing the IDs that 
        /// the receiving channel assigned to the activities.</remarks>
        /// <seealso cref="OnSendActivities(SendActivitiesHandler)"/>
        /// <seealso cref="SendActivity(string, string, string)"/>
        /// <seealso cref="SendActivity(IActivity)"/>
        /// <seealso cref="UpdateActivity(IActivity)"/>
        /// <seealso cref="DeleteActivity(ConversationReference)"/>
        Task<ResourceResponse[]> SendActivities(IActivity[] activities, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Replaces an existing activity. 
        /// </summary>
        /// <param name="activity">New replacement activity.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <remarks>If the activity is successfully sent, the task result contains
        /// a <see cref="ResourceResponse"/> object containing the ID that the receiving
        /// channel assigned to the activity.
        /// <para>Before calling this, set the ID of the replacement activity to the ID
        /// of the activity to replace.</para>
        /// <para>Not all channels support this operation. Channels that don't, may throw an exception.</para></remarks>
        /// <seealso cref="OnUpdateActivity(UpdateActivityHandler)"/>
        /// <seealso cref="SendActivities(IActivity[])"/>
        /// <seealso cref="DeleteActivity(ConversationReference)"/>
        Task<ResourceResponse> UpdateActivity(IActivity activity, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Deletes an existing activity.
        /// </summary>
        /// <param name="activityId">The ID of the activity to delete.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <remarks>Not all channels support this operation. Channels that don't, may throw an exception.</remarks>
        /// <seealso cref="OnDeleteActivity(DeleteActivityHandler)"/>
        /// <seealso cref="DeleteActivity(ConversationReference)"/>
        /// <seealso cref="SendActivities(IActivity[])"/>
        /// <seealso cref="UpdateActivity(IActivity)"/>
        Task DeleteActivity(string activityId, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Deletes an existing activity.
        /// </summary>
        /// <param name="conversationReference">The conversation containing the activity to delete.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <remarks>The conversation reference's <see cref="ConversationReference.ActivityId"/>
        /// indicates the activity in the conversation to delete.
        /// <para>Not all channels support this operation. Channels that don't, may throw an exception.</para></remarks>
        /// <seealso cref="OnDeleteActivity(DeleteActivityHandler)"/>
        /// <seealso cref="DeleteActivity(string)"/>
        /// <seealso cref="SendActivities(IActivity[])"/>
        /// <seealso cref="UpdateActivity(IActivity)"/>
        Task DeleteActivity(ConversationReference conversationReference, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Adds a response handler for send activity operations.
        /// </summary>
        /// <param name="handler">The handler to add to the context object.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The updated context object.</returns>
        /// <remarks>When the context's <see cref="SendActivity(IActivity)"/>
        /// or <see cref="SendActivities(IActivity[])"/> methods are called,
        /// the adapter calls the registered handlers in the order in which they were
        /// added to the context object.
        /// </remarks>
        /// <seealso cref="SendActivity(string, string, string)"/>
        /// <seealso cref="SendActivity(IActivity)"/>
        /// <seealso cref="SendActivities(IActivity[])"/>
        /// <seealso cref="SendActivitiesHandler"/>
        /// <seealso cref="OnUpdateActivity(UpdateActivityHandler)"/>
        /// <seealso cref="OnDeleteActivity(DeleteActivityHandler)"/>
        ITurnContext OnSendActivities(SendActivitiesHandler handler);

        /// <summary>
        /// Adds a response handler for update activity operations.
        /// </summary>
        /// <param name="handler">The handler to add to the context object.</param>
        /// <returns>The updated context object.</returns>
        /// <remarks>When the context's <see cref="UpdateActivity(IActivity)"/> is called,
        /// the adapter calls the registered handlers in the order in which they were
        /// added to the context object.
        /// </remarks>
        /// <seealso cref="UpdateActivity(IActivity)"/>
        /// <seealso cref="UpdateActivityHandler"/>
        /// <seealso cref="OnSendActivities(SendActivitiesHandler)"/>
        /// <seealso cref="OnDeleteActivity(DeleteActivityHandler)"/>
        ITurnContext OnUpdateActivity(UpdateActivityHandler handler);

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
        /// <seealso cref="DeleteActivity(ConversationReference)"/>
        /// <seealso cref="DeleteActivity(string)"/>
        /// <seealso cref="DeleteActivityHandler"/>
        /// <seealso cref="OnSendActivities(SendActivitiesHandler)"/>
        /// <seealso cref="OnUpdateActivity(UpdateActivityHandler)"/>
        ITurnContext OnDeleteActivity(DeleteActivityHandler handler);
    }
}
