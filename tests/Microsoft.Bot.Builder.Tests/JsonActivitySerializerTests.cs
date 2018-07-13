// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Serialization;
using Microsoft.Bot.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Microsoft.Bot.Builder.Tests
{
    public class JsonActivitySerializerTests
    {
        private readonly JsonActivitySerializer _serializer = new JsonActivitySerializer();

        [TestClass]
        public class SerializeAsyncTests : JsonActivitySerializerTests
        {
            [TestMethod]
            public async Task NullActivityParameterThrows()
            {
                try
                {
                    await _serializer.SerializeAsync(null, Mock.Of<Stream>());

                    Assert.Fail("Expected an exception.");
                }
                catch(ArgumentNullException exception)
                {
                    Assert.AreEqual("activity", exception.ParamName);
                }
            }

            [TestMethod]
            public async Task NullStreamParameterThrows()
            {
                try
                {
                    await _serializer.SerializeAsync(new Activity(), null);

                    Assert.Fail("Expected an exception.");
                }
                catch (ArgumentNullException exception)
                {
                    Assert.AreEqual("stream", exception.ParamName);
                }
            }

            // TODO: more tests
        }

        [TestClass]
        public class DeserializeAsyncTests : JsonActivitySerializerTests
        {
            [TestMethod]
            public async Task NullStreamParameterThrows()
            {
                try
                {
                    await _serializer.DeserializeAsync(null);

                    Assert.Fail("Expected an exception.");
                }
                catch (ArgumentNullException exception)
                {
                    Assert.AreEqual("stream", exception.ParamName);
                }
            }

            // TODO: more tests
        }
    }
}
