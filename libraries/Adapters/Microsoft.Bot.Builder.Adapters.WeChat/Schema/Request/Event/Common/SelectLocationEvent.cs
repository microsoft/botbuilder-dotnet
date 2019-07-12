using System.Xml.Serialization;

namespace Microsoft.Bot.Builder.Adapters.WeChat.Schema.Request.Event.Common
{
    [XmlRoot("xml")]
    public class SelectLocationEvent : RequestEventWithEventKey
    {
        public override string Event
        {
            get { return EventType.SelectLocation; }
        }

        [XmlElement(ElementName = "SendLocationInfo")]
        public SendLocationInfo SendLocationInfo { get; set; }
    }
}
