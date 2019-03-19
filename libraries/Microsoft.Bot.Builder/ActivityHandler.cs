// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder
{
    /// <summary>
    /// An implementation of the IBot interface intended for further subclassing.
    /// Derive from this class to plug in code to handle particular Activity types.
    /// Pre and post processing of Activities can be plugged in by deriving and calling
    /// the base class implementation.
    /// </summary>
    public class ActivityHandler : IBot
    {
        /// <summary>
        /// The OnTurnAsync function is called by the Adapter (for example, the <see cref="BotFrameworkAdapter"/>)
        /// at runtime in order to process an inbound Activity.
        /// </summary>
        /// <param name="turnContext">The context object for this turn.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        public virtual Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (turnContext == null)
            {
                throw new ArgumentNullException(nameof(turnContext));
            }

            if (turnContext.Activity == null)
            {
                throw new ArgumentException($"{nameof(turnContext)} must have non-null Activity.");
            }

            if (turnContext.Activity.Type == null)
            {
                throw new ArgumentException($"{nameof(turnContext)}.Activity must have non-null Type.");
            }

            switch (turnContext.Activity.Type)
            {
                case ActivityTypes.Message:
                    return OnMessageActivityAsync(new DelegatingTurnContext<IMessageActivity>(turnContext), cancellationToken);

                case ActivityTypes.ConversationUpdate:
                    return OnConversationUpdateActivityAsync(new DelegatingTurnContext<IConversationUpdateActivity>(turnContext), cancellationToken);

                case ActivityTypes.Event:
                    return OnEventActivityAsync(new DelegatingTurnContext<IEventActivity>(turnContext), cancellationToken);

                default:
                    return OnUnrecognizedActivityTypeAsync(turnContext, cancellationToken);
            }
        }

        /// <summary>
        /// Invoked when a message activity is received from the user when the base behavior of
        /// <see cref="OnTurnAsync(ITurnContext{IConversationUpdateActivity}, CancellationToken)"/> is used.
        /// If overridden, this could potentially contain conversational logic.
        /// By default, this method does nothing.
        /// </summary>
        /// <param name="turnContext">The context object for this turn.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        protected virtual Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Invoked when a conversation update activity is received from the channel when the base behavior of
        /// <see cref="OnTurnAsync(ITurnContext{IConversationUpdateActivity}, CancellationToken)"/> is used.
        /// Conversation update activities are useful when it comes to responding to users being added to or removed from the conversation.
        /// For example, a bot could respond to a user being added by greeting the user.
        /// By default, this method will call <see cref="OnMembersAddedAsync(IList{ChannelAccount}, ITurnContext{IConversationUpdateActivity}, CancellationToken)"/>
        /// if any users have been added or <see cref="OnMembersRemovedAsync(IList{ChannelAccount}, ITurnContext{IConversationUpdateActivity}, CancellationToken)"/>
        /// if any users have been removed. The method checks the member ID so that it only responds to updates regarding members other than the bot itself.
        /// </summary>
        /// <param name="turnContext">The context object for this turn.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        protected virtual Task OnConversationUpdateActivityAsync(ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            if (turnContext.Activity.MembersAdded != null)
            {
                if (turnContext.Activity.MembersAdded.Any(m => m.Id != turnContext.Activity.Recipient?.Id))
                {
                    return OnMembersAddedAsync(turnContext.Activity.MembersAdded, turnContext, cancellationToken);
                }
            }
            else if (turnContext.Activity.MembersRemoved != null)
            {
                if (turnContext.Activity.MembersRemoved.Any(m => m.Id != turnContext.Activity.Recipient?.Id))
                {
                    return OnMembersRemovedAsync(turnContext.Activity.MembersRemoved, turnContext, cancellationToken);
                }
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Invoked when members other than this bot (like a user) are added to the conversation when the base behavior of
        /// <see cref="OnConversationUpdateActivityAsync(ITurnContext{IConversationUpdateActivity}, CancellationToken)"/> is used.
        /// If overridden, this could potentially send a greeting message to the user instead of waiting for the user to send a message first.
        /// By default, this method does nothing.
        /// </summary>
        /// <param name="membersAdded">A list of all the users that have been added in the conversation update.</param>
        /// <param name="turnContext">The context object for this turn.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        protected virtual Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Invoked when members other than this bot (like a user) are removed from the conversation when the base behavior of
        /// <see cref="OnConversationUpdateActivityAsync(ITurnContext{IConversationUpdateActivity}, CancellationToken)"/> is used.
        /// This method could optionally be overridden to perform actions related to users leaving a group conversation.
        /// By default, this method does nothing.
        /// </summary>
        /// <param name="membersRemoved">A list of all the users that have been removed in the conversation update.</param>
        /// <param name="turnContext">The context object for this turn.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        protected virtual Task OnMembersRemovedAsync(IList<ChannelAccount> membersRemoved, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Invoked when an event activity is received from the connector when the base behavior of
        /// <see cref="OnTurnAsync(ITurnContext{IConversationUpdateActivity}, CancellationToken)"/> is used.
        /// Event activities can be used to communicate many different things.
        /// By default, this method will call <see cref="OnTokenResponseEventAsync(ITurnContext{IEventActivity}, CancellationToken)"/> if the
        /// activity's name is <c>tokens/response</c> or <see cref="OnEventAsync(ITurnContext{IEventActivity}, CancellationToken)"/> otherwise.
        /// A <c>tokens/response</c> event can be triggered by an <see cref="OAuthCard"/>.
        /// </summary>
        /// <param name="turnContext">The context object for this turn.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        protected virtual Task OnEventActivityAsync(ITurnContext<IEventActivity> turnContext, CancellationToken cancellationToken)
        {
            if (turnContext.Activity.Name == "tokens/response")
            {
                return OnTokenResponseEventAsync(turnContext, cancellationToken);
            }

            return OnEventAsync(turnContext, cancellationToken);
        }

        /// <summary>
        /// Invoked when a <c>tokens/response</c> event is received when the base behavior of
        /// <see cref="OnEventActivityAsync(ITurnContext{IConversationUpdateActivity}, CancellationToken)"/> is used.
        /// If using an <c>OAuthPrompt</c>, override this method to forward this <see cref="Activity"/> to the current dialog.
        /// By default, this method does nothing.
        /// </summary>
        /// <param name="turnContext">The context object for this turn.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        protected virtual Task OnTokenResponseEventAsync(ITurnContext<IEventActivity> turnContext, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Invoked when an event other than <c>tokens/response</c> is received when the base behavior of
        /// <see cref="OnEventActivityAsync(ITurnContext{IConversationUpdateActivity}, CancellationToken)"/> is used.
        /// This method could optionally be overridden if the bot is meant to handle miscellaneous events.
        /// By default, this method does nothing.
        /// </summary>
        /// <param name="turnContext">The context object for this turn.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        protected virtual Task OnEventAsync(ITurnContext<IEventActivity> turnContext, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Invoked when an activity other than a message, conversation update, or event is received when the base behavior of
        /// <see cref="OnTurnAsync(ITurnContext{IConversationUpdateActivity}, CancellationToken)"/> is used.
        /// If overridden, this could potentially respond to any of the other activity types like
        /// <see cref="ActivityTypes.ContactRelationUpdate"/> or <see cref="ActivityTypes.EndOfConversation"/>.
        /// By default, this method does nothing.
        /// </summary>
        /// <param name="turnContext">The context object for this turn.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        protected virtual Task OnUnrecognizedActivityTypeAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// A TurnContext with a strongly typed Activity property that wraps an untyped inner TurnContext.
        /// </summary>
        /// <typeparam name="T">An IActivity derived type, that is one of IMessageActivity, IConversationUpdateActivity etc.</typeparam>
        private class DelegatingTurnContext<T> : ITurnContext<T>
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

            public BotAdapter Adapter => _innerTurnContext.Adapter;

            public TurnContextStateCollection TurnState => _innerTurnContext.TurnState;

            public Activity Activity => _innerTurnContext.Activity;

            public bool Responded => _innerTurnContext.Responded;

            public Task DeleteActivityAsync(string activityId, CancellationToken cancellationToken = default(CancellationToken))
                => _innerTurnContext.DeleteActivityAsync(activityId, cancellationToken);

            public Task DeleteActivityAsync(ConversationReference conversationReference, CancellationToken cancellationToken = default(CancellationToken))
                => _innerTurnContext.DeleteActivityAsync(conversationReference, cancellationToken);

            public ITurnContext OnDeleteActivity(DeleteActivityHandler handler)
                => _innerTurnContext.OnDeleteActivity(handler);

            public ITurnContext OnSendActivities(SendActivitiesHandler handler)
                => _innerTurnContext.OnSendActivities(handler);

            public ITurnContext OnUpdateActivity(UpdateActivityHandler handler)
                => _innerTurnContext.OnUpdateActivity(handler);

            public Task<ResourceResponse[]> SendActivitiesAsync(IActivity[] activities, CancellationToken cancellationToken = default(CancellationToken))
                => _innerTurnContext.SendActivitiesAsync(activities, cancellationToken);

            public Task<ResourceResponse> SendActivityAsync(string textReplyToSend, string speak = null, string inputHint = InputHints.AcceptingInput, CancellationToken cancellationToken = default(CancellationToken))
                => _innerTurnContext.SendActivityAsync(textReplyToSend, speak, inputHint, cancellationToken);

            public Task<ResourceResponse> SendActivityAsync(IActivity activity, CancellationToken cancellationToken = default(CancellationToken))
                => _innerTurnContext.SendActivityAsync(activity, cancellationToken);

            public Task<ResourceResponse> UpdateActivityAsync(IActivity activity, CancellationToken cancellationToken = default(CancellationToken))
                => _innerTurnContext.UpdateActivityAsync(activity, cancellationToken);
        }
    }
}
