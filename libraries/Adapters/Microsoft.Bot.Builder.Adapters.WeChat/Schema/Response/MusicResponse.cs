using System.Xml.Serialization;

namespace Microsoft.Bot.Builder.Adapters.WeChat.Schema.Response
{
    [XmlRoot("xml")]
    public class MusicResponse : ResponseMessage
    {
        public MusicResponse()
        {
        }

        public MusicResponse(Music music)
        {
            Music = music;
        }

        [XmlIgnore]
        public override string MsgType => ResponseMessageType.Music;

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

        [XmlElement(ElementName = "Music")]
        public Music Music { get; set; }
    }
}
