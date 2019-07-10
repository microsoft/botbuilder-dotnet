namespace Microsoft.Bot.Builder.Adapters.WeChat.Schema.Response
{
    public enum ResponseMessageType
    {
        /// <summary>
        /// Text message.
        /// </summary>
        Text = 0,

        /// <summary>
        /// News message.
        /// </summary>
        News = 1,

        /// <summary>
        /// Music message.
        /// </summary>
        Music = 2,

        /// <summary>
        /// Image message.
        /// </summary>
        Image = 3,

        /// <summary>
        /// Voice message.
        /// </summary>
        Voice = 4,

        /// <summary>
        /// Video message.
        /// </summary>
        Video = 5,

        /// <summary>
        /// Transfer customer service message.
        /// </summary>
        Transfer_Customer_Service = 6,

        /// <summary>
        /// MpNews message.
        /// </summary>
        MpNews = 7,

        /// <summary>
        /// MultipleNews message.
        /// </summary>
        MultipleNews = 106,

        /// <summary>
        /// Location message.
        /// </summary>
        LocationMessage = 107,

        /// <summary>
        /// No responese message.
        /// </summary>
        NoResponse = 110,

        /// <summary>
        /// Success response message.
        /// </summary>
        SuccessResponse = 200,

        /// <summary>
        /// Use api message.
        /// </summary>
        UseApi = 998,

        /// <summary>
        /// Other message.
        /// </summary>
        Other = -2,

        /// <summary>
        /// Unknown message.
        /// </summary>
        Unknown = -1,
    }

    public interface IResponseMessageBase
    {
        ResponseMessageType MsgType { get; }
    }
}
