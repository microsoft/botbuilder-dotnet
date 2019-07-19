// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.AI.Luis
{
    /// <summary>
    /// Strongly typed LUIS builtin GeographyV2.
    /// </summary>
    public class GeographyV2
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GeographyV2"/> class.
        /// </summary>
        /// <param name="type">Type of geographic location from <see cref="Types"/>.</param>
        /// <param name="location">Geographic location.</param>
        public GeographyV2(string type, string location)
        {
            Type = type;
            Location = location;
        }

        /// <summary>
        /// Gets or sets type of geographic location.
        /// </summary>
        /// <value>
        /// Type of geographic location from <see cref="Types"/>.
        /// </value>
        [JsonProperty("type")]
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets geographic location.
        /// </summary>
        /// <value>
        /// Geographic location.
        /// </value>
        [JsonProperty("location")]
        public string Location { get; set; }

        /// <inheritdoc/>
        public override string ToString() => $"GeographyV2({Type}, {Location})";

        /// <summary>
        /// Different types of geographic locations.
        /// </summary>
        public static class Types
        {
            public const string POI = "poi";
            public const string City = "city";
            public const string CountryRegion = "countryRegion";
            public const string Continent = "continent";
            public const string State = "state";
        }
    }
}
