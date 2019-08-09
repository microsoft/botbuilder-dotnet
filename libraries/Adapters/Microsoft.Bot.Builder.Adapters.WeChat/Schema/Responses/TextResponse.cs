// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Xml;
using System.Xml.Serialization;

namespace Microsoft.Bot.Builder.Adapters.WeChat.Schema.Responses
{
    [XmlRoot("xml")]
    public class TextResponse : ResponseMessage
    {
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
