// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Microsoft.Bot.Builder.Integration;
using Microsoft.Bot.Connector;
using Newtonsoft.Json.Serialization;
using Xunit;

namespace Microsoft.Bot.Builder.Tests
{
    public class Integration
    {
        [Fact]
        public void CheckSerializerSettings()
        {
            // used in the integration layer
            var settings = MessageSerializerSettings.Create();

            // connector exposes the serializer settings it uses
            var connector = new ConnectorClient(new Uri("http://localhost/"));

            Assert.IsType<DefaultContractResolver>(settings.ContractResolver);
            Assert.IsType<DefaultContractResolver>(connector.DeserializationSettings.ContractResolver);
            Assert.IsType<DefaultContractResolver>(connector.SerializationSettings.ContractResolver);
        }
    }
}
