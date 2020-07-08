// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Integration
{
    public static class MessageSerializerSettings
    {
        public static JsonSerializerSettings Create()
        {
            using (var connector = new ConnectorClient(new Uri("http://localhost/")))
            {
                return connector.DeserializationSettings;
            }
        }
    }
}
