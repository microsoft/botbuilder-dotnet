// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Xml.Serialization;

namespace Microsoft.Bot.Builder.Adapters.WeChat.Schema.Requests.Events
{
    [XmlRoot("ResultList")]
    public class ResultList
    {
        [XmlElement(ElementName = "item")]
        public List<ResultListItem> Items { get; set; }
    }
}
