// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Xml.Serialization;

namespace Microsoft.Bot.Builder.Adapters.WeChat.Schema.Requests.Events
{
    [XmlRoot("xml")]
    public class ViewMiniProgramEvent : RequestEventWithEventKey
    {
        public override string EventType => EventTypes.ViewMiniProgram;

        [XmlElement(ElementName = "MenuId")]
        public string MenuId { get; set; }
    }
}
