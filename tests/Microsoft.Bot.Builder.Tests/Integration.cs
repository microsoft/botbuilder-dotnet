// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Microsoft.Bot.Builder.Integration;
using Microsoft.Bot.Connector;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Serialization;

namespace Microsoft.Bot.Builder.Tests
{
    [TestClass]
    [TestCategory("BotAdapter")]

    public class Integration
    {
        [TestMethod]
        public void CheckSerializerSettings()
        {
            // used in the integration layer
            var settings = MessageSerializerSettings.Create();

            // connector exposes the serializer settings it uses
            var connector = new ConnectorClient(new Uri("http://localhost/"));

            Assert.IsInstanceOfType(settings.ContractResolver, typeof(DefaultContractResolver));
            Assert.IsInstanceOfType(connector.DeserializationSettings.ContractResolver, typeof(DefaultContractResolver));
            Assert.IsInstanceOfType(connector.SerializationSettings.ContractResolver, typeof(DefaultContractResolver));
        }
    }
}
