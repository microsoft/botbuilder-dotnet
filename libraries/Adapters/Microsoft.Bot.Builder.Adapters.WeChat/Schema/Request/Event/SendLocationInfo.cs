// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Xml.Serialization;

namespace Microsoft.Bot.Builder.Adapters.WeChat.Schema.Request.Event
{
    public class SendLocationInfo
    {
        /// <summary>
        /// Gets or sets location_X.
        /// </summary>
        /// <value>
        /// The Latitude infomation.
        /// </value>
        [XmlElement(ElementName = "Location_X")]
        public string Location_X { get; set; }

        /// <summary>
        /// Gets or sets location_Y.
        /// </summary>
        /// <value>
        /// The longtitude information.
        /// </value>
        [XmlElement(ElementName = "Location_Y")]
        public string Location_Y { get; set; }

        /// <summary>
        /// Gets or sets scale.
        /// </summary>
        /// <value>
        /// Map Zoom Size information.
        /// </value>
        [XmlElement(ElementName = "Scale")]
        public string Scale { get; set; }

        /// <summary>
        /// Gets or sets Label.
        /// </summary>
        /// <value>
        /// Geolocation information in Text.
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
