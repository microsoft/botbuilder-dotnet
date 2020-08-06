// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Builder.Dialogs.Debugging.Protocol
{
    internal class Response : Message
    {
        public Response(int seq, Request request)
        {
            Seq = seq;
            Type = "response";
            Request_seq = request.Seq;
            Success = true;
            Command = request.Command;
        }

#pragma warning disable CA1707 // Identifiers should not contain underscores (we can't change this without breaking binary compat)
        public int Request_seq { get; set; }
#pragma warning restore CA1707 // Identifiers should not contain underscores

        public bool Success { get; set; }

        public string Message { get; set; }

        public string Command { get; set; }

        public static Response<TBody> From<TBody>(int seq, Request request, TBody body) => new Response<TBody>(seq, request) { Body = body };

        public static Response<string> Fail(int seq, Request request, string message) => new Response<string>(seq, request)
        {
            Body = message,
            Message = message,
            Success = false
        };
    }
}
