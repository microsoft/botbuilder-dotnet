// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Rest.TransientFaultHandling;

namespace Microsoft.Bot.Builder.Integration.AspNet.Core
{
    public class BotFrameworkOptions
    {
        private readonly List<IMiddleware> _middleware;

        public BotFrameworkOptions()
        {
            _middleware = new List<IMiddleware>();
        }

        public ICredentialProvider CredentialProvider { get; set; }
        public IList<IMiddleware> Middleware { get => _middleware; }
        public bool EnableProactiveMessages { get; set; }

        /// <summary>
        /// Gets or sets the retry policy to retry operations in case of errors from Bot Framework.
        /// </summary>
        public RetryPolicy ConnectorClientRetryPolicy { get; set; }
    }
}
