// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Xml;
using System.Xml.Serialization;

namespace Microsoft.Bot.Builder.Adapters.WeChat.Schema
{
    [XmlRoot("item")]
    public class Article
    {
        [XmlIgnore]
        public string Title { get; set; }

        [XmlElement(ElementName = "Title")]
        public XmlCDataSection TitleCData
        {
            get
            {
                return new XmlDocument().CreateCDataSection(Title);
            }

            set
            {
                Title = value.Value;
            }
        }

        [XmlIgnore]
        public string Description { get; set; }

        [XmlElement(ElementName = "Description")]
        public XmlCDataSection DescriptionCData
        {
            get
            {
                return new XmlDocument().CreateCDataSection(Description);
            }

            set
            {
                Description = value.Value;
            }
        }

        [XmlIgnore]
        public string Url { get; set; }

        [XmlElement(ElementName = "Url")]
        public XmlCDataSection UrlCData
        {
            get
            {
                return new XmlDocument().CreateCDataSection(Url);
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
        public XmlCDataSection PicUrlCData
        {
            get
            {
                return new XmlDocument().CreateCDataSection(PicUrl);
            }

            set
            {
                PicUrl = value.Value;
            }
        }
    }
}
