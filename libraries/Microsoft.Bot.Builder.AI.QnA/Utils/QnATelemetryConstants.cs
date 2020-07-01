// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Builder.AI.QnA
{
    /// <summary>
    /// Default QnA event and property names logged using IBotTelemetryClient.
    /// </summary>
    public static class QnATelemetryConstants
    {
        /// <summary>
        /// The Key used for the custom event type within telemetry.
        /// </summary>
        public static readonly string QnaMsgEvent = "QnaMessage"; // Event name

        /// <summary>
        /// The Key used when storing a QnA Knowledge Base ID in a custom event within telemetry.
        /// </summary>
        public static readonly string KnowledgeBaseIdProperty = "knowledgeBaseId";

        /// <summary>
        /// The Key used when storing a QnA Answer in a custom event within telemetry.
        /// </summary>
        public static readonly string AnswerProperty = "answer";

        /// <summary>
        /// The Key used when storing a flag indicating if a QnA article was found in a custom event within telemetry.
        /// </summary>
        public static readonly string ArticleFoundProperty = "articleFound";

        /// <summary>
        /// The Key used when storing the Channel ID in a custom event within telemetry.
        /// </summary>
        public static readonly string ChannelIdProperty = "channelId";

        /// <summary>
        /// The Key used when storing a matched question ID in a custom event within telemetry.
        /// </summary>
        public static readonly string MatchedQuestionProperty = "matchedQuestion";

        /// <summary>
        /// The Key used when storing the identified question text in a custom event within telemetry.
        /// </summary>
        public static readonly string QuestionProperty = "question";

        /// <summary>
        /// The Key used when storing the identified question ID in a custom event within telemetry.
        /// </summary>
        public static readonly string QuestionIdProperty = "questionId";

        /// <summary>
        /// The Key used when storing a QnA Maker result score in a custom event within telemetry.
        /// </summary>
        public static readonly string ScoreProperty = "score";

        /// <summary>
        /// The Key used when storing a username in a custom event within telemetry.
        /// </summary>
        public static readonly string UsernameProperty = "username";
    }
}
