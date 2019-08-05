// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Xml.Serialization;

namespace Microsoft.Bot.Builder.Adapters.WeChat.Schema.Responses
{
    [XmlRoot("xml")]
    public class ImageResponse : ResponseMessage
    {
        public ImageResponse()
        {
        }

        public ImageResponse(Image image)
        {
            Image = image;
        }

        public ImageResponse(string mediaId)
        {
            Image = new Image(mediaId);
        }

        [XmlIgnore]
        public override string MsgType => ResponseMessageType.Image;

        [XmlElement(ElementName = "MsgType")]
        public System.Xml.XmlCDataSection MsgTypeCData
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

        [XmlElement(ElementName = "Image")]
        public Image Image { get; set; }
    }
}
