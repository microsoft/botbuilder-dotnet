// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Xml;
using System.Xml.Serialization;

namespace Microsoft.Bot.Builder.Adapters.WeChat.Schema.Responses
{
    [XmlRoot("xml")]
    public class VideoResponse : ResponseMessage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VideoResponse"/> class.
        /// </summary>
        public VideoResponse()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VideoResponse"/> class.
        /// </summary>
        /// <param name="senderId">The sender's id.</param>
        /// <param name="recipientId">The recipient id.</param>
        /// <param name="video">The <see cref="Video"/> of the video resposne.</param>
        public VideoResponse(string senderId, string recipientId, Video video)
            : base(senderId, recipientId)
        {
            Video = video;
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
