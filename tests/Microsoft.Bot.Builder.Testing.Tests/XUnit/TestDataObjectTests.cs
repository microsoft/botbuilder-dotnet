// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Microsoft.Bot.Builder.Testing.XUnit;
using Moq;
using Newtonsoft.Json;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Bot.Builder.Testing.Tests.XUnit
{
    public class TestDataObjectTests
    {
        private readonly MyTestDataObject _testObject;

        public TestDataObjectTests()
        {
            _testObject = new MyTestDataObject()
            {
                SomeText = "Some Text",
                SomeNumber = 42,
            };
        }

        [Fact]
        public void ShouldSerializeAsJson()
        {
            var sut = new TestDataObject(_testObject);
            var innerObject = sut.GetObject<MyTestDataObject>();
            Assert.Equal(JsonConvert.SerializeObject(_testObject), sut.TestObject);
            Assert.NotSame(_testObject, innerObject);
            Assert.Equal(_testObject.SomeText, innerObject.SomeText);
            Assert.Equal(_testObject.SomeNumber, innerObject.SomeNumber);
        }

        [Fact]
        public void ShouldSupportIXunitSerializable()
        {
            var sut = new TestDataObject(_testObject);
            Assert.IsAssignableFrom<IXunitSerializable>(sut);

            object receivedObject = null;
            var mockXUnitSerializer = new Mock<IXunitSerializationInfo>();
            mockXUnitSerializer
                .Setup(x => x.AddValue(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<Type>()))
                .Callback((string key, object @object, Type type) =>
                {
                    receivedObject = @object;
                });

            // Serializes the json representation
            sut.Serialize(mockXUnitSerializer.Object);
            Assert.Equal(sut.TestObject, receivedObject);

            // Invokes the GetValue on XUnit for deserialization.
            sut.Deserialize(mockXUnitSerializer.Object);
            mockXUnitSerializer.Verify(x => x.GetValue<string>(It.IsAny<string>()), Times.Once);
        }

        private class MyTestDataObject
        {
            public string SomeText { get; set; }

            public int SomeNumber { get; set; }
        }
    }
}
