// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Xml.Serialization;

namespace Microsoft.Bot.Builder.Adapters.WeChat.Schema.Requests.Events
{
    /// <summary>
    /// Url jump to view.
    /// </summary>
    [XmlRoot("xml")]
    public class ViewEvent : RequestEventWithEventKey
    {
        /// <summary>
        /// Gets event, EventType: VIEW.
        /// </summary>
        /// <value>
        /// EventType: VIEW.
        /// </value>
        public override string EventType => EventTypes.View;
    }
}
