using System.Xml.Serialization;

namespace Microsoft.Bot.Builder.Adapters.WeChat.Schema.Request.Event.Common
{
    [XmlRoot("xml")]
    public class TemplateSendFinishedEvent : RequestEvent
    {
        public override string Event
        {
            get { return EventType.TemplateSendFinished; }
        }

        [XmlElement(ElementName = "Status")]
        public string Status { get; set; }

        [XmlElement(ElementName = "MsgID")]
        public long MsgID { get; set; }
    }
}
