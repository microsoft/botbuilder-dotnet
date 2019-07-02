// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;

namespace Microsoft.Bot.StreamingExtensions
{
    internal class OperationException : Exception
    {
        public OperationException(string message, int statusCode, object body)
    : base(message)
        {
            StatusCode = statusCode;
            Body = body;
        }

        public int StatusCode { get; set; }

        public object Body { get; set; }
    }
}
