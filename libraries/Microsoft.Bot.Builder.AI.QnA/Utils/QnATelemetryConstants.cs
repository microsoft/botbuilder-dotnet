// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Builder.AI.QnA
{
    /// <summary>
    /// Default QnA event and property names logged using IBotTelemetryClient.
    /// </summary>
    public static class QnATelemetryConstants
    {
        public static readonly string QnaMsgEvent = "QnaMessage"; // Event name
        public static readonly string KnowledgeBaseIdProperty = "knowledgeBaseId";
        public static readonly string AnswerProperty = "answer";
        public static readonly string ArticleFoundProperty = "articleFound";
        public static readonly string ChannelIdProperty = "channelId";
        public static readonly string MatchedQuestionProperty = "matchedQuestion";
        public static readonly string QuestionProperty = "question";
        public static readonly string QuestionIdProperty = "questionId";
        public static readonly string ScoreProperty = "score";
        public static readonly string UsernameProperty = "username";
    }
}
