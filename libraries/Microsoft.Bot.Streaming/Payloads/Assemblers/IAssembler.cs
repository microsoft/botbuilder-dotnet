// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.IO;

namespace Microsoft.Bot.Streaming.Payloads
{
    public interface IAssembler
    {
#pragma warning disable CA1716 // Identifiers should not match keywords (we can't change this without breaking binary compat)
        bool End { get; }
#pragma warning restore CA1716 // Identifiers should not match keywords

        Guid Id { get; }

        void Close();

        Stream CreateStreamFromPayload();

        Stream GetPayloadAsStream();

        void OnReceive(Header header, Stream stream, int contentLength);
    }
}
