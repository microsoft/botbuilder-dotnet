// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Xunit;

namespace Microsoft.Bot.Schema.Tests
{
    public class FactTests
    {
        [Fact]
        public void FactInits()
        {
            var key = "key";
            var value = "value";

            var fact = new Fact(key, value);

            Assert.NotNull(fact);
            Assert.IsType<Fact>(fact);
            Assert.Equal(key, fact.Key);
            Assert.Equal(value, fact.Value);
        }

        [Fact]
        public void FactInitsWithNoArgs()
        {
            var fact = new Fact();

            Assert.NotNull(fact);
            Assert.IsType<Fact>(fact);
        }
    }
}
