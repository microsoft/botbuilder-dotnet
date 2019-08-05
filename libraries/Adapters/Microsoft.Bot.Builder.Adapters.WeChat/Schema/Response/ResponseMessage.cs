// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Xml.Serialization;

namespace Microsoft.Bot.Builder.Adapters.WeChat.Schema.Response
{
    public abstract class ResponseMessage : IResponseMessageBase
    {
        /// <summary>
        /// Gets or sets ToUserName.
        /// </summary>
        /// <value>
        /// Recipient openId.
        /// </value>
        [XmlIgnore]
        public string ToUserName { get; set; }

        [XmlElement(ElementName = "ToUserName")]
        public System.Xml.XmlCDataSection ToUserNameCDATA
        {
            get
            {
                return new System.Xml.XmlDocument().CreateCDataSection(ToUserName);
            }

            set
            {
                ToUserName = value.Value;
            }
        }

        /// <summary>
        /// Gets or sets FromUserName.
        /// </summary>
        /// <value>
        /// Sender openId.
        /// </value>
        [XmlIgnore]
        public string FromUserName { get; set; }

        [XmlElement(ElementName = "FromUserName")]
        public System.Xml.XmlCDataSection FromUserNameCDATA
        {
            get
            {
                return new System.Xml.XmlDocument().CreateCDataSection(FromUserName);
            }

            set
            {
                FromUserName = value.Value;
            }
        }

        /// <summary>
        /// Gets or sets creation time.
        /// </summary>
        /// <value>
        /// Message creation time.
        /// </value>
        [XmlElement(ElementName = "CreateTime")]
        public long CreateTime { get; set; }

        /// <summary>
        /// Gets or sets message type.
        /// </summary>
        /// /// <value>
        /// Response message type.
        /// </value>
        [XmlIgnore]
        public virtual string MsgType { get; set; }
    }
}
