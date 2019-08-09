// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Xml.Serialization;

namespace Microsoft.Bot.Builder.Adapters.WeChat.Schema.Requests.Events
{
    /// <summary>
    /// Subscribe.
    /// </summary>
    [XmlRoot("xml")]
    public class SubscribeEvent : RequestEventWithEventKey
    {
        /// <summary>
        /// Gets event, EventType: subscribe.
        /// </summary>
        /// <value>
        /// EventType: subscribe.
        /// </value>
        public override string EventType => EventTypes.Subscribe;

        /// <summary>
        /// Gets or sets Ticket.
        /// </summary>
        /// <value>
        /// Use to get QR code picture.
        /// </value>
        [XmlElement(ElementName = "Ticket")]
        public string Ticket { get; set; }
    }
}
