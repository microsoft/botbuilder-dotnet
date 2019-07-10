using System.Xml.Serialization;

namespace Microsoft.Bot.Builder.Adapters.WeChat.Schema.Request
{
    [XmlRoot("xml")]
    public class VoiceRequest : RequestMessage
    {
        public override RequestMessageType MsgType => RequestMessageType.Voice;

        [XmlElement(ElementName = "MediaId")]
        public string MediaId { get; set; }

        [XmlElement(ElementName = "Format")]
        public string Format { get; set; }

        [XmlElement(ElementName = "Recognition")]
        public string Recognition { get; set; }
    }
}
