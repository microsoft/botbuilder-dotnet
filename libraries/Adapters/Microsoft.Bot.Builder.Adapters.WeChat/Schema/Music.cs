// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Xml;
using System.Xml.Serialization;

namespace Microsoft.Bot.Builder.Adapters.WeChat.Schema
{
    [XmlRoot("Music")]
    public class Music
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
        public string MusicUrl { get; set; }

        [XmlElement(ElementName = "MusicUrl")]
        public XmlCDataSection MusicUrlCData
        {
            get
            {
                return new XmlDocument().CreateCDataSection(MusicUrl);
            }

            set
            {
                MusicUrl = value.Value;
            }
        }

        [XmlIgnore]
        public string HQMusicUrl { get; set; }

        [XmlElement(ElementName = "HQMusicUrl")]
        public XmlCDataSection HQMusicUrlCData
        {
            get
            {
                return new XmlDocument().CreateCDataSection(HQMusicUrl);
            }

            set
            {
                HQMusicUrl = value.Value;
            }
        }

        [XmlIgnore]
        public string ThumbMediaId { get; set; }

        [XmlElement(ElementName = "ThumbMediaId")]
        public XmlCDataSection ThumbMediaIdCData
        {
            get
            {
                return new XmlDocument().CreateCDataSection(ThumbMediaId);
            }

            set
            {
                ThumbMediaId = value.Value;
            }
        }
    }
}
