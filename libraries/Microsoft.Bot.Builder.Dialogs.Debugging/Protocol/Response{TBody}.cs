// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Builder.Dialogs.Debugging.Protocol
{
    internal class Response<TBody> : Response
    {
        public Response(int seq, Request request)
            : base(seq, request)
        {
        }

        public TBody Body { get; set; }
    }
}
