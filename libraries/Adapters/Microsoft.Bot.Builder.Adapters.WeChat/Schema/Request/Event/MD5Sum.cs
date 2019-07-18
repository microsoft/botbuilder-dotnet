using System.Xml.Serialization;

namespace Microsoft.Bot.Builder.Adapters.WeChat.Schema.Request.Event
{
    public class MD5Sum
    {
        [XmlElement(ElementName = "PicMd5Sum")]
        public string PicMD5Sum { get; set; }
    }
}
