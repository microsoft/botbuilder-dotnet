// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.using System.Security.Claims;

using System;
using Microsoft.Bot.Connector;
using Microsoft.Rest;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Tests
{
    public class MemoryConnectorClient : ConnectorClientBase
    {
        public MemoryConversations MemoryConversations { get; private set; } = new MemoryConversations();

        public override Conversations Conversations
        {
            get => MemoryConversations;
        }

        public override Uri BaseUri { get; set; }

        public override JsonSerializerSettings SerializationSettings { get; }

        public override JsonSerializerSettings DeserializationSettings { get; }

        public override ServiceClientCredentials Credentials { get; }

        public override IAttachments Attachments { get; }
    }
}
