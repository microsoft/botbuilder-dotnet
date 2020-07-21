// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;

namespace Microsoft.Bot.Connector.Authentication
{
    public class AuthenticatorResult
    {
        public string AccessToken { get; set;  }

        public DateTimeOffset ExpiresOn { get; set; }
    }
}
