// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Xml.Serialization;

namespace Microsoft.Bot.Builder.Adapters.WeChat.Schema.Responses
{
    [XmlRoot("xml")]
    public class MusicResponse : ResponseMessage
    {
        public MusicResponse()
        {
        }

        public MusicResponse(Music music)
        {
            Music = music;
        }

        [XmlIgnore]
        public override string MsgType => ResponseMessageType.Music;

        [XmlElement(ElementName = "MsgType")]
        public System.Xml.XmlCDataSection MsgTypeCData
        {
            get
            {
                return new System.Xml.XmlDocument().CreateCDataSection(MsgType);
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
