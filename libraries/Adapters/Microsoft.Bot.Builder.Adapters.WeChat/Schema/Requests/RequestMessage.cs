// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Xml.Serialization;

namespace Microsoft.Bot.Builder.Adapters.WeChat.Schema.Requests
{
    /// <summary>
    /// Base class of the request from wechat.
    /// TODO: may need to be an abstract class, force user to implement MsgType.
    /// </summary>
    [XmlRoot("xml")]
    public abstract class RequestMessage : IRequestMessageBase
    {
        /// <summary>
        /// Gets or sets MsgId.
        /// </summary>
        /// <value>
        /// Message id, required except event message.
        /// </value>
        [XmlElement(ElementName = "MsgId")]
        public long MsgId { get; set; }

        [XmlElement(ElementName = "Encrypt")]
        public string Encrypt { get; set; }

        /// <summary>
        /// Gets MsgType.
        /// </summary>
        /// <value>
        /// Message type of the request message, override it if needed.
        /// </value>
        [XmlElement(ElementName = "MsgType")]
        public abstract string MsgType { get; }

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
