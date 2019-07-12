using System.Xml.Serialization;

namespace Microsoft.Bot.Builder.Adapters.WeChat.Schema.Request.Event.Common
{
    /// <summary>
    /// Url jump to view.
    /// </summary>
    [XmlRoot("xml")]
    public class ViewEvent : RequestEventWithEventKey
    {
        /// <summary>
        /// Gets event, EventType: VIEW.
        /// </summary>
        /// <value>
        /// EventType: VIEW.
        /// </value>
        public override string Event
        {
            get { return EventType.View; }
        }
    }
}
