// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Builder.Dialogs.Debugging.Identifiers;
using Xunit;

namespace Microsoft.Bot.Builder.Dialogs.Debugging.Tests.Identifiers
{
    public sealed class IdentifierMutexTests
    {
        [Fact]
        public void IdentifierMutex_Items()
        {
            IIdentifier<string> mutexIdent = new IdentifierMutex<string>(new Identifier<string>());

            var items = mutexIdent.Items;

            Assert.NotNull(items);
        }

        [Fact]
        public void IdentifierMutex_Add()
        {
            IIdentifier<string> mutexIdent = new IdentifierMutex<string>(new Identifier<string>());

            var code = mutexIdent.Add("item");

            Assert.Equal(1ul, code);
        }

        [Fact]
        public void IdentifierMutex_TryGetValue()
        {
            IIdentifier<string> mutexIdent = new IdentifierMutex<string>(new Identifier<string>());

            var code = mutexIdent.Add("item");
            mutexIdent.TryGetValue("item", out var result);

            Assert.Equal(code, result);

            mutexIdent.TryGetValue(code, out var item);
            
            Assert.Equal("item", item);
        }

        [Fact]
        public void IdentifierMutex_Remove()
        {
            IIdentifier<string> mutexIdent = new IdentifierMutex<string>(new Identifier<string>());

            var code = mutexIdent.Add("item");
            Assert.True(mutexIdent.TryGetValue(code, out var item));

            mutexIdent.Remove(item);
            Assert.False(mutexIdent.TryGetValue(code, out _));
        }

        [Fact]
        public void IdentifierMutex_Clear()
        {
            IIdentifier<string> mutexIdent = new IdentifierMutex<string>(new Identifier<string>());

            var code = mutexIdent.Add("item");
            Assert.True(mutexIdent.TryGetValue(code, out var item));

            mutexIdent.Clear();
            Assert.False(mutexIdent.TryGetValue(code, out _));
        }

        [Fact]
        public void IdentifierMutex_GetEnumerator()
        {
            IIdentifier<string> mutexIdent = new IdentifierMutex<string>(new Identifier<string>());

            var code1 = mutexIdent.Add("item");

            Assert.True(mutexIdent.TryGetValue(code1, out _));

            Assert.NotNull(mutexIdent.GetEnumerator());
        }
    }
}
