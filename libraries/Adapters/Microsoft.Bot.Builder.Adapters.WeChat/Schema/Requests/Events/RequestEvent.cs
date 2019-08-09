// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Xml.Serialization;

namespace Microsoft.Bot.Builder.Adapters.WeChat.Schema.Requests.Events
{
    public abstract class RequestEvent : IRequestMessageEventBase
    {
        /// <summary>
        /// Gets the event type.
        /// </summary>
        /// <value>
        /// The event type, should be one of EventType.
        /// </value>
        [XmlElement(ElementName = "Event")]
        public virtual string EventType { get; }

        [XmlElement(ElementName = "Encrypt")]
        public string Encrypt { get; set; }

        /// <summary>
        /// Gets event message type.
        /// </summary>
        /// <value>
        /// Event message type, should be a static value.
        /// </value>
        public string MsgType => RequestMessageTypes.Event;

        /// <summary>
        /// Gets or sets ToUserName.
        /// </summary>
        /// <value>
        /// Recipient openId.
        /// </value>
        [XmlElement(ElementName = "ToUserName")]
        public string ToUserName { get; set; }

        /// <summary>
        /// Gets or sets FromUserName.
        /// </summary>
        /// <value>
        /// Sender openId.
        /// </value>
        [XmlElement(ElementName = "FromUserName")]
        public string FromUserName { get; set; }

        /// <summary>
        /// Gets or sets CreateTime.
        /// </summary>
        /// <value>
        /// Message creation time.
        /// </value>
        [XmlElement(ElementName = "CreateTime")]
        public long CreateTime { get; set; }
    }
}
