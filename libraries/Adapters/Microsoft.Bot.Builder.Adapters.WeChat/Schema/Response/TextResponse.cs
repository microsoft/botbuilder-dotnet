using System.Xml.Serialization;

namespace Microsoft.Bot.Builder.Adapters.WeChat.Schema.Response
{
    [XmlRoot("xml")]
    public class TextResponse : ResponseMessage
    {
        [XmlIgnore]
        public override string MsgType => ResponseMessageType.Text;

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

        [XmlIgnore]
        public string Content { get; set; }

        [XmlElement(ElementName = "Content")]
        public System.Xml.XmlCDataSection ContentCDATA
        {
            get
            {
                return new System.Xml.XmlDocument().CreateCDataSection(Content);
            }

            set
            {
                Content = value.Value;
            }
        }
    }
}
