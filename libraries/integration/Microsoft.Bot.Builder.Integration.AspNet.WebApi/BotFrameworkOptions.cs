// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Builder.Middleware;
using Microsoft.Bot.Connector.Authentication;
using System.Collections.Generic;

namespace Microsoft.Bot.Builder.Integration.AspNet.WebApi
{
    public class BotFrameworkOptions
    {
        private readonly List<IMiddleware> _middleware;

        public BotFrameworkOptions()
        {
            RouteBaseUrl = "api/";
            _middleware = new List<IMiddleware>();
        }

        public string RouteBaseUrl { get; set; }
        public ICredentialProvider CredentialProvider { get; set; }
        public List<IMiddleware> Middleware { get => _middleware; }
    }
}