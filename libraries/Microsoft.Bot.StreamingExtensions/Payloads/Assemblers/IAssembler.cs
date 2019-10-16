// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.IO;

namespace Microsoft.Bot.StreamingExtensions.Payloads
{
    public interface IAssembler
    {
        bool End { get; }

        Guid Id { get; }

        void Close();

        Stream CreateStreamFromPayload();

        Stream GetPayloadAsStream();

        void OnReceive(Header header, Stream stream, int contentLength);
    }
}
