// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder
{
    /// <summary>
    /// Middleware for logging incoming, outgoing, updated or deleted Activity messages.
    /// Uses the IBotTelemetryClient interface.
    /// </summary>
    public class TelemetryLoggerMiddleware : IMiddleware
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TelemetryLoggerMiddleware"/> class.
        /// </summary>
        /// <param name="telemetryClient">The IBotTelemetryClient implementation used for registering telemetry events.</param>
        /// <param name="logPersonalInformation"> (Optional) TRUE to include personally identifiable information.</param>
        public TelemetryLoggerMiddleware(IBotTelemetryClient telemetryClient, bool logPersonalInformation = false)
        {
            TelemetryClient = telemetryClient ?? new NullBotTelemetryClient();
            LogPersonalInformation = logPersonalInformation;
        }

        /// <summary>
        /// Gets a value indicating whether determines whether to log personal information that came from the user.
        /// </summary>
        /// <value>If true, will log personal information into the IBotTelemetryClient.TrackEvent method; otherwise the properties will be filtered.</value>
        public bool LogPersonalInformation { get; }

        /// <summary>
        /// Gets the currently configured <see cref="IBotTelemetryClient"/> that logs the QnaMessage event.
        /// </summary>
        /// <value>
        /// The <see cref="IBotTelemetryClient"/> being used to log events.
        /// </value>
        public IBotTelemetryClient TelemetryClient { get; }

        /// <summary>
        /// Logs events based on incoming and outgoing activities using the <see cref="IBotTelemetryClient"/> interface.
        /// </summary>
        /// <param name="context">The context object for this turn.</param>
        /// <param name="nextTurn">The delegate to call to continue the bot middleware pipeline.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <seealso cref="ITurnContext"/>
        /// <seealso cref="Bot.Schema.IActivity"/>
        public virtual async Task OnTurnAsync(ITurnContext context, NextDelegate nextTurn, CancellationToken cancellationToken)
        {
            BotAssert.ContextNotNull(context);

            // log incoming activity at beginning of turn
            if (context.Activity != null)
            {
                var activity = context.Activity;

                // Log Bot Message Received
                await OnReceiveActivityAsync(activity, cancellationToken).ConfigureAwait(false);
            }

            // hook up onSend pipeline
            context.OnSendActivities(async (ctx, activities, nextSend) =>
            {
                // run full pipeline
                var responses = await nextSend().ConfigureAwait(false);

                foreach (var activity in activities)
                {
                    await OnSendActivityAsync(activity, cancellationToken).ConfigureAwait(false);
                }

                return responses;
            });

            // hook up update activity pipeline
            context.OnUpdateActivity(async (ctx, activity, nextUpdate) =>
            {
                // run full pipeline
                var response = await nextUpdate().ConfigureAwait(false);

                await OnUpdateActivityAsync(activity, cancellationToken).ConfigureAwait(false);

                return response;
            });

            // hook up delete activity pipeline
            context.OnDeleteActivity(async (ctx, reference, nextDelete) =>
            {
                // run full pipeline
                await nextDelete().ConfigureAwait(false);

                var deleteActivity = new Activity
                {
                    Type = ActivityTypes.MessageDelete,
                    Id = reference.ActivityId,
                }
                .ApplyConversationReference(reference, isIncoming: false)
                .AsMessageDeleteActivity();

                await OnDeleteActivityAsync((Activity)deleteActivity, cancellationToken).ConfigureAwait(false);
            });

            if (nextTurn != null)
            {
                await nextTurn(cancellationToken).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Invoked when a message is received from the user.
        /// Performs logging of telemetry data using the IBotTelemetryClient.TrackEvent() method.
        /// This event name used is "BotMessageReceived".
        /// </summary>
        /// <param name="activity">Current activity sent from user.</param>
        /// <param name="cancellation"> cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        protected virtual async Task OnReceiveActivityAsync(Activity activity, CancellationToken cancellation)
        {
            TelemetryClient.TrackEvent(TelemetryLoggerConstants.BotMsgReceiveEvent, await FillReceiveEventPropertiesAsync(activity).ConfigureAwait(false));
            return;
        }

        /// <summary>
        /// Invoked when the bot sends a message to the user.
        /// Performs logging of telemetry data using the IBotTelemetryClient.TrackEvent() method.
        /// This event name used is "BotMessageSend".
        /// </summary>
        /// <param name="activity">Current activity sent from user.</param>
        /// <param name="cancellation"> cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        protected virtual async Task OnSendActivityAsync(Activity activity, CancellationToken cancellation)
        {
            TelemetryClient.TrackEvent(TelemetryLoggerConstants.BotMsgSendEvent, await FillSendEventPropertiesAsync(activity).ConfigureAwait(false));
            return;
        }

        /// <summary>
        /// Invoked when the bot updates a message.
        /// Performs logging of telemetry data using the IBotTelemetryClient.TrackEvent() method.
        /// This event name used is "BotMessageUpdate".
        /// </summary>
        /// <param name="activity">Current activity sent from user.</param>
        /// <param name="cancellation"> cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        protected virtual async Task OnUpdateActivityAsync(Activity activity, CancellationToken cancellation)
        {
            TelemetryClient.TrackEvent(TelemetryLoggerConstants.BotMsgUpdateEvent, await FillUpdateEventPropertiesAsync(activity).ConfigureAwait(false));
            return;
        }

        /// <summary>
        /// Invoked when the bot deletes a message.
        /// Performs logging of telemetry data using the IBotTelemetryClient.TrackEvent() method.
        /// This event name used is "BotMessageDelete".
        /// </summary>
        /// <param name="activity">Current activity sent from user.</param>
        /// <param name="cancellation"> cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        protected virtual async Task OnDeleteActivityAsync(Activity activity, CancellationToken cancellation)
        {
            TelemetryClient.TrackEvent(TelemetryLoggerConstants.BotMsgDeleteEvent, await FillDeleteEventPropertiesAsync(activity).ConfigureAwait(false));
            return;
        }

        /// <summary>
        /// Fills the event properties for the BotMessageReceived.
        /// Adheres to the LogPersonalInformation flag to filter Name, Text and Speak properties.
        /// </summary>
        /// <param name="activity">Last activity sent from user.</param>
        /// <param name="additionalProperties">Additional properties to add to the event.</param>
        /// <returns>A dictionary that is sent as "Properties" to IBotTelemetryClient.TrackEvent method for the BotMessageReceived event.</returns>
        protected Task<Dictionary<string, string>> FillReceiveEventPropertiesAsync(Activity activity, Dictionary<string, string> additionalProperties = null)
        {
            var properties = new Dictionary<string, string>()
                {
                    { TelemetryConstants.FromIdProperty, activity.From.Id },
                    { TelemetryConstants.ConversationNameProperty, activity.Conversation.Name },
                    { TelemetryConstants.LocaleProperty, activity.Locale },
                    { TelemetryConstants.RecipientIdProperty, activity.Recipient.Id },
                    { TelemetryConstants.RecipientNameProperty, activity.Recipient.Name },
                };

            // Use the LogPersonalInformation flag to toggle logging PII data, text and user name are common examples
            if (LogPersonalInformation)
            {
                if (!string.IsNullOrWhiteSpace(activity.From.Name))
                {
                    properties.Add(TelemetryConstants.FromNameProperty, activity.From.Name);
                }

                if (!string.IsNullOrWhiteSpace(activity.Text))
                {
                    properties.Add(TelemetryConstants.TextProperty, activity.Text);
                }

                if (!string.IsNullOrWhiteSpace(activity.Speak))
                {
                    properties.Add(TelemetryConstants.SpeakProperty, activity.Speak);
                }
            }

            // Additional Properties can override "stock" properties.
            if (additionalProperties != null)
            {
                return Task.FromResult(additionalProperties.Concat(properties)
                           .GroupBy(kv => kv.Key)
                           .ToDictionary(g => g.Key, g => g.First().Value));
            }

            return Task.FromResult(properties);
        }

        /// <summary>
        /// Fills the event properties for BotMessageSend.
        /// These properties are logged when an activity message is sent by the Bot to the user.
        /// </summary>
        /// <param name="activity">Last activity sent from user.</param>
        /// <param name="additionalProperties">Additional properties to add to the event.</param>
        /// <returns>A dictionary that is sent as "Properties" to IBotTelemetryClient.TrackEvent method for the BotMessageSend event.</returns>
        protected Task<Dictionary<string, string>> FillSendEventPropertiesAsync(Activity activity, Dictionary<string, string> additionalProperties = null)
        {
            var properties = new Dictionary<string, string>()
                {
                    { TelemetryConstants.ReplyActivityIDProperty, activity.ReplyToId },
                    { TelemetryConstants.RecipientIdProperty, activity.Recipient.Id },
                    { TelemetryConstants.ConversationNameProperty, activity.Conversation.Name },
                    { TelemetryConstants.LocaleProperty, activity.Locale },
                };

            // Use the LogPersonalInformation flag to toggle logging PII data, text and user name are common examples
            if (LogPersonalInformation)
            {
                if (!string.IsNullOrWhiteSpace(activity.Recipient.Name))
                {
                    properties.Add(TelemetryConstants.RecipientNameProperty, activity.Recipient.Name);
                }

                if (!string.IsNullOrWhiteSpace(activity.Text))
                {
                    properties.Add(TelemetryConstants.TextProperty, activity.Text);
                }

                if (!string.IsNullOrWhiteSpace(activity.Speak))
                {
                    properties.Add(TelemetryConstants.SpeakProperty, activity.Speak);
                }
            }

            // Additional Properties can override "stock" properties.
            if (additionalProperties != null)
            {
                return Task.FromResult(additionalProperties.Concat(properties)
                           .GroupBy(kv => kv.Key)
                           .ToDictionary(g => g.Key, g => g.First().Value));
            }

            return Task.FromResult(properties);
        }

        /// <summary>
        /// Fills the event properties for BotMessageUpdate.
        /// These properties are logged when an activity message is updated by the Bot.
        /// For example, if a card is interacted with by the use, and the card needs to be updated to reflect
        /// some interaction.
        /// </summary>
        /// <param name="activity">Last activity sent from user.</param>
        /// <param name="additionalProperties">Additional properties to add to the event.</param>
        /// <returns>A dictionary that is sent as "Properties" to IBotTelemetryClient.TrackEvent method for the BotMessageUpdate event.</returns>
        protected Task<Dictionary<string, string>> FillUpdateEventPropertiesAsync(Activity activity, Dictionary<string, string> additionalProperties = null)
        {
            var properties = new Dictionary<string, string>()
                {
                    { TelemetryConstants.RecipientIdProperty, activity.Recipient.Id },
                    { TelemetryConstants.ConversationIdProperty, activity.Conversation.Id },
                    { TelemetryConstants.ConversationNameProperty, activity.Conversation.Name },
                    { TelemetryConstants.LocaleProperty, activity.Locale },
                };

            // Use the LogPersonalInformation flag to toggle logging PII data, text is a common example
            if (LogPersonalInformation && !string.IsNullOrWhiteSpace(activity.Text))
            {
                properties.Add(TelemetryConstants.TextProperty, activity.Text);
            }

            // Additional Properties can override "stock" properties.
            if (additionalProperties != null)
            {
                return Task.FromResult(additionalProperties.Concat(properties)
                           .GroupBy(kv => kv.Key)
                           .ToDictionary(g => g.Key, g => g.First().Value));
            }

            return Task.FromResult(properties);
        }

        /// <summary>
        /// Fills the event properties for BotMessageDelete.
        /// These properties are logged when an activity message is deleted by the Bot.
        /// </summary>
        /// <param name="activity">The Activity object deleted by bot.</param>
        /// <param name="additionalProperties">Additional properties to add to the event.</param>
        /// <returns>A dictionary that is sent as "Properties" to IBotTelemetryClient.TrackEvent method for the BotMessageDelete event.</returns>
        protected Task<Dictionary<string, string>> FillDeleteEventPropertiesAsync(IMessageDeleteActivity activity, Dictionary<string, string> additionalProperties = null)
        {
            var properties = new Dictionary<string, string>()
                {
                    { TelemetryConstants.RecipientIdProperty, activity.Recipient.Id },
                    { TelemetryConstants.ConversationIdProperty, activity.Conversation.Id },
                    { TelemetryConstants.ConversationNameProperty, activity.Conversation.Name },
                };

            // Additional Properties can override "stock" properties.
            if (additionalProperties != null)
            {
                return Task.FromResult(additionalProperties.Concat(properties)
                           .GroupBy(kv => kv.Key)
                           .ToDictionary(g => g.Key, g => g.First().Value));
            }

            return Task.FromResult(properties);
        }
    }
}
