// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Builder.Dialogs.Debugging.Identifiers;
using Xunit;

namespace Microsoft.Bot.Builder.Dialogs.Debugging.Tests.Identifiers
{
    public sealed class IdentifierCacheTests
    {
        [Fact]
        public void IdentifierCache_Items()
        {
            IIdentifier<string> identCache = new IdentifierCache<string>(new Identifier<string>(), 1);

            var items = identCache.Items;

            Assert.NotNull(items);
        }

        [Fact]
        public void IdentifierCache_Add()
        {
            IIdentifier<string> identCache = new IdentifierCache<string>(new Identifier<string>(), 1);

            var code = identCache.Add("item");

            Assert.Equal(1ul, code);
        }

        [Fact]
        public void IdentifierCache_TryGetValue()
        {
            IIdentifier<string> identCache = new IdentifierCache<string>(new Identifier<string>(), 1);

            var code = identCache.Add("item");
            identCache.TryGetValue("item", out var result);

            Assert.Equal(code, result);

            identCache.TryGetValue(code, out var item);
            
            Assert.Equal("item", item);
        }

        [Fact]
        public void IdentifierCache_Remove()
        {
            IIdentifier<string> identCache = new IdentifierCache<string>(new Identifier<string>(), 1);

            var code = identCache.Add("item");
            Assert.True(identCache.TryGetValue(code, out var item));

            identCache.Remove(item);
            Assert.False(identCache.TryGetValue(code, out _));
        }

        [Fact]
        public void IdentifierCache_Clear()
        {
            IIdentifier<string> identCache = new IdentifierCache<string>(new Identifier<string>(), 1);

            var code = identCache.Add("item");
            Assert.True(identCache.TryGetValue(code, out var item));

            identCache.Clear();
            Assert.False(identCache.TryGetValue(code, out _));
        }

        [Fact]
        public void IdentifierCache_GetEnumerator()
        {
            IIdentifier<string> identCache = new IdentifierCache<string>(new Identifier<string>(), 1);

            var code1 = identCache.Add("item");
            Assert.True(identCache.TryGetValue(code1, out _));

            Assert.NotNull(identCache.GetEnumerator());
        }
    }
}
