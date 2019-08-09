// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Xml.Serialization;

namespace Microsoft.Bot.Builder.Adapters.WeChat.Schema.Requests
{
    [XmlRoot("xml")]
    public class VideoRequest : RequestMessage
    {
        public override string MsgType => RequestMessageTypes.Video;

        [XmlElement(ElementName = "MediaId")]
        public string MediaId { get; set; }

        [XmlElement(ElementName = "ThumbMediaId")]
        public string ThumbMediaId { get; set; }
    }
}
