// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Schema.Teams;
using Xunit;

namespace Microsoft.Bot.Schema.Tests.Teams
{
    public class MessagingExtensionQueryOptionsTests
    {
        [Fact]
        public void MessagingExtensionQueryOptionsInits()
        {
            var skip = 0;
            var count = 2;

            var msgExtQueryOptions = new MessagingExtensionQueryOptions(skip, count);

            Assert.NotNull(msgExtQueryOptions);
            Assert.IsType<MessagingExtensionQueryOptions>(msgExtQueryOptions);
            Assert.Equal(skip, msgExtQueryOptions.Skip);
            Assert.Equal(count, msgExtQueryOptions.Count);
        }
        
        [Fact]
        public void MessagingExtensionQueryOptionsInitsWithNoArgs()
        {
            var msgExtQueryOptions = new MessagingExtensionQueryOptions();

            Assert.NotNull(msgExtQueryOptions);
            Assert.IsType<MessagingExtensionQueryOptions>(msgExtQueryOptions);
        }
    }
}
