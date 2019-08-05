// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Xml.Serialization;

namespace Microsoft.Bot.Builder.Adapters.WeChat.Schema.Requests.Events.Common
{
    [XmlRoot("xml")]
    public class WeChatAlbumEvent : RequestEventWithEventKey
    {
        public override string Event => EventType.WeChatAlbum;

        [XmlElement(ElementName = "SendPicsInfo")]
        public SendPicsInfo SendPicsInfo { get; set; }
    }
}
