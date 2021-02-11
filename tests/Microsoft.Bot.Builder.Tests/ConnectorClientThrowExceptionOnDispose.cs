// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Microsoft.Bot.Connector;
using Microsoft.Rest;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Tests
{
    /// <summary>
    /// ConnectorClient which throws exception when disposed.
    /// </summary>
    /// <remarks>Moq failed to create this properly. Boo moq!.</remarks>
    public class ConnectorClientThrowExceptionOnDispose : IConnectorClient
    {
        public Uri BaseUri { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public JsonSerializerSettings SerializationSettings => throw new NotImplementedException();

        public JsonSerializerSettings DeserializationSettings => throw new NotImplementedException();

        public ServiceClientCredentials Credentials => throw new NotImplementedException();

        public IAttachments Attachments => throw new NotImplementedException();

        public IConversations Conversations => throw new NotImplementedException();

        public void Dispose() => throw new Exception("Should not be disposed!");
    }
}
