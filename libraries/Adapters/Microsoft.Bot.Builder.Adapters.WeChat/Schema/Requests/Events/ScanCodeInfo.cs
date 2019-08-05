// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Xml.Serialization;

namespace Microsoft.Bot.Builder.Adapters.WeChat.Schema.Requests.Events
{
    public class ScanCodeInfo
    {
        [XmlElement(ElementName = "ScanType")]
        public string ScanType { get; set; }

        [XmlElement(ElementName = "ScanResult")]
        public string ScanResult { get; set; }
    }
}
