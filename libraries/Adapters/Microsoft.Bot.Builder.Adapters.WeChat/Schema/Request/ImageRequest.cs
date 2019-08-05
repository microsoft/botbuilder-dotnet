// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Xml.Serialization;

namespace Microsoft.Bot.Builder.Adapters.WeChat.Schema.Request
{
    [XmlRoot("xml")]
    public class ImageRequest : RequestMessage
    {
        public override RequestMessageType MsgType => RequestMessageType.Image;

        /// <summary>
        /// Gets or sets MediaId.
        /// </summary>
        /// <value>
        /// Media id of the image.
        /// </value>
        [XmlElement(ElementName = "MediaId")]
        public string MediaId { get; set; }

        /// <summary>
        /// Gets or sets PicUrl.
        /// </summary>
        /// <value>
        /// Image's link.
        /// </value>
        [XmlElement(ElementName = "PicUrl")]
        public string PicUrl { get; set; }
    }
}
