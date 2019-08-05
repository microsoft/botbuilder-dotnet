// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Xml.Serialization;

namespace Microsoft.Bot.Builder.Adapters.WeChat.Schema.Requests.Events.Common
{
    [XmlRoot("xml")]
    public class SelectLocationEvent : RequestEventWithEventKey
    {
        public override string Event => EventType.SelectLocation;

        [XmlElement(ElementName = "SendLocationInfo")]
        public SendLocationInfo SendLocationInfo { get; set; }
    }
}
