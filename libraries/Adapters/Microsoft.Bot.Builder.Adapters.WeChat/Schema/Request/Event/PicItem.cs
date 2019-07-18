using System.Xml.Serialization;

namespace Microsoft.Bot.Builder.Adapters.WeChat.Schema.Request.Event
{
    public class PicItem
    {
        [XmlElement(ElementName = "item")]
        public MD5Sum Item { get; set; }
    }
}
