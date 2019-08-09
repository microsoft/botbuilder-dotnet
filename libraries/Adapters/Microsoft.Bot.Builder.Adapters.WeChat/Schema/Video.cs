// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Xml;
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
        public XmlCDataSection MediaIdCData
        {
            get
            {
                return new XmlDocument().CreateCDataSection(MediaId);
            }

            set
            {
                MediaId = value.Value;
            }
        }

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
    }
}
