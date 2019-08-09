// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Xml.Serialization;

namespace Microsoft.Bot.Builder.Adapters.WeChat.Schema.Requests.Events
{
    /// <summary>
    /// Click.
    /// </summary>
    [XmlRoot("xml")]
    public class ClickEvent : RequestEventWithEventKey
    {
        /// <summary>
        /// Gets Event, eventType: CLICK.
        /// </summary>
        /// <value>
        /// Event type click.
        /// </value>
        public override string EventType => EventTypes.Click;
    }
}
