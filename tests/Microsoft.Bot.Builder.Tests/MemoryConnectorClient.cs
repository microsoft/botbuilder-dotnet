// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.using System.Security.Claims;

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.Rest;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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
