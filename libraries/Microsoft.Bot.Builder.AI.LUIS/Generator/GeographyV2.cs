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
#pragma warning disable CA1034 // Nested types should not be visible (we can't change this without breaking binary compat)
        public static class Types
#pragma warning restore CA1034 // Nested types should not be visible
        {
            /// <summary>
            /// Constant for LUIS geographic location type of POI.
            /// </summary>
            public const string POI = "poi";

            /// <summary>
            /// Constant for LUIS geographic location type of City.
            /// </summary>
            public const string City = "city";

            /// <summary>
            /// Constant for LUIS geographic location type Country or Region.
            /// </summary>
            public const string CountryRegion = "countryRegion";

            /// <summary>
            /// Constant for LUIS geographic location type of Continent.
            /// </summary>
            public const string Continent = "continent";

            /// <summary>
            /// Constant for LUIS geographic location type of State.
            /// </summary>
            public const string State = "state";
        }
    }
}
