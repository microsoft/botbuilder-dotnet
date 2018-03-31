// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;

namespace Microsoft.Bot.Builder
{
    public class BotFrameworkAuthenticationContext
    {
        public ClaimsIdentity ClaimsIdentity { get; set; }

        public string BotAppId { get; set; }

        public string ServiceUrl { get; set; }

        public bool IsEmulator { get; set; }

        public bool? IsAuthenticated { get; set; }
    }
}
