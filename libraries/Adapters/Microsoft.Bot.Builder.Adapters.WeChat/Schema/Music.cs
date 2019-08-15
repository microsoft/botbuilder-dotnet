// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Xml;
using System.Xml.Serialization;

namespace Microsoft.Bot.Builder.Adapters.WeChat.Schema
{
    [XmlRoot("Music")]
    public class Music
    {
        /// <summary>
        /// Gets or sets Title.
        /// </summary>
        /// <value>
        /// Title of music.
        /// </value>
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

        /// <summary>
        /// Gets or sets Description.
        /// </summary>
        /// <value>
        /// Description of music.
        /// </value>
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

        /// <summary>
        /// Gets or sets MusicUrl.
        /// </summary>
        /// <value>
        /// Url of music.
        /// </value>
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

        /// <summary>
        /// Gets or sets HQMusicUrl.
        /// </summary>
        /// <value>
        /// High quality music Url.
        /// </value>
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

        /// <summary>
        /// Gets or sets ThumbMediaId.
        /// </summary>
        /// <value>
        /// Thumbnail image id of the music.
        /// </value>
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
