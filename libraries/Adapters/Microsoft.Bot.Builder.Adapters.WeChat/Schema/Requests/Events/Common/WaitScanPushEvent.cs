// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Xml.Serialization;

namespace Microsoft.Bot.Builder.Adapters.WeChat.Schema.Requests.Events.Common
{
    [XmlRoot("xml")]
    public class WaitScanPushEvent : RequestEventWithEventKey
    {
        public override string Event
        {
            get { return EventType.WaitScanPush; }
        }

        [XmlElement(ElementName = "ScanCodeInfo")]
        public ScanCodeInfo ScanCodeInfo { get; set; }
    }
}
