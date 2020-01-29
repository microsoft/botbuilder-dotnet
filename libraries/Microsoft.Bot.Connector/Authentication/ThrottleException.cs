// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;

namespace Microsoft.Bot.Connector.Authentication
{
    public class ThrottleException : Exception
    {
        public RetryParams RetryParams { get; set; }
    }
}
