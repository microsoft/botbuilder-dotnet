// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Newtonsoft.Json;
using Xunit;

namespace Microsoft.Bot.Schema.Tests
{
    /// <summary>
    /// Entity schema validation tests to ensure that serilization and deserialization work as expected.
    /// </summary>
    public class EntitySchemaValidationTest
    {
        /// <summary>
        /// Ensures that <see cref="GeoCoordinates"/> class can be serialized and deserialized properly.
        /// </summary>
        [Fact]
        public void EntityTests_GeoCoordinatesSerializationDeserializationTest()
        {
            GeoCoordinates geoCoordinates = new GeoCoordinates()
            {
                Latitude = 22,
                Elevation = 23,
            };

            Assert.Equal("GeoCoordinates", geoCoordinates.Type);
            string serialized = JsonConvert.SerializeObject(geoCoordinates);

            Entity deserializedEntity = JsonConvert.DeserializeObject<Entity>(serialized);
            Assert.Equal(deserializedEntity.Type, geoCoordinates.Type);
            var geo = deserializedEntity.GetAs<GeoCoordinates>();
            Assert.Equal(geo.Type, geoCoordinates.Type);
        }

        /// <summary>
        /// Ensures that <see cref="Mention"/> class can be serialized and deserialized properly.
        /// </summary>
        [Fact]
        public void EntityTests_MentionSerializationDeserializationTest()
        {
            Mention mentionEntity = new Mention()
            {
                Text = "TESTTEST",
            };

            Assert.Equal("mention", mentionEntity.Type);
            string serialized = JsonConvert.SerializeObject(mentionEntity);

            Entity deserializedEntity = JsonConvert.DeserializeObject<Entity>(serialized);
            Assert.Equal(deserializedEntity.Type, mentionEntity.Type);
            var mentionDeserialized = deserializedEntity.GetAs<Mention>();
            Assert.Equal(mentionDeserialized.Type, mentionEntity.Type);
        }

        /// <summary>
        /// Ensures that <see cref="Place"/> class can be serialized and deserialized properly.
        /// </summary>
        [Fact]
        public void EntityTests_PlaceSerializationDeserializationTest()
        {
            Place placeEntity = new Place()
            {
                Name = "TESTTEST",
            };

            Assert.Equal("Place", placeEntity.Type);
            string serialized = JsonConvert.SerializeObject(placeEntity);

            Entity deserializedEntity = JsonConvert.DeserializeObject<Entity>(serialized);
            Assert.Equal(deserializedEntity.Type, placeEntity.Type);
            var placeDeserialized = deserializedEntity.GetAs<Place>();
            Assert.Equal(placeDeserialized.Type, placeEntity.Type);
        }
    }
}
