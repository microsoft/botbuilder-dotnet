// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Newtonsoft.Json;
using Xunit.Abstractions;

namespace Microsoft.Bot.Builder.Testing.XUnit
{
    /// <summary>
    /// A wrapper class for XUnit test data that enables support for enumerating test cases in Test Explorer.
    /// </summary>
    /// <remarks>
    /// VS Test explorer only supports value types for data driven tests.
    /// This class implements <see cref="IXunitSerializable"/> and serializes complex types as json
    /// so the test cases can be enumerated and displayed into VS test explorer.
    /// This also allows the developer to right click on a particular test case on VS Test explorer and run it individually.
    /// </remarks>
    public class TestDataObject : IXunitSerializable
    {
        private const string TestObjectKey = "TestObjectKey";

        /// <summary>
        /// Initializes a new instance of the <see cref="TestDataObject"/> class.
        /// </summary>
        public TestDataObject()
        {
            // Note: This empty constructor is needed by the serializer.
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TestDataObject"/> class.
        /// </summary>
        /// <param name="testData">An object with the data to be used in the test.</param>
        public TestDataObject(object testData)
        {
            TestObject = JsonConvert.SerializeObject(testData);
        }

        /// <summary>
        /// Gets a json string with the test data object.
        /// </summary>
        /// <value>The test data object as a json string.</value>
        public string TestObject { get; private set; }

        /// <summary>
        /// Used by XUnit.net for deserialization.
        /// </summary>
        /// <param name="serializationInfo">A parameter used by XUnit.net.</param>
        public void Deserialize(IXunitSerializationInfo serializationInfo)
        {
            TestObject = serializationInfo.GetValue<string>(TestObjectKey);
        }

        /// <summary>
        /// Used by XUnit.net for serialization.
        /// </summary>
        /// <param name="serializationInfo">A parameter used by XUnit.net.</param>
        public void Serialize(IXunitSerializationInfo serializationInfo)
        {
            serializationInfo.AddValue(TestObjectKey, TestObject);
        }

        /// <summary>
        /// Gets the test data object for the specified .Net type.
        /// </summary>
        /// <typeparam name="T">The type of the object to be returned.</typeparam>
        /// <returns>The test object instance.</returns>
        public T GetObject<T>()
        {
            return JsonConvert.DeserializeObject<T>(TestObject);
        }
    }
}
