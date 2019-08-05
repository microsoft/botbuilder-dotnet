// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Xml.Serialization;

namespace Microsoft.Bot.Builder.Adapters.WeChat.Schema.Request.Event.Common
{
    /// <summary>
    /// unsubscribe.
    /// </summary>
    [XmlRoot("xml")]
    public class UnsunscribeEvent : RequestEvent
    {
        /// <summary>
        /// Gets event, EventType: unsubscribe.
        /// </summary>
        /// <value>
        /// EventType: unsubscribe.
        /// </value>
        public override string Event
        {
            get { return EventType.Unsubscribe; }
        }
    }
}
