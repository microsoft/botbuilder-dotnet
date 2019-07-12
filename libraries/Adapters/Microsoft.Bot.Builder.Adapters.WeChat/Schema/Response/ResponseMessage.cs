using System.Xml.Serialization;

namespace Microsoft.Bot.Builder.Adapters.WeChat.Schema.Response
{
    public abstract class ResponseMessage : IResponseMessageBase
    {
        /// <summary>
        /// Gets or sets ToUserName.
        /// </summary>
        /// <value>
        /// Recipient openId.
        /// </value>
        [XmlElement(ElementName = "ToUserName")]
        public string ToUserName { get; set; }

        /// <summary>
        /// Gets or sets FromUserName.
        /// </summary>
        /// <value>
        /// Sender openId.
        /// </value>
        [XmlElement(ElementName = "FromUserName")]
        public string FromUserName { get; set; }

        /// <summary>
        /// Gets or sets creation time.
        /// </summary>
        /// <value>
        /// Message creation time.
        /// </value>
        [XmlElement(ElementName = "CreateTime")]
        public long CreateTime { get; set; }

        public virtual ResponseMessageType MsgType { get; }
    }
}
