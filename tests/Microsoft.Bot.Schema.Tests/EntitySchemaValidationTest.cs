// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace Microsoft.Bot.Schema.Tests
{
    /// <summary>
    /// Entity schema validation tests to ensure that serilization and deserialization work as expected.
    /// </summary>
    [TestClass]
    public class EntitySchemaValidationTest
    {
        /// <summary>
        /// Ensures that <see cref="GeoCoordinates"/> class can be serialized and deserialized properly.
        /// </summary>
        [TestMethod]
        public void EntityTests_GeoCoordinatesSerializationDeserializationTest()
        {
            GeoCoordinates geoCoordinates = new GeoCoordinates()
            {
                Latitude = 22,
                Elevation = 23,
            };

            Assert.AreEqual("GeoCoordinates", geoCoordinates.Type);
            string serialized = JsonConvert.SerializeObject(geoCoordinates);

            Entity deserializedEntity = JsonConvert.DeserializeObject<Entity>(serialized);
            Assert.AreEqual(deserializedEntity.Type, geoCoordinates.Type);
            var geo = deserializedEntity.GetAs<GeoCoordinates>();
            Assert.AreEqual(geo.Type, geoCoordinates.Type);
        }

        /// <summary>
        /// Ensures that <see cref="Mention"/> class can be serialized and deserialized properly.
        /// </summary>
        [TestMethod]
        public void EntityTests_MentionSerializationDeserializationTest()
        {
            Mention mentionEntity = new Mention()
            {
                Text = "TESTTEST",
            };

            Assert.AreEqual("mention", mentionEntity.Type);
            string serialized = JsonConvert.SerializeObject(mentionEntity);

            Entity deserializedEntity = JsonConvert.DeserializeObject<Entity>(serialized);
            Assert.AreEqual(deserializedEntity.Type, mentionEntity.Type);
            var mentionDeserialized = deserializedEntity.GetAs<Mention>();
            Assert.AreEqual(mentionDeserialized.Type, mentionEntity.Type);
        }

        /// <summary>
        /// Ensures that <see cref="Place"/> class can be serialized and deserialized properly.
        /// </summary>
        [TestMethod]
        public void EntityTests_PlaceSerializationDeserializationTest()
        {
            Place placeEntity = new Place()
            {
                Name = "TESTTEST",
            };

            Assert.AreEqual("Place", placeEntity.Type);
            string serialized = JsonConvert.SerializeObject(placeEntity);

            Entity deserializedEntity = JsonConvert.DeserializeObject<Entity>(serialized);
            Assert.AreEqual(deserializedEntity.Type, placeEntity.Type);
            var placeDeserialized = deserializedEntity.GetAs<Place>();
            Assert.AreEqual(placeDeserialized.Type, placeEntity.Type);
        }
    }
}
