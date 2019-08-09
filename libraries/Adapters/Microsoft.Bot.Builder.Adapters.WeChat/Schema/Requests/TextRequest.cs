// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Xml.Serialization;

namespace Microsoft.Bot.Builder.Adapters.WeChat.Schema.Requests
{
    [XmlRoot("xml")]
    public class TextRequest : RequestMessage
    {
        public override string MsgType => RequestMessageTypes.Text;

        [XmlElement(ElementName = "Content")]
        public string Content { get; set; }

        // public string Bizmsgmenuid { get; set; }
    }
}
