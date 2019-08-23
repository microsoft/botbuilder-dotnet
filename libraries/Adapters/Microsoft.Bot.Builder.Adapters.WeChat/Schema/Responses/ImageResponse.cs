// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Xml;
using System.Xml.Serialization;

namespace Microsoft.Bot.Builder.Adapters.WeChat.Schema.Responses
{
    [XmlRoot("xml")]
    public class ImageResponse : ResponseMessage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ImageResponse"/> class.
        /// </summary>
        public ImageResponse()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ImageResponse"/> class.
        /// </summary>
        /// <param name="senderId">The sender's id.</param>
        /// <param name="recipientId">The recipient id.</param>
        /// <param name="mediaId">The media id of the image.</param>
        public ImageResponse(string senderId, string recipientId, string mediaId)
            : base(senderId, recipientId)
        {
            Image = new Image(mediaId);
        }

        [XmlIgnore]
        public override string MsgType => ResponseMessageTypes.Image;

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

        [XmlElement(ElementName = "Image")]
        public Image Image { get; set; }
    }
}
