// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Newtonsoft.Json;
using Xunit.Abstractions;

namespace Microsoft.BotBuilderSamples.Tests.Framework.XUnit
{
    /// <summary>
    /// A wrapper class for test data that enables support for enumerating test cases in Test Explorer.
    /// </summary>
    /// <remarks>
    /// VS Test explorer only supports value types for data driven tests.
    /// This class implements <see cref="IXunitSerializable"/> and serializes complex types as json
    /// so the test data can be enumerated and displayed into VS test explorer.
    /// This also allows the developer to right click on a particular test case and run it individually.
    /// </remarks>
    public class TestDataObject : IXunitSerializable
    {
        private const string TestObjectKey = "TestObjectKey";

        // Needed by serializer
        public TestDataObject()
        {
        }

        public TestDataObject(object testData)
        {
            TestObject = JsonConvert.SerializeObject(testData);
        }

        /// <summary>
        /// Gets a json string with the test data object.
        /// </summary>
        /// <value>The test data object as a json string.</value>
        public string TestObject { get; private set; }

        public void Deserialize(IXunitSerializationInfo serializationInfo)
        {
            TestObject = serializationInfo.GetValue<string>(TestObjectKey);
        }

        public void Serialize(IXunitSerializationInfo serializationInfo)
        {
            serializationInfo.AddValue(TestObjectKey, TestObject);
        }

        /// <summary>
        /// Gets the test data object for the specified .Net type.
        /// </summary>
        /// <typeparam name="T">The type of the object to returned.</typeparam>
        /// <returns>The test object.</returns>
        public T GetObject<T>()
        {
            return JsonConvert.DeserializeObject<T>(TestObject);
        }
    }
}
