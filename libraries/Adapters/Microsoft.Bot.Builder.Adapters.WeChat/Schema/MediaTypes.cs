// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Builder.Adapters.WeChat.Schema
{
    public static class MediaTypes
    {
        /// <summary>
        /// Image: 2M, support PNG/JEPG/JPG/GIF.
        /// </summary>
        public const string Image = "image";

        /// <summary>
        /// Voice: 2M, no longer than 60s, support AMR/MP3.
        /// </summary>
        public const string Voice = "voice";

        /// <summary>
        /// Video: 10M, support MP4.
        /// </summary>
        public const string Video = "video";

        /// <summary>
        /// General audio type.
        /// </summary>
        public const string Audio = "audio";

        /// <summary>
        /// Thumb：64KB，support JPG.
        /// </summary>
        public const string Thumb = "thumb";

        /// <summary>
        /// News type.
        /// </summary>
        public const string News = "news";
    }
}
