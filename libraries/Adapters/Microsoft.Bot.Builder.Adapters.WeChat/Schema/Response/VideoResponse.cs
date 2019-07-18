using System.Xml.Serialization;

namespace Microsoft.Bot.Builder.Adapters.WeChat.Schema.Response
{
    [XmlRoot("xml")]
    public class VideoResponse : ResponseMessage
    {
        public VideoResponse()
        {
        }

        public VideoResponse(Video video)
        {
            Video = video;
        }

        public VideoResponse(string mediaId, string title = null, string description = null)
        {
            Video = new Video(mediaId, title, description);
        }

        [XmlIgnore]
        public override string MsgType => ResponseMessageType.Video;

        [XmlElement(ElementName = "MsgType")]
        public System.Xml.XmlCDataSection MsgTypeCDATA
        {
            get
            {
                return new System.Xml.XmlDocument().CreateCDataSection(MsgType);
            }

            set
            {
                MsgType = value.Value;
            }
        }

        [XmlElement(ElementName = "Video")]
        public Video Video { get; set; }
    }
}
