using System.Xml.Serialization;

namespace Microsoft.Bot.Builder.Adapters.WeChat.Schema.Request
{
    [XmlRoot("xml")]
    public class TextRequest : RequestMessage
    {
        public override RequestMessageType MsgType => RequestMessageType.Text;

        [XmlElement(ElementName = "Content")]
        public string Content { get; set; }

        // public string Bizmsgmenuid { get; set; }
    }
}
