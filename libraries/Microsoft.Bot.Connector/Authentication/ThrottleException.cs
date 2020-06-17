// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;

namespace Microsoft.Bot.Connector.Authentication
{
    public class ThrottleException : Exception
    {
        public ThrottleException()
        {
        }

        public ThrottleException(string message)
            : base(message)
        {
        }

        public ThrottleException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public RetryParams RetryParams { get; set; }
    }
}
