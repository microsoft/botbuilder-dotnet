using System.Xml.Serialization;

namespace Microsoft.Bot.Builder.Adapters.WeChat.Schema.Request
{
    [XmlRoot("xml")]
    public class LocationRequest : RequestMessage
    {
        public override RequestMessageType MsgType => RequestMessageType.Location;

        /// <summary>
        /// Gets or sets Location_X.
        /// </summary>
        /// <value>
        /// The Latitude.
        /// </value>
        [XmlElement(ElementName = "Location_X")]
        public double Location_X { get; set; }

        /// <summary>
        /// Gets or sets Location_Y.
        /// </summary>
        /// <value>
        /// The longtitude.
        /// </value>
        [XmlElement(ElementName = "Location_Y")]
        public double Location_Y { get; set; }

        /// <summary>
        /// Gets or sets Scale.
        /// </summary>
        /// <value>
        /// Map Zoom Size.
        /// </value>
        [XmlElement(ElementName = "Scale")]
        public int Scale { get; set; }

        /// <summary>
        /// Gets or sets Label.
        /// </summary>
        /// <value>
        /// Geolocation information in Text.
        /// </value>
        [XmlElement(ElementName = "Label")]
        public string Label { get; set; }
    }
}
