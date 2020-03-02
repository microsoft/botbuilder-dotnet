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
    public class ConnectorClientThrowExceptionOnDispose : ConnectorClientBase
    {
        public override Uri BaseUri { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public override JsonSerializerSettings SerializationSettings => throw new NotImplementedException();

        public override JsonSerializerSettings DeserializationSettings => throw new NotImplementedException();

        public override ServiceClientCredentials Credentials => throw new NotImplementedException();

        public override IAttachments Attachments => throw new NotImplementedException();

        public override Conversations Conversations => throw new NotImplementedException();

        public new void Dispose() => throw new Exception("Should not be disposed!");
    }
}
