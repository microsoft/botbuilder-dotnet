// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Xml.Serialization;

namespace Microsoft.Bot.Builder.Adapters.WeChat.Schema.Requests.Events
{
    /// <summary>
    /// Request Event with event key, most likly comming from static menu event.
    /// </summary>
    public abstract class RequestEventWithEventKey : RequestEvent
    {
        [XmlElement(ElementName = "EventKey")]
        public string EventKey { get; set; }
    }
}
