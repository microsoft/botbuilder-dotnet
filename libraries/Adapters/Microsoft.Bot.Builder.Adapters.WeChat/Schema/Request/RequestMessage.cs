using System.Xml.Serialization;

namespace Microsoft.Bot.Builder.Adapters.WeChat.Schema.Request
{
    /// <summary>
    /// Base class of the request from wechat.
    /// TODO: may need to be an abstract class, force user to implement MsgType.
    /// </summary>
    [XmlRoot("xml")]
    public abstract class RequestMessage : IRequestMessageBase
    {
        /// <summary>
        /// Message id, required except event message.
        /// </summary>
        [XmlElement(ElementName = "MsgId")]
        public long MsgId { get; set; }

        [XmlElement(ElementName = "Encrypt")]
        public string Encrypt { get; set; }

        /// <summary>
        /// Message type of the request message, override it if needed.
        /// </summary>
        [XmlElement(ElementName = "MsgType")]
        public abstract RequestMessageType MsgType { get; }

        /// <summary>
        /// Recipient openId.
        /// </summary>
        [XmlElement(ElementName = "ToUserName")]
        public string ToUserName { get; set; }

        /// <summary>
        /// Sender openId.
        /// </summary>
        [XmlElement(ElementName = "FromUserName")]
        public string FromUserName { get; set; }

        /// <summary>
        /// Message creation time.
        /// </summary>
        [XmlElement(ElementName = "CreateTime")]
        public long CreateTime { get; set; }
    }
}
