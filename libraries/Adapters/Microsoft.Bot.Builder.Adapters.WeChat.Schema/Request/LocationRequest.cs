using System.Xml.Serialization;

namespace Microsoft.Bot.Builder.Adapters.WeChat.Schema.Request
{
    [XmlRoot("xml")]
    public class LocationRequest : RequestMessage
    {
        public override RequestMessageType MsgType => RequestMessageType.Location;

        /// <summary>
        /// The Latitude.
        /// </summary>
        [XmlElement(ElementName = "Location_X")]
        public double Location_X { get; set; }

        /// <summary>
        /// The longtitude.
        /// </summary>
        [XmlElement(ElementName = "Location_Y")]
        public double Location_Y { get; set; }

        /// <summary>
        /// Map Zoom Size.
        /// </summary>
        [XmlElement(ElementName = "Scale")]
        public int Scale { get; set; }

        /// <summary>
        /// Geolocation information in Text.
        /// </summary>
        [XmlElement(ElementName = "Label")]
        public string Label { get; set; }
    }
}
