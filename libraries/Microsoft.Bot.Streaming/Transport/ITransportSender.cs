// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading.Tasks;

namespace Microsoft.Bot.Streaming.Transport
{
    public interface ITransportSender : ITransport
    {
        Task<int> SendAsync(byte[] buffer, int offset, int count);
    }
}
