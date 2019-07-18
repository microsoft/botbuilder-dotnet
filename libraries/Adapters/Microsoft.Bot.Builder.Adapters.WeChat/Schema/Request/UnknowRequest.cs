using System.Xml.Linq;
using System.Xml.Serialization;

namespace Microsoft.Bot.Builder.Adapters.WeChat.Schema.Request
{
    [XmlRoot("xml")]
    public class UnknowRequest : RequestMessage
    {
        public override RequestMessageType MsgType => RequestMessageType.Unknown;

        /// <summary>
        /// Gets or sets Content.
        /// </summary>
        /// <value>
        /// Original request body of the unknow type, should be xml format.
        /// </value>
        [XmlElement(ElementName = "Content")]
        public XDocument Content { get; set; }
    }
}
