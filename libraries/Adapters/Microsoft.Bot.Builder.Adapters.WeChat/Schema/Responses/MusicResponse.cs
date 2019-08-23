// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Xml;
using System.Xml.Serialization;

namespace Microsoft.Bot.Builder.Adapters.WeChat.Schema.Responses
{
    [XmlRoot("xml")]
    public class MusicResponse : ResponseMessage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MusicResponse"/> class.
        /// </summary>
        public MusicResponse()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MusicResponse"/> class.
        /// </summary>
        /// <param name="senderId">The sender's id.</param>
        /// <param name="recipientId">The recipient id.</param>
        /// <param name="music">The <see cref="Music"/> of the music resposne.</param>
        public MusicResponse(string senderId, string recipientId, Music music)
            : base(senderId, recipientId)
        {
            Music = music;
        }

        [XmlIgnore]
        public override string MsgType => ResponseMessageTypes.Music;

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

        [XmlElement(ElementName = "Music")]
        public Music Music { get; set; }
    }
}
