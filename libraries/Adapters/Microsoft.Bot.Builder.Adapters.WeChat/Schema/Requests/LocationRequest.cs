// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Xml.Serialization;

namespace Microsoft.Bot.Builder.Adapters.WeChat.Schema.Requests
{
    [XmlRoot("xml")]
    public class LocationRequest : RequestMessage
    {
        public override string MsgType => RequestMessageTypes.Location;

        /// <summary>
        /// Gets or sets Latitude.
        /// </summary>
        /// <value>
        /// The latitude infomation.
        /// </value>
        [XmlElement(ElementName = "Location_X")]
        public double Latitude { get; set; }

        /// <summary>
        /// Gets or sets Longtitude.
        /// </summary>
        /// <value>
        /// The longtitude infomation.
        /// </value>
        [XmlElement(ElementName = "Location_Y")]
        public double Longtitude { get; set; }

        /// <summary>
        /// Gets or sets Scale.
        /// </summary>
        /// <value>
        /// Map zoom size.
        /// </value>
        [XmlElement(ElementName = "Scale")]
        public int Scale { get; set; }

        /// <summary>
        /// Gets or sets Label.
        /// </summary>
        /// <value>
        /// Geolocation information in text.
        /// </value>
        [XmlElement(ElementName = "Label")]
        public string Label { get; set; }
    }
}
