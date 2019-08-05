// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Xml.Serialization;

namespace Microsoft.Bot.Builder.Adapters.WeChat.Schema.Requests.Events
{
    public class SendPicsInfo
    {
        [XmlElement(ElementName = "Count")]
        public int Count { get; set; }

        [XmlElement(ElementName = "PicList")]
        public List<PicItem> PicList { get; set; }
    }
}
