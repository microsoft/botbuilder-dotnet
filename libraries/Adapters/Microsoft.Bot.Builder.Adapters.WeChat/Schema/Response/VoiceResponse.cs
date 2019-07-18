using System.Xml.Serialization;

namespace Microsoft.Bot.Builder.Adapters.WeChat.Schema.Response
{
    [XmlRoot("xml")]
    public class VoiceResponse : ResponseMessage
    {
        public VoiceResponse()
        {
        }

        public VoiceResponse(Voice voice)
        {
            Voice = voice;
        }

        public VoiceResponse(string mediaId)
        {
            Voice = new Voice(mediaId);
        }

        [XmlIgnore]
        public override string MsgType => ResponseMessageType.Voice;

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

        [XmlElement(ElementName = "Voice")]
        public Voice Voice { get; set; }
    }
}
