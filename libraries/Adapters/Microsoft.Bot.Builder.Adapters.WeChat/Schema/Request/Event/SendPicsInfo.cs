using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace Microsoft.Bot.Builder.Adapters.WeChat.Schema.Request.Event
{
    public class SendPicsInfo
    {
        [XmlElement(ElementName = "Count")]
        public int Count { get; set; }

        [XmlElement(ElementName = "PicList")]
        public List<PicItem> PicList { get; set; }
    }
}
