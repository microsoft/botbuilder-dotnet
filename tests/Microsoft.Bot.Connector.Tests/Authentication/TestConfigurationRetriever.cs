// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Protocols;

namespace Microsoft.Bot.Connector.Authentication.Tests
{
    public class TestConfigurationRetriever : IConfigurationRetriever<IDictionary<string, HashSet<string>>>
    {
        public Dictionary<string, HashSet<string>> EndorsementTable { get; private set; } = new Dictionary<string, HashSet<string>>();

        public async Task<IDictionary<string, HashSet<string>>> GetConfigurationAsync(string address, IDocumentRetriever retriever, CancellationToken cancel)
        {
            return EndorsementTable;
        }
    }
}
