// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Protocols;

namespace Microsoft.Bot.Connector.Tests.Authentication
{
    public class TestConfigurationRetriever : IConfigurationRetriever<IDictionary<string, HashSet<string>>>
    {
        public IDictionary<string, HashSet<string>> EndorsementTable { get; } = new Dictionary<string, HashSet<string>>();

        public Task<IDictionary<string, HashSet<string>>> GetConfigurationAsync(string address, IDocumentRetriever retriever, CancellationToken cancel)
        {
            return Task.FromResult(EndorsementTable);
        }
    }
}
