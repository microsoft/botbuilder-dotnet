using System.Xml.Serialization;

namespace Microsoft.Bot.Builder.Adapters.WeChat.Schema
{
    [XmlRoot("Voice")]
    public class Voice
    {
        public Voice()
        {
        }

        public Voice(string mediaId)
        {
            MediaId = mediaId;
        }

        [XmlIgnore]
        public string MediaId { get; set; }

        [XmlElement(ElementName = "MediaId")]
        public System.Xml.XmlCDataSection MediaIdCDATA
        {
            get
            {
                return new System.Xml.XmlDocument().CreateCDataSection(MediaId);
            }

            set
            {
                MediaId = value.Value;
            }
        }
    }
}
