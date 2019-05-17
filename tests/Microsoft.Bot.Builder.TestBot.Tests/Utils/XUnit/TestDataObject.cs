// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Newtonsoft.Json;
using Xunit.Abstractions;

namespace Microsoft.BotBuilderSamples.Tests.Utils.XUnit
{
    /// <summary>
    /// A wrapper class for test data that enables support for enumerating test cases in Test Explorer.
    /// </summary>
    /// <remarks>
    /// Test explorer only supports value types for data driven tests. This class takes a complex types
    /// and serializes it as json so it can be enumerated and displayed into test explorer.
    /// This also allows the developer to right click on a particular test case and run it.
    /// </remarks>
    public class TestDataObject : IXunitSerializable
    {
        private const string TestObjectKey = "TestObjectName";

        public TestDataObject()
        {
        }

        // Needed for deserializer
        public TestDataObject(object testData)
        {
            TestObject = JsonConvert.SerializeObject(testData);
        }

        public string TestObject { get; private set; }

        public void Deserialize(IXunitSerializationInfo serializationInfo)
        {
            TestObject = serializationInfo.GetValue<string>(TestObjectKey);
        }

        public void Serialize(IXunitSerializationInfo serializationInfo)
        {
            serializationInfo.AddValue(TestObjectKey, TestObject);
        }

        public T GetObject<T>()
        {
            return JsonConvert.DeserializeObject<T>(TestObject);
        }
    }
}
