// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using Microsoft.Extensions.Primitives;
using Xunit;

namespace Microsoft.Bot.Connector.Tests
{
    [Collection("Non-Parallel Collection")] // Ensure this test runs in a single-threaded context to avoid issues with static dictionary.
    public class HeaderPropagationTests
    {
        public HeaderPropagationTests()
        {
            HeaderPropagation.HeadersToPropagate = new Dictionary<string, StringValues>();
        }

        [Fact]
        public void HeaderPropagationContext_ShouldFilterHeaders()
        {
            // Arrange
            HeaderPropagation.RequestHeaders = new Dictionary<string, StringValues>
            {
                { "x-custom-header-1", new StringValues("Value-1") },
                { "x-custom-header-2", new StringValues("Value-2") },
                { "x-custom-header-3", new StringValues("Value-3") }
            };

            var headersToPropagate = new HeaderPropagationEntryCollection();

            headersToPropagate.Add("x-custom-header", "custom-value");
            headersToPropagate.Propagate("x-custom-header-1");
            headersToPropagate.Override("x-custom-header-2", "new-value");
            headersToPropagate.Append("x-custom-header-3", "extra-value");

            // Act
            var filteredHeaders = HeaderPropagation.FilterHeaders(headersToPropagate);

            // Assert
            Assert.Equal(4, filteredHeaders.Count);
            Assert.Equal("custom-value", filteredHeaders["x-custom-header"]);
            Assert.Equal("Value-1", filteredHeaders["x-custom-header-1"]);
            Assert.Equal("new-value", filteredHeaders["x-custom-header-2"]);
            Assert.Equal("Value-3,extra-value", filteredHeaders["x-custom-header-3"]);
        }

        [Fact]
        public void HeaderPropagationContext_ShouldAppendMultipleValues()
        {
            // Arrange
            HeaderPropagation.RequestHeaders = new Dictionary<string, StringValues>
            {
                { "User-Agent", new StringValues("Value-1") }
            };

            var headersToPropagate = new HeaderPropagationEntryCollection();

            headersToPropagate.Append("User-Agent", "extra-value-1");
            headersToPropagate.Append("User-Agent", "extra-value-2");

            // Act
            var filteredHeaders = HeaderPropagation.FilterHeaders(headersToPropagate);

            // Assert
            Assert.Single(filteredHeaders);
            Assert.Equal("Value-1,extra-value-1,extra-value-2", filteredHeaders["User-Agent"]);
        }

        [Fact]
        public void HeaderPropagationContext_MultipleAdd_ShouldKeepLastValue()
        {
            // Arrange
            HeaderPropagation.RequestHeaders = new Dictionary<string, StringValues>();
            
            var headersToPropagate = new HeaderPropagationEntryCollection();

            headersToPropagate.Add("x-custom-header-1", "value-1");
            headersToPropagate.Add("x-custom-header-1", "value-2");

            // Act
            var filteredHeaders = HeaderPropagation.FilterHeaders(headersToPropagate);

            // Assert
            Assert.Single(filteredHeaders);
            Assert.Equal("value-2", filteredHeaders["x-custom-header-1"]);
        }

        [Fact]
        public void HeaderPropagationContext_MultipleOverride_ShouldKeepLastValue()
        {
            // Arrange
            HeaderPropagation.RequestHeaders = new Dictionary<string, StringValues>
            {
                { "x-custom-header-1", new StringValues("Value-1") }
            };

            var headersToPropagate = new HeaderPropagationEntryCollection();
            headersToPropagate.Override("x-custom-header-1", "new-value-1");
            headersToPropagate.Override("x-custom-header-1", "new-value-2");

            // Act
            var filteredHeaders = HeaderPropagation.FilterHeaders(headersToPropagate);

            // Assert
            Assert.Single(filteredHeaders);
            Assert.Equal("new-value-2", filteredHeaders["x-custom-header-1"]);
        }
    }

    [CollectionDefinition("Non-Parallel Collection", DisableParallelization = true)]
#pragma warning disable SA1402 // File may only contain a single type
    public class NonParallelCollectionDefinition 
    {
    }
#pragma warning restore SA1402 // File may only contain a single type
}
