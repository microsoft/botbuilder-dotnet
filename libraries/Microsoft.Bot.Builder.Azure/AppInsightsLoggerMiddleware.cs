// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Azure
{
    /// <summary>
    /// Middleware for logging incoming activitites into Application Insights.
    /// In addition, registers a service that other Application Insights components can log
    /// telemetry.
    /// If this component is not registered, visibility within the Bot is not logged.
    /// </summary>
    public class AppInsightsLoggerMiddleware : IMiddleware
    {
        public static readonly string AppInsightsServiceKey = $"{nameof(AppInsightsLoggerMiddleware)}.AppInsightsContext";
        public static readonly string BotMsgEvent = "BotMessageReceived";
        private TelemetryClient _telemetryClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="AppInsightsLoggerMiddleware"/> class.
        /// </summary>
        /// <param name="instrumentationKey">The Application Insights instrumentation key.  See Application Insights for more information.</param>
        /// <param name="logUserName"> (Optional) Enable/Disable logging user name within Application Insights.</param>
        /// <param name="logOriginalMessage"> (Optional) Enable/Disable logging original message name within Application Insights.</param>
        /// <param name="config"> (Optional) TelemetryConfiguration to use for Application Insights.</param>
        public AppInsightsLoggerMiddleware(string instrumentationKey, bool logUserName = false, bool logOriginalMessage = false, TelemetryConfiguration config = null)
        {
            if (string.IsNullOrEmpty(instrumentationKey))
            {
                throw new ArgumentNullException(nameof(instrumentationKey));
            }

            var telemetryConfiguration = config ?? new TelemetryConfiguration(instrumentationKey);
            _telemetryClient = new TelemetryClient(telemetryConfiguration);
            LogUserName = logUserName;
            LogOriginalMessage = logOriginalMessage;
        }

        /// <summary>
        /// Gets a value indicating whether indicates whether to log the user name into the BotMessageReceived event.
        /// </summary>
        /// <value>
        /// A value indicating whether indicates whether to log the user name into the BotMessageReceived event.
        /// </value>
        public bool LogUserName { get; }

        /// <summary>
        /// Gets a value indicating whether indicates whether to log the original message into the BotMessageReceived event.
        /// </summary>
        /// <value>
        /// Indicates whether to log the original message into the BotMessageReceived event.
        /// </value>
        public bool LogOriginalMessage { get; }

        /// <summary>
        /// Records incoming and outgoing activities to the Application Insights store.
        /// </summary>
        /// <param name="context">The context object for this turn.</param>
        /// <param name="nextTurn">The delegate to call to continue the bot middleware pipeline.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <seealso cref="ITurnContext"/>
        /// <seealso cref="Bot.Schema.IActivity"/>
        public async Task OnTurnAsync(ITurnContext context, NextDelegate nextTurn, CancellationToken cancellationToken)
        {
            BotAssert.ContextNotNull(context);

            context.Services.Add(AppInsightsLoggerMiddleware.AppInsightsServiceKey, _telemetryClient);

            // log incoming activity at beginning of turn
            if (context.Activity != null)
            {
                Activity activity = context.Activity;
                if (string.IsNullOrEmpty((string)activity.From.Properties["role"]))
                {
                    activity.From.Properties["role"] = "user";
                }

                // Context properties for App Insights
                if (!string.IsNullOrEmpty(activity.Conversation.Id))
                {
                    _telemetryClient.Context.Session.Id = activity.Conversation.Id;
                }

                if (!string.IsNullOrEmpty(activity.From.Id))
                {
                    _telemetryClient.Context.User.Id = activity.From.Id;
                }

                // Log the Application Insights Bot Message Received
                _telemetryClient.TrackEvent(BotMsgEvent, FillEventProperties(context));
            }

            if (nextTurn != null)
            {
                await nextTurn(cancellationToken).ConfigureAwait(false);
            }
        }

        private Dictionary<string, string> FillEventProperties(ITurnContext context)
        {
            Activity activity = context.Activity;

            var properties = new Dictionary<string, string>()
                {
                    { AppInsightsConstants.ChannelProperty, activity.ChannelId },
                    { AppInsightsConstants.FromIdProperty, activity.From.Id },
                    { AppInsightsConstants.ConversationIdProperty, activity.Conversation.Id },
                    { AppInsightsConstants.ConversationNameProperty, activity.Conversation.Name },
                    { AppInsightsConstants.LocaleProperty, activity.Locale },
                };

            // For some customers, logging user name within Application Insights might be an issue so have provided a config setting to disable this feature
            if (LogUserName && !string.IsNullOrEmpty(activity.From.Name))
            {
                properties.Add(AppInsightsConstants.FromNameProperty, activity.From.Name);
            }

            // For some customers, logging the utterances within Application Insights might be an so have provided a config setting to disable this feature
            if (LogOriginalMessage && !string.IsNullOrEmpty(activity.Text))
            {
                properties.Add(AppInsightsConstants.TextProperty, activity.Text);
            }

            return properties;
        }

        public static class AppInsightsConstants
        {
            public const string ChannelProperty = "Channel";
            public const string FromIdProperty = "FromId";
            public const string FromNameProperty = "FromName";
            public const string ConversationIdProperty = "ConversationId";
            public const string ConversationNameProperty = "ConversationName";
            public const string TextProperty = "Text";
            public const string LocaleProperty = "Locale";
        }
    }

}
