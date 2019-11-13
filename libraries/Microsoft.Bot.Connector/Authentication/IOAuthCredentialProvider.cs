// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Connector.Authentication
{
    public interface IOAuthCredentialProvider
    {
        string OAuthMicrosoftAppId { get; set; }

        string OAuthMicrosoftAppPassword { get; set; }
    }
}
