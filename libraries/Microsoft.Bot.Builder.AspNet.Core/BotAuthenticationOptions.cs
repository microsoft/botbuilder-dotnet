// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Microsoft.Bot.Builder.AspNet.Core
{
    /// <summary>
    /// Bot authentication options.
    /// </summary>
    /// <seealso cref="JwtBearerOptions" />
    public class BotAuthenticationOptions : JwtBearerOptions
    {
        /// <summary>
        /// Gets or sets the HTTP client to get endorsements.
        /// </summary>
        public HttpClient HttpClient { get; set; }
    }
}
