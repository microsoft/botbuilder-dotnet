// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder
{
    /// <summary>
    /// Uses a <see cref="IBotTelemetryClient"/> object to log incoming, outgoing, updated, or deleted message activities.
    /// </summary>
    public class TelemetryLoggerMiddleware : IMiddleware
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TelemetryLoggerMiddleware"/> class.
        /// </summary>
        /// <param name="telemetryClient">The telemetry client to send telemetry events to.</param>
        /// <param name="logPersonalInformation">`true` to include personally identifiable information; otherwise, `false`.</param>
        public TelemetryLoggerMiddleware(IBotTelemetryClient telemetryClient, bool logPersonalInformation = false)
        {
            TelemetryClient = telemetryClient ?? new NullBotTelemetryClient();
            LogPersonalInformation = logPersonalInformation;
        }

        /// <summary>
        /// Gets a value indicating whether to include personal information that came from the user.
        /// </summary>
        /// <value>`true` to include personally identifiable information; otherwise, `false`.</value>
        /// <remarks>
        /// If true, personal information is included in calls to the telemetry client's
        /// <see cref="IBotTelemetryClient.TrackEvent(string, IDictionary{string, string}, IDictionary{string, double})"/> method;
        /// otherwise this information is filtered out.
        /// </remarks>
        public bool LogPersonalInformation { get; }

        /// <summary>
        /// Gets The telemetry client to send telemetry events to.
        /// </summary>
        /// <value>
        /// The <see cref="IBotTelemetryClient"/> this middleware uses to log events.
        /// </value>
        [JsonIgnore]
        public IBotTelemetryClient TelemetryClient { get; }

        /// <summary>
        /// Logs events for incoming, outgoing, updated, or deleted message activities, using the <see cref="TelemetryClient"/>.
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
        /// Uses the telemetry client's
        /// <see cref="IBotTelemetryClient.TrackEvent(string, IDictionary{string, string}, IDictionary{string, double})"/> method to
        /// log telemetry data when a message is received from the user.
        /// The event name is <see cref="TelemetryLoggerConstants.BotMsgReceiveEvent"/>.
        /// </summary>
        /// <param name="activity">Current activity sent from user.</param>
        /// <param name="cancellation">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        protected virtual async Task OnReceiveActivityAsync(Activity activity, CancellationToken cancellation)
        {
            TelemetryClient.TrackEvent(TelemetryLoggerConstants.BotMsgReceiveEvent, await FillReceiveEventPropertiesAsync(activity).ConfigureAwait(false));
            return;
        }

        /// <summary>
        /// Uses the telemetry client's
        /// <see cref="IBotTelemetryClient.TrackEvent(string, IDictionary{string, string}, IDictionary{string, double})"/> method to
        /// log telemetry data when the bot sends the user a message. It uses the telemetry client's
        /// The event name is <see cref="TelemetryLoggerConstants.BotMsgSendEvent"/>.
        /// </summary>
        /// <param name="activity">Current activity sent from user.</param>
        /// <param name="cancellation">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        protected virtual async Task OnSendActivityAsync(Activity activity, CancellationToken cancellation)
        {
            TelemetryClient.TrackEvent(TelemetryLoggerConstants.BotMsgSendEvent, await FillSendEventPropertiesAsync(activity).ConfigureAwait(false));
            return;
        }

        /// <summary>
        /// Uses the telemetry client's
        /// <see cref="IBotTelemetryClient.TrackEvent(string, IDictionary{string, string}, IDictionary{string, double})"/> method to
        /// log telemetry data when the bot updates a message it sent previously.
        /// The event name is <see cref="TelemetryLoggerConstants.BotMsgUpdateEvent"/>.
        /// </summary>
        /// <param name="activity">Current activity sent from user.</param>
        /// <param name="cancellation">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        protected virtual async Task OnUpdateActivityAsync(Activity activity, CancellationToken cancellation)
        {
            TelemetryClient.TrackEvent(TelemetryLoggerConstants.BotMsgUpdateEvent, await FillUpdateEventPropertiesAsync(activity).ConfigureAwait(false));
            return;
        }

        /// <summary>
        /// Uses the telemetry client's
        /// <see cref="IBotTelemetryClient.TrackEvent(string, IDictionary{string, string}, IDictionary{string, double})"/> method to
        /// log telemetry data when the bot deletes a message it sent previously.
        /// The event name is <see cref="TelemetryLoggerConstants.BotMsgDeleteEvent"/>.
        /// </summary>
        /// <param name="activity">Current activity sent from user.</param>
        /// <param name="cancellation">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        protected virtual async Task OnDeleteActivityAsync(Activity activity, CancellationToken cancellation)
        {
            TelemetryClient.TrackEvent(TelemetryLoggerConstants.BotMsgDeleteEvent, await FillDeleteEventPropertiesAsync(activity).ConfigureAwait(false));
            return;
        }

        /// <summary>
        /// Fills event properties for the <see cref="TelemetryLoggerConstants.BotMsgReceiveEvent"/> event.
        /// If the <see cref="LogPersonalInformation"/> is true, filters out the sender's name and the
        /// message's text and speak fields.
        /// </summary>
        /// <param name="activity">The message activity sent from user.</param>
        /// <param name="additionalProperties">Additional properties to add to the event.</param>
        /// <returns>The properties and their values to log when a message is received from the user.</returns>
        protected Task<Dictionary<string, string>> FillReceiveEventPropertiesAsync(Activity activity, Dictionary<string, string> additionalProperties = null)
        {
            var properties = new Dictionary<string, string>()
                {
                    { TelemetryConstants.FromIdProperty, activity.From?.Id },
                    { TelemetryConstants.ConversationNameProperty, activity.Conversation.Name },
                    { TelemetryConstants.LocaleProperty, activity.Locale },
                    { TelemetryConstants.RecipientIdProperty, activity.Recipient.Id },
                    { TelemetryConstants.RecipientNameProperty, activity.Recipient.Name },
                };

            // Use the LogPersonalInformation flag to toggle logging PII data, text and user name are common examples
            if (LogPersonalInformation)
            {
                if (!string.IsNullOrWhiteSpace(activity.From?.Name))
                {
                    properties.Add(TelemetryConstants.FromNameProperty, activity.From?.Name);
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
        /// Fills event properties for the <see cref="TelemetryLoggerConstants.BotMsgSendEvent"/> event.
        /// If the <see cref="LogPersonalInformation"/> is true, filters out the recipient's name and the
        /// message's text and speak fields.
        /// </summary>
        /// <param name="activity">The user's activity to which the bot is responding.</param>
        /// <param name="additionalProperties">Additional properties to add to the event.</param>
        /// <returns>The properties and their values to log when the bot sends the user a message.</returns>
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

                if (activity.Attachments != null && activity.Attachments.Any())
                {
                    properties.Add(TelemetryConstants.AttachmentsProperty, JsonConvert.SerializeObject(activity.Attachments));
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
        /// Fills event properties for the <see cref="TelemetryLoggerConstants.BotMsgUpdateEvent"/> event.
        /// If the <see cref="LogPersonalInformation"/> is true, filters out the message's text field.
        /// </summary>
        /// <param name="activity">Last activity sent from user.</param>
        /// <param name="additionalProperties">Additional properties to add to the event.</param>
        /// <returns>The properties and their values to log when the bot updates a message it sent previously.</returns>
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
        /// Fills event properties for the <see cref="TelemetryLoggerConstants.BotMsgDeleteEvent"/> event.
        /// </summary>
        /// <param name="activity">The Activity object deleted by bot.</param>
        /// <param name="additionalProperties">Additional properties to add to the event.</param>
        /// <returns>The properties and their values to log when the bot deletes a message it sent previously.</returns>
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
