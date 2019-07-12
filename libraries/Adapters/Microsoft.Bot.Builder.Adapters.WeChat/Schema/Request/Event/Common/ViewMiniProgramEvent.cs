using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace Microsoft.Bot.Builder.Adapters.WeChat.Schema.Request.Event.Common
{
    [XmlRoot("xml")]
    public class ViewMiniProgramEvent : RequestEventWithEventKey
    {
        public override string Event
        {
            get { return EventType.ViewMiniProgram; }
        }

        [XmlElement(ElementName = "MenuId")]
        public string MenuId { get; set; }
    }
}
