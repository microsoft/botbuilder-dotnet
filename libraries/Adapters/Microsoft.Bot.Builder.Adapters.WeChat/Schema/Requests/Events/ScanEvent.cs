// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Xml.Serialization;

namespace Microsoft.Bot.Builder.Adapters.WeChat.Schema.Requests.Events
{
    /// <summary>
    /// Scan QR code.
    /// </summary>
    [XmlRoot("xml")]
    public class ScanEvent : RequestEventWithEventKey
    {
        /// <summary>
        /// Gets event, EventType: scan.
        /// </summary>
        /// <value>
        /// EventType: scan.
        /// </value>
        public override string EventType => EventTypes.Scan;

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
