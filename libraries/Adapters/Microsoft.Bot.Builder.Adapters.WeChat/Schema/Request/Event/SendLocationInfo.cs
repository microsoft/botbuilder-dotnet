using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace Microsoft.Bot.Builder.Adapters.WeChat.Schema.Request.Event
{
    public class SendLocationInfo
    {
        /// <summary>
        /// Gets or sets location_X.
        /// </summary>
        /// <value>
        /// Location_X infomation.
        /// </value>
        [XmlElement(ElementName = "Location_X")]
        public string Location_X { get; set; }

        /// <summary>
        /// Gets or sets location_Y.
        /// </summary>
        /// <value>
        /// Location_Y information.
        /// </value>
        [XmlElement(ElementName = "Location_Y")]
        public string Location_Y { get; set; }

        /// <summary>
        /// Gets or sets scale.
        /// </summary>
        /// <value>
        /// Scale information.
        /// </value>
        [XmlElement(ElementName = "Scale")]
        public string Scale { get; set; }

        /// <summary>
        /// Gets or sets Label.
        /// </summary>
        /// <value>
        /// String of location information.
        /// </value>
        [XmlElement(ElementName = "Label")]
        public string Label { get; set; }

        /// <summary>
        /// Gets or sets Poiname.
        /// </summary>
        /// <value>
        /// POI name at Friend Zone.
        /// </value>
        [XmlElement(ElementName = "Poiname")]
        public string Poiname { get; set; }
    }
}
