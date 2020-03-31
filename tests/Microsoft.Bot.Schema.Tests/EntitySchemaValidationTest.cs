// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

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
        public void EntityTests_GeoCoordinatesSerializationDeserializationTest_AsType()
        {
            GeoCoordinates geoCoordinates = new GeoCoordinates()
            {
                Latitude = 22,
                Elevation = 23,
            };

            EntityTest(geoCoordinates, EntityTypes.GeoCoordinates);
        }

        /// <summary>
        /// Ensures that <see cref="GeoCoordinates"/> class can be serialized and deserialized properly.
        /// </summary>
        [TestMethod]
        public void EntityTests_GeoCoordinatesSerializationDeserializationTest_AsEntity()
        {
            Entity geoCoordinates = new GeoCoordinates()
            {
                Latitude = 22,
                Elevation = 23,
            };

            EntityTest(geoCoordinates, EntityTypes.GeoCoordinates);
        }

        /// <summary>
        /// Ensures that <see cref="GeoCoordinates"/> class can be serialized and deserialized properly.
        /// </summary>
        [TestMethod]
        public void EntityTests_GeoCoordinatesSerializationDeserializationTest_SetAs()
        {
            var entity = new Entity();
            entity.SetAs(new GeoCoordinates
            {
                Latitude = 22,
                Elevation = 23,
            });

            EntityTest(entity, EntityTypes.GeoCoordinates);
        }

        /// <summary>
        /// Ensures that <see cref="Mention"/> class can be serialized and deserialized properly.
        /// </summary>
        [TestMethod]
        public void EntityTests_MentionSerializationDeserializationTest_AsType()
        {
            Mention mentionEntity = new Mention()
            {
                Text = "TESTTEST",
            };

            EntityTest(mentionEntity, EntityTypes.Mention);
        }

        /// <summary>
        /// Ensures that <see cref="Mention"/> class can be serialized and deserialized properly.
        /// </summary>
        [TestMethod]
        public void EntityTests_MentionSerializationDeserializationTest_AsEntity()
        {
            Entity mentionEntity = new Mention()
            {
                Text = "TESTTEST",
            };

            EntityTest(mentionEntity, EntityTypes.Mention);
        }

        /// <summary>
        /// Ensures that <see cref="Mention"/> class can be serialized and deserialized properly.
        /// </summary>
        [TestMethod]
        public void EntityTests_MentionSerializationDeserializationTest_SetAs()
        {
            var entity = new Entity();
            entity.SetAs(new Mention()
            {
                Text = "TESTTEST",
            });

            EntityTest(entity, EntityTypes.Mention);
        }

        /// <summary>
        /// Ensures that <see cref="Place"/> class can be serialized and deserialized properly.
        /// </summary>
        [TestMethod]
        public void EntityTests_PlaceSerializationDeserializationTest_AsType()
        {
            Place placeEntity = new Place()
            {
                Name = "TESTTEST",
            };
            EntityTest(placeEntity, EntityTypes.Place);
        }

        /// <summary>
        /// Ensures that <see cref="Place"/> class can be serialized and deserialized properly.
        /// </summary>
        [TestMethod]
        public void EntityTests_PlaceSerializationDeserializationTest_AsEntity()
        {
            Entity placeEntity = new Place()
            {
                Name = "TESTTEST",
            };
            EntityTest(placeEntity, EntityTypes.Place);
        }

        /// <summary>
        /// Ensures that <see cref="Place"/> class can be serialized and deserialized properly.
        /// </summary>
        [TestMethod]
        public void EntityTests_PlaceSerializationDeserializationTest_SetAs()
        {
            var entity = new Entity();
            entity.SetAs(new Place()
            {
                Name = "TESTTEST",
            });

            EntityTest(entity, EntityTypes.Place);
        }

        private void EntityTest<T>(T geoCoordinates, string type)
            where T : Entity
        {
            Assert.AreEqual(type, geoCoordinates.Type);

            string serialized = JsonConvert.SerializeObject(geoCoordinates);

            Entity deserializedEntity = JsonConvert.DeserializeObject<Entity>(serialized);
            Assert.AreEqual(deserializedEntity.Type, geoCoordinates.Type);
            var geo = deserializedEntity.GetAs<GeoCoordinates>();
            Assert.AreEqual(geo.Type, geoCoordinates.Type);
        }
    }
}
