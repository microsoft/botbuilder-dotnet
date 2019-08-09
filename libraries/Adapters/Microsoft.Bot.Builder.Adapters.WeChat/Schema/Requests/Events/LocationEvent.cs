// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Xml.Serialization;

namespace Microsoft.Bot.Builder.Adapters.WeChat.Schema.Requests.Events
{
    /// <summary>
    /// Get user's location
    /// Two method to get location:
    /// 1. submit location when user enter the conversation
    /// 2. submit location every 5 seconds after user enter the conversation.
    /// </summary>
    [XmlRoot("xml")]
    public class LocationEvent : RequestEvent
    {
        /// <summary>
        /// Gets event, EventType: LOCATION.
        /// </summary>
        /// <value>
        /// EventType: LOCATION.
        /// </value>
        public override string EventType => EventTypes.Location;

        /// <summary>
        /// Gets or sets latitude.
        /// </summary>
        /// <value>
        /// Latitude, exist when EventType is Location.
        /// </value>
        [XmlElement(ElementName = "Latitude")]
        public double Latitude { get; set; }

        /// <summary>
        /// Gets or sets longitude.
        /// </summary>
        /// <value>
        /// Longitude, exist when EventType is Location.
        /// </value>
        [XmlElement(ElementName = "Longitude")]
        public double Longitude { get; set; }

        /// <summary>
        /// Gets or sets precision.
        /// </summary>
        /// <value>
        /// Precision, exist when EventType is Location.
        /// </value>
        [XmlElement(ElementName = "Precision")]
        public double Precision { get; set; }
    }
}
