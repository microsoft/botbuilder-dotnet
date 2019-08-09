// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Xml;
using System.Xml.Serialization;

namespace Microsoft.Bot.Builder.Adapters.WeChat.Schema.Responses
{
    [XmlRoot("xml")]
    public class VoiceResponse : ResponseMessage
    {
        public VoiceResponse()
        {
        }

        public VoiceResponse(Voice voice)
        {
            Voice = voice;
        }

        public VoiceResponse(string mediaId)
        {
            Voice = new Voice(mediaId);
        }

        [XmlIgnore]
        public override string MsgType => ResponseMessageTypes.Voice;

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

        [XmlElement(ElementName = "Voice")]
        public Voice Voice { get; set; }
    }
}
