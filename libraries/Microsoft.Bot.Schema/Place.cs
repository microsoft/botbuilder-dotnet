// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Schema
{
    using Newtonsoft.Json;

    /// <summary>
    /// Place (entity type: "https://schema.org/Place").
    /// </summary>
    public class Place : Entity
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Place"/> class.
        /// </summary>
        public Place()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Place"/> class.
        /// </summary>
        /// <param name="address">Address of the place (may be `string` or
        /// complex object of type `PostalAddress`).</param>
        /// <param name="geo">Geo coordinates of the place (may be complex
        /// object of type `GeoCoordinates` or `GeoShape`).</param>
        /// <param name="hasMap">Map to the place (may be `string` (URL) or
        /// complex object of type `Map`).</param>
        /// <param name="type">The type of the thing.</param>
        /// <param name="name">The name of the thing.</param>
        public Place(object address = default(object), object geo = default(object), object hasMap = default(object), string type = default(string), string name = default(string))
        {
            Address = address;
            Geo = geo;
            HasMap = hasMap;
            Type = type;
            Name = name;
            CustomInit();
        }

        /// <summary>
        /// Gets or sets address of the place (may be `string` or complex
        /// object of type `PostalAddress`).
        /// </summary>
        /// <value>The address of the place.</value>
        [JsonProperty(PropertyName = "address")]
        public object Address { get; set; }

        /// <summary>
        /// Gets or sets geo coordinates of the place (may be complex object of
        /// type `GeoCoordinates` or `GeoShape`).
        /// </summary>
        /// <value>The geo coordinates of the place.</value>
        [JsonProperty(PropertyName = "geo")]
        public object Geo { get; set; }

        /// <summary>
        /// Gets or sets map to the place (may be `string` (URL) or complex
        /// object of type `Map`).
        /// </summary>
        /// <value>The map to the place.</value>
        [JsonProperty(PropertyName = "hasMap")]
        public object HasMap { get; set; }

        /// <summary>
        /// Gets or sets the name of the thing.
        /// </summary>
        /// <value>The name of the thing.</value>
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults.
        /// </summary>
        private void CustomInit()
        {
            Type = "Place";
        }
    }
}
