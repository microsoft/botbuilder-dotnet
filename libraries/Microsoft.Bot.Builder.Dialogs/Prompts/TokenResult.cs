// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Dialogs
{
    public class TokenResult : PromptResult
    {
        public TokenResponse TokenResponse
        {
            get { return GetProperty<TokenResponse>(nameof(TokenResponse)); }
            set { this[nameof(TokenResponse)] = value; }
        }
    }
}