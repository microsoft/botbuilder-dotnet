// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Xml.Serialization;

namespace Microsoft.Bot.Builder.Adapters.WeChat.Schema.Requests.Events
{
    public class MD5Sum
    {
        [XmlElement(ElementName = "PicMd5Sum")]
        public string PicMD5Sum { get; set; }
    }
}
