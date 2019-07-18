using System.Xml.Serialization;

namespace Microsoft.Bot.Builder.Adapters.WeChat.Schema
{
    [XmlRoot("Video")]
    public class Video
    {
        public Video()
        {
        }

        public Video(string mediaId, string title = null, string description = null)
        {
            MediaId = mediaId;
            Title = title;
            Description = description;
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

        [XmlIgnore]
        public string Title { get; set; }

        [XmlElement(ElementName = "Title")]
        public System.Xml.XmlCDataSection TitleCDATA
        {
            get
            {
                return new System.Xml.XmlDocument().CreateCDataSection(Title);
            }

            set
            {
                Title = value.Value;
            }
        }

        [XmlIgnore]
        public string Description { get; set; }

        [XmlElement(ElementName = "Description")]
        public System.Xml.XmlCDataSection DescriptionCDATA
        {
            get
            {
                return new System.Xml.XmlDocument().CreateCDataSection(Description);
            }

            set
            {
                Description = value.Value;
            }
        }
    }
}
