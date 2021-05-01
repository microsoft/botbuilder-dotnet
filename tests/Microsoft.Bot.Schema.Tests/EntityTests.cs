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
        [ClassData(typeof(EntityTestData))]
        public void EntityEqualsObject(object obj, bool expected)
        {
            // TODO need to change this to test case where obj is the entity itself
            // might need to do separate unit test
            var entity = new Entity();
            var areEqual = entity.Equals(obj);

            Assert.Equal(expected, areEqual);
        }

        private class EntityTestData : IEnumerable<object[]>
        {
            public IEnumerator<object[]> GetEnumerator()
            {
                yield return new object[] { null, false };
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }
    }
}
