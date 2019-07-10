using System.Xml.Serialization;

namespace Microsoft.Bot.Builder.Adapters.WeChat.Schema.Request
{
    [XmlRoot("xml")]
    public class ShortVideoRequest : RequestMessage
    {
        public override RequestMessageType MsgType => RequestMessageType.ShortVideo;

        [XmlElement(ElementName = "MediaId")]
        public string MediaId { get; set; }

        [XmlElement(ElementName = "ThumbMediaId")]
        public string ThumbMediaId { get; set; }
    }
}
