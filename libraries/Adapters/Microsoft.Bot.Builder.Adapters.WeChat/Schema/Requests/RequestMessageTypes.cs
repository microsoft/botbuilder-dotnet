// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Builder.Adapters.WeChat.Schema.Requests
{
    /// <summary>
    /// Request Message types.
    /// </summary>
    public static class RequestMessageTypes
    {
        /// <summary>
        /// Request message type: Unknown.
        /// </summary>
        public const string Unknown = "unknown";

        /// <summary>
        /// Request message type: Text.
        /// </summary>
        public const string Text = "text";

        /// <summary>
        /// Request message type: Location.
        /// </summary>
        public const string Location = "location";

        /// <summary>
        /// Request message type: Image.
        /// </summary>
        public const string Image = "image";

        /// <summary>
        /// Request message type:  Voice.
        /// </summary>
        public const string Voice = "voice";

        /// <summary>
        /// Request message type: Video.
        /// </summary>
        public const string Video = "video";

        /// <summary>
        /// Request message type: Link.
        /// </summary>
        public const string Link = "link";

        /// <summary>
        /// Request message type: ShortVideo.
        /// </summary>
        public const string ShortVideo = "shortvideo";

        /// <summary>
        /// Request message type: Event.
        /// </summary>
        public const string Event = "event";

        /// <summary>
        /// Request message type: File.
        /// </summary>
        public const string File = "file";
    }
}
