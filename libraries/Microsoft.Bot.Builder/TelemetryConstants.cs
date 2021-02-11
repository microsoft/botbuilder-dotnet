// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Builder
{
    /// <summary>
    /// Defines names of common properties for use with a <see cref="IBotTelemetryClient"/> object.
    /// </summary>
    public static class TelemetryConstants
    {
        /// <summary>
        /// The telemetry property value for channel id.
        /// </summary>
        public static readonly string ChannelIdProperty = "channelId";

        /// <summary>
        /// The telemetry property value for conversation id.
        /// </summary>
        public static readonly string ConversationIdProperty = "conversationId";

        /// <summary>
        /// The telemetry property value for conversation name.
        /// </summary>
        public static readonly string ConversationNameProperty = "conversationName";

        /// <summary>
        /// The telemetry property value for dialog id.
        /// </summary>
        public static readonly string DialogIdProperty = "dialogId";

        /// <summary>
        /// The telemetry property value for from id.
        /// </summary>
        public static readonly string FromIdProperty = "fromId";

        /// <summary>
        /// The telemetry property value for from name.
        /// </summary>
        public static readonly string FromNameProperty = "fromName";

        /// <summary>
        /// The telemetry property value for locale.
        /// </summary>
        public static readonly string LocaleProperty = "locale";

        /// <summary>
        /// The telemetry property value for recipient id.
        /// </summary>
        public static readonly string RecipientIdProperty = "recipientId";

        /// <summary>
        /// The telemetry property value for recipient name.
        /// </summary>
        public static readonly string RecipientNameProperty = "recipientName";

        /// <summary>
        /// The telemetry property value for reply activity id.
        /// </summary>
        public static readonly string ReplyActivityIDProperty = "replyActivityId";

        /// <summary>
        /// The telemetry property value for text.
        /// </summary>
        public static readonly string TextProperty = "text";

        /// <summary>
        /// The telemetry property value for speak.
        /// </summary>
        public static readonly string SpeakProperty = "speak";

        /// <summary>
        /// The telemetry property value for user id.
        /// </summary>
        public static readonly string UserIdProperty = "userId";

        /// <summary>
        /// The telemetry property value for attachments.
        /// </summary>
        public static readonly string AttachmentsProperty = "attachments";
    }
}
