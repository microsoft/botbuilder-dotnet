// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Xml.Serialization;

namespace Microsoft.Bot.Builder.Adapters.WeChat.Schema.Requests.Events
{
    public class SendLocationInfo
    {
        /// <summary>
        /// Gets or sets Latitude.
        /// </summary>
        /// <value>
        /// The latitude infomation.
        /// </value>
        [XmlElement(ElementName = "Location_X")]
        public string Latitude { get; set; }

        /// <summary>
        /// Gets or sets Longtitude.
        /// </summary>
        /// <value>
        /// The longtitude information.
        /// </value>
        [XmlElement(ElementName = "Location_Y")]
        public string Longtitude { get; set; }

        /// <summary>
        /// Gets or sets scale.
        /// </summary>
        /// <value>
        /// Map zoom size information.
        /// </value>
        [XmlElement(ElementName = "Scale")]
        public string Scale { get; set; }

        /// <summary>
        /// Gets or sets Label.
        /// </summary>
        /// <value>
        /// Geolocation information in text.
        /// </value>
        [XmlElement(ElementName = "Label")]
        public string Label { get; set; }

        /// <summary>
        /// Gets or sets PoiName.
        /// </summary>
        /// <value>
        /// POI name at Friend Zone.
        /// </value>
        [XmlElement(ElementName = "Poiname")]
        public string PoiName { get; set; }
    }
}
