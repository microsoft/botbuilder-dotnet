// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Xml;
using System.Xml.Serialization;

namespace Microsoft.Bot.Builder.Adapters.WeChat.Schema.Responses
{
    [XmlRoot("xml")]
    public class VideoResponse : ResponseMessage
    {
        public VideoResponse()
        {
        }

        public VideoResponse(string mediaId, string title = null, string description = null)
        {
            Video = new Video(mediaId, title, description);
        }

        [XmlIgnore]
        public override string MsgType => ResponseMessageTypes.Video;

        [XmlElement(ElementName = "MsgType")]
        public XmlCDataSection MsgTypeCData
        {
            get
            {
                return new XmlDocument().CreateCDataSection(MsgType);
            }

            set
            {
                MsgType = value.Value;
            }
        }

        [XmlElement(ElementName = "Video")]
        public Video Video { get; set; }
    }
}
