// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Xml;
using System.Xml.Serialization;

namespace Microsoft.Bot.Builder.Adapters.WeChat.Schema.Responses
{
    [XmlRoot("xml")]
    public class TextResponse : ResponseMessage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TextResponse"/> class.
        /// </summary>
        public TextResponse()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TextResponse"/> class.
        /// </summary>
        /// <param name="senderId">The sender's id.</param>
        /// <param name="recipientId">The recipient id.</param>
        /// <param name="content">The content of the text response.</param>
        public TextResponse(string senderId, string recipientId, string content)
            : base(senderId, recipientId)
        {
            Content = content;
        }

        [XmlIgnore]
        public override string MsgType => ResponseMessageTypes.Text;

        [XmlElement(ElementName = "MsgType")]
        public XmlCDataSection MsgTypeCData
        {
            get
            {
                return new XmlDocument().CreateCDataSection(MsgType);
            }

            set
            {
                MsgType = value.Value;
            }
        }

        [XmlIgnore]
        public string Content { get; set; }

        [XmlElement(ElementName = "Content")]
        public XmlCDataSection ContentCData
        {
            get
            {
                return new XmlDocument().CreateCDataSection(Content);
            }

            set
            {
                Content = value.Value;
            }
        }
    }
}
