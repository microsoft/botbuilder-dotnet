namespace Microsoft.Bot.Builder.Adapters.WeChat.Schema.Request
{
    /// <summary>
    /// Request Message types.
    /// </summary>
    public enum RequestMessageType
    {
        /// <summary>
        /// Unknown
        /// </summary>
        Unknown = -1,

        /// <summary>
        /// Text
        /// </summary>
        Text = 0,

        /// <summary>
        /// Location
        /// </summary>
        Location = 1,

        /// <summary>
        /// Image
        /// </summary>
        Image = 2,

        /// <summary>
        /// Voice
        /// </summary>
        Voice = 3,

        /// <summary>
        /// Video
        /// </summary>
        Video = 4,

        /// <summary>
        /// Link
        /// </summary>
        Link = 5,

        /// <summary>
        /// ShortVideo
        /// </summary>
        ShortVideo = 6,

        /// <summary>
        /// Event
        /// </summary>
        Event = 7,

        /// <summary>
        /// File
        /// </summary>
        File = 8,
    }

    public interface IRequestMessageBase
    {
        /// <summary>
        /// Gets MsgType.
        /// </summary>
        /// <value>
        /// Message type of the request.
        /// </value>
        RequestMessageType MsgType { get; }

        string Encrypt { get; set; }

        /// <summary>
        /// Gets or sets ToUserName.
        /// </summary>
        /// <value>
        /// Recipient OpenId from WeChat.
        /// </value>
        string ToUserName { get; set; }

        /// <summary>
        /// Gets or sets FromUserName.
        /// </summary>
        /// <value>
        /// Sender OpenId from WeChat.
        /// </value>
        string FromUserName { get; set; }

        /// <summary>
        /// Gets or sets CreateTime.
        /// </summary>
        /// <value>
        /// Message Created time.
        /// </value>
        long CreateTime { get; set; }
    }
}
