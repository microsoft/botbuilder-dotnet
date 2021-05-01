// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.Bot.Schema.Tests
{
    public class EntityTests
    {
        [Fact]
        public void EntityInits()
        {
            var type = "entityType";

            var entity = new Entity(type);

            Assert.NotNull(entity);
            Assert.IsType<Entity>(entity);
            Assert.Equal(type, entity.Type);
        }

        [Fact]
        public void EntityInitsWithNoArgs()
        {
            var entity = new Entity();

            Assert.NotNull(entity);
            Assert.IsType<Entity>(entity);
        }

        [Fact]
        public void SetEntityAsTargetObject()
        {
            var entity = new Entity();
            Assert.Null(entity.Type);

            var type = typeof(JObject).Name;
            var obj = new JObject()
            {
                { "Name", "Esper" },
                { "Eyes", "Brown" },
                { "Type", type }
            };

            entity.SetAs(obj);
            var properties = entity.Properties;

            Assert.Equal(type, entity.Type);
            Assert.Equal(obj.Value<string>("Name"), properties.Value<string>("Name"));
            Assert.Equal(obj.Value<string>("Eyes"), properties.Value<string>("Eyes"));
        }

        [Fact]
        public void TestGetHashCode()
        {
            var hash = new Entity().GetHashCode();

            Assert.IsType<int>(hash);
        }

        [Theory]
        [ClassData(typeof(EntityToEntityData))]
        public void EntityEqualsAnotherEntity(Entity other, bool expected)
        {
            var entity = new Entity("color");
            var areEqual = entity.Equals(other);

            Assert.Equal(expected, areEqual);
        }

        [Theory]
        [ClassData(typeof(EntityToObjectData))]
        public void EntityEqualsObject(Entity entity, object obj, bool expected)
        {
            var areEqual = entity.Equals(obj);

            Assert.Equal(expected, areEqual);
        }

        private class EntityToObjectData : IEnumerable<object[]>
        {
            public Entity Entity { get; set; } = new Entity("color");

            public IEnumerator<object[]> GetEnumerator()
            {
                yield return new object[] { Entity, null, false };
                yield return new object[] { Entity, Entity, true };
                yield return new object[] { Entity, new JObject(), false };
                yield return new object[] { Entity, new Entity("color"), true };
                yield return new object[] { Entity, new Entity("flamingo"), false };
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        private class EntityToEntityData : IEnumerable<object[]>
        {
            public IEnumerator<object[]> GetEnumerator()
            {
                yield return new object[] { new Entity("color"), true };
                yield return new object[] { new Entity("flamingo"), false };
                yield return new object[] { null, false };
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }
    }
}
