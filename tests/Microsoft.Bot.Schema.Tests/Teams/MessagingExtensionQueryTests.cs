// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using Microsoft.Bot.Schema.Teams;
using Xunit;

namespace Microsoft.Bot.Schema.Tests.Teams
{
    public class MessagingExtensionQueryTests
    {
        [Fact]
        public void MessagingExtensionQueryInits()
        {
            var commandId = "commandId123";
            var parameters = new List<MessagingExtensionParameter>() { new MessagingExtensionParameter("pandaCount", 5) };
            var queryOptions = new MessagingExtensionQueryOptions(0, 1);
            var state = "secureAuthStateValue123";

            var msgExtQuery = new MessagingExtensionQuery(commandId, parameters, queryOptions, state);

            Assert.NotNull(msgExtQuery);
            Assert.IsType<MessagingExtensionQuery>(msgExtQuery);
            Assert.Equal(commandId, msgExtQuery.CommandId);
            Assert.Equal(parameters, msgExtQuery.Parameters);
            Assert.Equal(queryOptions, msgExtQuery.QueryOptions);
            Assert.Equal(state, msgExtQuery.State);
        }
        
        [Fact]
        public void MessagingExtensionQueryInitsWithNoArgs()
        {
            var msgExtQuery = new MessagingExtensionQuery();

            Assert.NotNull(msgExtQuery);
            Assert.IsType<MessagingExtensionQuery>(msgExtQuery);
        }
    }
}
