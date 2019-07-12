namespace Microsoft.Bot.Builder.Adapters.WeChat.Schema
{
    public class UploadMediaType
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
        /// Thumb：64KB，support JPG.
        /// </summary>
        public const string Thumb = "thumb";

        /// <summary>
        /// News type.
        /// </summary>
        public const string News = "news";
    }
}
