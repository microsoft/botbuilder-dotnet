// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Streaming.Payloads
{
    internal interface IPayloadTypeManager
    {
        IAssembler CreatePayloadAssembler(Header header);
    }
}
