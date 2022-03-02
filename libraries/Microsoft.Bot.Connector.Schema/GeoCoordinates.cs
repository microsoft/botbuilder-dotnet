// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Text.Json;

namespace Microsoft.Bot.Connector.Schema
{
    /// <summary>
    /// GeoCoordinates (entity type: "https://schema.org/GeoCoordinates").
    /// </summary>
    public class GeoCoordinates : Entity
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GeoCoordinates"/> class.
        /// </summary>
        public GeoCoordinates()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GeoCoordinates"/> class.
        /// </summary>
        /// <param name="elevation">Elevation of the location [WGS
        /// 84](https://en.wikipedia.org/wiki/World_Geodetic_System).</param>
        /// <param name="latitude">Latitude of the location [WGS
        /// 84](https://en.wikipedia.org/wiki/World_Geodetic_System).</param>
        /// <param name="longitude">Longitude of the location [WGS
        /// 84](https://en.wikipedia.org/wiki/World_Geodetic_System).</param>
        /// <param name="type">The type of the thing.</param>
        /// <param name="name">The name of the thing.</param>
        public GeoCoordinates(double? elevation = default, double? latitude = default, double? longitude = default, string type = default, string name = default)
        {
            Elevation = elevation;
            Latitude = latitude;
            Longitude = longitude;
            Type = type;
            Name = name;
            CustomInit();
        }

        /// <summary>
        /// Gets or sets elevation of the location [WGS
        /// 84](https://en.wikipedia.org/wiki/World_Geodetic_System).
        /// </summary>
        /// <value>The elevation of the location.</value>
        [JsonPropertyName("elevation")]
        public double? Elevation { get; set; }

        /// <summary>
        /// Gets or sets latitude of the location [WGS
        /// 84](https://en.wikipedia.org/wiki/World_Geodetic_System).
        /// </summary>
        /// <value>The lattitude of the location.</value>
        [JsonPropertyName("latitude")]
        public double? Latitude { get; set; }

        /// <summary>
        /// Gets or sets longitude of the location [WGS
        /// 84](https://en.wikipedia.org/wiki/World_Geodetic_System).
        /// </summary>
        /// <value>The longitude of the location.</value>
        [JsonPropertyName("longitude")]
        public double? Longitude { get; set; }

        /// <summary>
        /// Gets or sets the name of the thing.
        /// </summary>
        /// <value>The name of the location.</value>
        [JsonPropertyName("name")]
        public string Name { get; set; }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults.
        /// </summary>
        private void CustomInit()
        {
            Type = "GeoCoordinates";
        }
    }
}
