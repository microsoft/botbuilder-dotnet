using System.Xml.Serialization;

namespace Microsoft.Bot.Builder.Adapters.WeChat.Schema
{
    [XmlRoot("item")]
    public class Article
    {
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

        [XmlIgnore]
        public string Url { get; set; }

        [XmlElement(ElementName = "Url")]
        public System.Xml.XmlCDataSection UrlCDATA
        {
            get
            {
                return new System.Xml.XmlDocument().CreateCDataSection(Url);
            }

            set
            {
                Url = value.Value;
            }
        }

        /// <summary>
        /// Gets or sets PicUrl.
        /// </summary>
        /// <value>
        /// Should be JPG or PNG type.
        /// </value>
        [XmlIgnore]
        public string PicUrl { get; set; }

        [XmlElement(ElementName = "PicUrl")]
        public System.Xml.XmlCDataSection PicUrlCDATA
        {
            get
            {
                return new System.Xml.XmlDocument().CreateCDataSection(PicUrl);
            }

            set
            {
                PicUrl = value.Value;
            }
        }
    }
}
