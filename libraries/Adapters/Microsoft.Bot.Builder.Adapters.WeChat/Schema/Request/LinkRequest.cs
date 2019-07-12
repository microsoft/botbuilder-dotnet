using System.Xml.Serialization;

namespace Microsoft.Bot.Builder.Adapters.WeChat.Schema.Request
{
    /// <summary>
    /// Link request is used to share some online aritclies.
    /// </summary>
    [XmlRoot("xml")]
    public class LinkRequest : RequestMessage
    {
        public override RequestMessageType MsgType => RequestMessageType.Link;

        [XmlElement(ElementName = "Title")]
        public string Title { get; set; }

        [XmlElement(ElementName = "Description")]
        public string Description { get; set; }

        [XmlElement(ElementName = "Url")]
        public string Url { get; set; }
    }
}
