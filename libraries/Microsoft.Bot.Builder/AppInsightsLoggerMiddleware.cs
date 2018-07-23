// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder
{
    /// <summary>
    /// Middleware for logging incoming activitites into Application Insights.
    /// In addition, registers a service that other Application Insights components can log
    /// telemetry.
    /// If this component is not registered, visibility within the Bot is not logged.
    /// </summary>
    public class AppInsightsLoggerMiddleware : IMiddleware
    {
        public const string AppInsightServiceKey = "AppInsightsContext";
        public const string BotMsgEvent = "BotMessageReceived";
        public const string ChannelProperty = "Channel";
        public const string FromIdProperty = "FromId";
        public const string FromNameProperty = "FromName";
        public const string ConversationIdProperty = "ConversationId";
        public const string ConversationNameProperty = "ConversationName";
        public const string TextProperty = "Text";
        public const string LocaleProperty = "Locale";
        public const string LanguageProperty = "Language";
        public const string SentimentProperty = "Sentiment";
        public const string KeyPhrasesProperty = "KeyPhrases";
        public const string IntentScoreProperty = "Score";
        public const string ConfidenceScoreProperty = "ConfidenceScore";
        public const string QuestionProperty = "Question";
        public const string FoundInKnowledgeSourceProperty = "FoundInKnowledgeSource";
        public const string KnowledgeBasedUsedProperty = "KnowledgeBasedUsed";
        public const string UserAcceptedAnswerProperty = "UserAcceptedAnswer";
        public const string IntentProperty = "Intent";
        public const string ButtonValueProperty = "ButtonValue";
        public const string KnowledgeItemsDiscardedProperty = "KnowledgeItemsDiscarded";
        public const string QnAResponseProperty = "QnAResponse";
        public const string ErrorProperty = "Error";
        public const string ErrorHeadlineProperty = "ErrorHeadline";
        public const string ErrorDataProperty = "ErrorData";
        public const string NoResponseGivenProperty = "NoResponseGiven";
        private static readonly JsonSerializerSettings _jsonSettings = new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore };
        private static TelemetryClient _telemetryClient;
        private static bool _logUserName;
        private static bool _logOriginalMessage;

        /// <summary>
        /// Initializes a new instance of the <see cref="AppInsightsLoggerMiddleware"/> class.
        /// </summary>
        /// <param name="instrumentationKey">The Application Insights instrumentation key.  See Application Insights for more information.</param>
        /// <param name="logUserName"> (Optional) Enable/Disable logging user name within Application Insights.</param>
        /// <param name="logOriginalMessage"> (Optional) Enable/Disable logging original message name within Application Insights.</param>
        /// <param name="config"> (Optional) TelemetryConfiguration to use for Application Insights.</param>
        public AppInsightsLoggerMiddleware(string instrumentationKey, bool logUserName = false, bool logOriginalMessage = false, TelemetryConfiguration config = null)
        {
            if (instrumentationKey == null)
            {
                throw new ArgumentNullException(nameof(instrumentationKey));
            }

            var telemetryConfiguration = config ?? new TelemetryConfiguration(instrumentationKey);
            _telemetryClient = new TelemetryClient(telemetryConfiguration);
            _logUserName = logUserName;
            _logOriginalMessage = logOriginalMessage;
        }

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

            context.Services.Add(AppInsightServiceKey, _telemetryClient);

            // log incoming activity at beginning of turn
            if (context.Activity != null)
            {
                if (string.IsNullOrEmpty((string)context.Activity.From.Properties["role"]))
                {
                    context.Activity.From.Properties["role"] = "user";
                }

                // Context properties for App Insights
                if (!string.IsNullOrEmpty(context.Activity.Conversation.Id))
                {
                    _telemetryClient.Context.Session.Id = context.Activity.Conversation.Id;
                }

                if (!string.IsNullOrEmpty(context.Activity.From.Id))
                {
                    _telemetryClient.Context.User.Id = context.Activity.From.Id;
                }

                // Log the Application Insights Bot Message Received
                try
                {
                    _telemetryClient.TrackEvent(BotMsgEvent, FillEventProperties(context));
                }
                catch (Exception ex)
                {
                    _telemetryClient.TrackException(ex);
                }
            }

            // process bot logic
            await nextTurn(cancellationToken).ConfigureAwait(false);
        }

        private static Dictionary<string, string> FillEventProperties(ITurnContext context)
        {
            // Payload properties
            var properties = new Dictionary<string, string>()
                {
                    { ChannelProperty, context.Activity.ChannelId },
                    { FromIdProperty, context.Activity.From.Id },
                    { ConversationIdProperty, context.Activity.Conversation.Id },
                    { ConversationNameProperty, context.Activity.Conversation.Name },
                    { SentimentProperty, null },
                    { LocaleProperty, context.Activity.Locale },
                    { LanguageProperty, context.Activity.Locale },
                };

            // For some customers, logging user name within Application Insights might be an issue so have provided a config setting to disable this feature
            if (_logUserName && !string.IsNullOrEmpty(context.Activity.From.Name))
            {
                properties.Add(FromNameProperty, context.Activity.From.Name);
            }

            // For some customers, logging the utterances within Application Insights might be an so have provided a config setting to disable this feature
            if (_logOriginalMessage && !string.IsNullOrEmpty(context.Activity.Text))
            {
                properties.Add(TextProperty, context.Activity.Text);
            }

            return properties;
        }
    }
}
