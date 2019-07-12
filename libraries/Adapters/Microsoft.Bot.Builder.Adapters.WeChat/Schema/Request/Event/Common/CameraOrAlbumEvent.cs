using System.Xml.Serialization;

namespace Microsoft.Bot.Builder.Adapters.WeChat.Schema.Request.Event.Common
{
    [XmlRoot("xml")]
    public class CameraOrAlbumEvent : RequestEventWithEventKey
    {
        public override string Event
        {
            get { return EventType.CameraOrAlbum; }
        }

        [XmlElement(ElementName = "SendPicsInfo")]
        public SendPicsInfo SendPicsInfo { get; set; }
    }
}
