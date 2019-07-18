using System.Xml.Serialization;

namespace Microsoft.Bot.Builder.Adapters.WeChat.Schema.Request.Event
{
    public class ScanCodeInfo
    {
        [XmlElement(ElementName = "ScanType")]
        public string ScanType { get; set; }

        [XmlElement(ElementName = "ScanResult")]
        public string ScanResult { get; set; }
    }
}
