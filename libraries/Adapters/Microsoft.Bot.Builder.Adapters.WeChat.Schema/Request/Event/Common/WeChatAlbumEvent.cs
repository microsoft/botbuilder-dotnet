using System.Xml.Serialization;

namespace Microsoft.Bot.Builder.Adapters.WeChat.Schema.Request.Event.Common
{
    [XmlRoot("xml")]
    public class WeChatAlbumEvent : RequestEventWithEventKey
    {
        public override string Event
        {
            get { return EventType.WeChatAlbum; }
        }

        [XmlElement(ElementName = "SendPicsInfo")]
        public SendPicsInfo SendPicsInfo { get; set; }
    }
}
