// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.using System.Security.Claims;

using System;
using Microsoft.Bot.Connector;
using Microsoft.Rest;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Tests
{
    public class MemoryConnectorClient : IConnectorClient
    {
        public MemoryConversations MemoryConversations { get; private set; } = new MemoryConversations();

        public Uri BaseUri { get; set; }

        public JsonSerializerSettings SerializationSettings { get; set; }

        public JsonSerializerSettings DeserializationSettings { get; set; }

        public ServiceClientCredentials Credentials { get; set; }

        public IAttachments Attachments { get; set; }

        public IConversations Conversations
        {
            get => MemoryConversations;
        }

        public void Dispose()
        {
        }
    }
}
