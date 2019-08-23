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
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VoiceResponse"/> class.
        /// </summary>
        /// <param name="senderId">The sender's id.</param>
        /// <param name="recipientId">The recipient id.</param>
        /// <param name="mediaId">The media id of the voice.</param>
        public VoiceResponse(string senderId, string recipientId, string mediaId)
            : base(senderId, recipientId)
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
