using System.Xml.Serialization;

namespace Microsoft.Bot.Builder.Adapters.WeChat.Schema.Request.Event.Common
{
    [XmlRoot("xml")]
    public class WaitScanPushEvent : RequestEventWithEventKey
    {
        public override string Event
        {
            get { return EventType.WaitScanPush; }
        }

        [XmlElement(ElementName = "ScanCodeInfo")]
        public ScanCodeInfo ScanCodeInfo { get; set; }
    }
}
