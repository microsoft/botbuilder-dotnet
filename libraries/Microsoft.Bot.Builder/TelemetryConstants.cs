// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Builder
{
    /// <summary>
    /// Defines names of common properties for use with a <see cref="IBotTelemetryClient"/> object.
    /// </summary>
    public static class TelemetryConstants
    {
        public static readonly string ChannelIdProperty = "channelId";
        public static readonly string ConversationIdProperty = "conversationId";
        public static readonly string ConversationNameProperty = "conversationName";
        public static readonly string DialogIdProperty = "dialogId";
        public static readonly string FromIdProperty = "fromId";
        public static readonly string FromNameProperty = "fromName";
        public static readonly string LocaleProperty = "locale";
        public static readonly string RecipientIdProperty = "recipientId";
        public static readonly string RecipientNameProperty = "recipientName";
        public static readonly string ReplyActivityIDProperty = "replyActivityId";
        public static readonly string TextProperty = "text";
        public static readonly string SpeakProperty = "speak";
        public static readonly string UserIdProperty = "userId";
        public static readonly string AttachmentsProperty = "attachments";
    }
}
