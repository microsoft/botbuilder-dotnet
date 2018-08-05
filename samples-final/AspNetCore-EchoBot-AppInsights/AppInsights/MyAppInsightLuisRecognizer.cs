using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Ai.Luis;
using Newtonsoft.Json.Linq;

namespace AspNetCore_EchoBot_With_AppInsights.AppInsights
{
    /// <summary>
    /// MyAppInsightLuisRecognizer invokes the Luis Recognizer and logs some results into Application Insights.
    /// Logs the Top Intent, Sentiment (label/score), (Optionally) Original Text
    /// 
    /// Along with Conversation and ActivityID.
    /// See <seealso cref="LuisRecognizer"/> for additional information.
    /// </summary>
    public class MyAppInsightLuisRecognizer : LuisRecognizer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MyAppInsightLuisRecognizer"/> class.
        /// </summary>
        /// <param name="application">The LUIS _application to use to recognize text.</param>
        /// <param name="predictionOptions">The LUIS prediction options to use.</param>
        /// <param name="includeApiResults">TRUE to include raw LUIS API response.</param>
        public MyAppInsightLuisRecognizer(LuisApplication application, LuisPredictionOptions predictionOptions = null, bool includeApiResults = false)
            : base(application, predictionOptions, includeApiResults)
        {
        }


        /// <summary>
        /// Analyze the current message text and return results of the analysis (Suggested actions and intents).
        /// </summary>
        /// <param name="context">Context object containing information for a single turn of conversation with a user.</param>
        /// <param name="ct">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <param name="logOriginalMessage">Determines if the original message is logged into Application Insights.  This is a privacy consideration.</param>
        /// <returns>The LUIS results of the analysis of the current message text in the current turn's context activity.</returns>
        public async Task<RecognizerResult> RecognizeAsync(ITurnContext context, CancellationToken ct, bool logOriginalMessage = false)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            // Call Luis Recognizer
            var recognizerResult = await base.RecognizeAsync(context, ct);

            // Log into Application Insights a portion of the Luis Results
            var conversationId = context.Activity.Conversation.Id;

            // Find the Telemetry Client
            if (context.Services.TryGetValue("AppInsightsLoggerMiddleware.AppInsightsContext", out var telemetryClient) && recognizerResult != null)
            {
                var topLuisIntent = recognizerResult.GetTopScoringIntent();
                var intentScore = topLuisIntent.score.ToString("N2");
                

                // Add the intent score and conversation id properties
                Dictionary<string, string> telemetryProperties = new Dictionary<string, string>();
                telemetryProperties.Add(MyAppInsightsConstants.ActivityIdProperty, context.Activity.Id);
                telemetryProperties.Add(MyAppInsightsConstants.IntentProperty, topLuisIntent.intent);
                telemetryProperties.Add(MyAppInsightsConstants.IntentScoreProperty, intentScore);
                if (recognizerResult.Properties.TryGetValue("sentiment", out var sentiment) && sentiment is JObject)
                {
                    if (((JObject)sentiment).TryGetValue("label", out var label))
                    {
                        telemetryProperties.Add(MyAppInsightsConstants.SentimentLabelProperty, label.Value<string>());
                    }
                    if (((JObject)sentiment).TryGetValue("score", out var score))
                    {
                        telemetryProperties.Add(MyAppInsightsConstants.SentimentScoreProperty, score.Value<string>());
                    }
                }
                

                if (!string.IsNullOrEmpty(conversationId))
                {
                    telemetryProperties.Add(MyAppInsightsConstants.ConversationIdProperty, conversationId);
                }
                

                // Add Luis Entitites 
                var entities = new List<string>();
                foreach (var entity in recognizerResult.Entities)
                {
                    if (!entity.Key.ToString().Equals("$instance"))
                    {
                        entities.Add($"{entity.Key}: {entity.Value.First}");
                    }
                }

                // For some customers, logging user name within Application Insights might be an issue so have provided a config setting to disable this feature     
                if (logOriginalMessage && !string.IsNullOrEmpty(context.Activity.Text))
                {
                    telemetryProperties.Add(MyAppInsightsConstants.QuestionProperty, context.Activity.Text);
                }

                // Track the event
                ((TelemetryClient)telemetryClient).TrackEvent($"{MyAppInsightsConstants.IntentPrefix}.{topLuisIntent.intent}", telemetryProperties);
            }

            return recognizerResult;
        }
    }

    /// <summary>
    /// The Application Insights property names that we're logging.
    /// In this example, we're not logging any metrics.
    /// </summary>
    public static class MyAppInsightsConstants
    {
        public const string IntentPrefix = "LuisIntent";  // Application Insights Custom Event name (with Intent)
        public const string IntentProperty = "Intent";
        public const string IntentScoreProperty = "IntentScore";
        public const string ConversationIdProperty = "ConversationId";
        public const string QuestionProperty = "Question";
        public const string ActivityIdProperty = "ActivityId";
        public const string SentimentLabelProperty = "SentimentLabel";
        public const string SentimentScoreProperty = "SentimentScore";
    }
}
