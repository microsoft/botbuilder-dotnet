using System.Collections.Generic;
using System.Xml.Serialization;

namespace Microsoft.Bot.Builder.Adapters.WeChat.Schema.Response
{
    [XmlRoot("xml")]
    public class NewsResponse : ResponseMessage
    {
        [XmlIgnore]
        public override string MsgType => ResponseMessageType.News;

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

        [XmlElement(ElementName = "ArticleCount")]
        public int ArticleCount
        {
            get
            {
                return Articles == null ? 0 : Articles.Count;
            }

            set
            {
            }
        }

        /// <summary>
        /// Gets or sets Articles.
        /// </summary>
        /// <value>
        /// Article list, can only show up to 10 article.
        /// </value>
        [XmlElement(ElementName = "item")]
        public List<Article> Articles { get; set; }
    }
}
