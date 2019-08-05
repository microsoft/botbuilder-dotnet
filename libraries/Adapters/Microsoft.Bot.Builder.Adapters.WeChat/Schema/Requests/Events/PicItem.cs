// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Xml.Serialization;

namespace Microsoft.Bot.Builder.Adapters.WeChat.Schema.Requests.Events
{
    public class PicItem
    {
        [XmlElement(ElementName = "item")]
        public MD5Sum Item { get; set; }
    }
}
