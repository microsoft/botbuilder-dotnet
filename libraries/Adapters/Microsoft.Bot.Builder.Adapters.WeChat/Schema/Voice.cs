// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Xml;
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
    }
}
