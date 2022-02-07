// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
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
            // connector exposes the serializer settings it uses
            using (var connector = new ConnectorClient(new Uri("http://localhost/")))
            {
                Assert.IsType<DefaultContractResolver>(connector.DeserializationSettings.ContractResolver);
                Assert.IsType<DefaultContractResolver>(connector.SerializationSettings.ContractResolver);
            }
        }
    }
}
