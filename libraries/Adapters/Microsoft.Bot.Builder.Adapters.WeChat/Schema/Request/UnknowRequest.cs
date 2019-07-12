using System.Xml.Linq;
using System.Xml.Serialization;

namespace Microsoft.Bot.Builder.Adapters.WeChat.Schema.Request
{
    [XmlRoot("xml")]
    public class UnknowRequest : RequestMessage
    {
        public override RequestMessageType MsgType => RequestMessageType.Unknown;

        /// <summary>
        /// Original request body of the unknow type, should be xml format.
        /// </summary>
        [XmlElement(ElementName = "Content")]
        public XDocument Content { get; set; }
    }
}
