// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Xunit;

namespace Microsoft.Bot.Builder.Tests
{
    public class MemoryStorageTests : StorageBaseTests, IDisposable
    {
        private IStorage storage;

        public MemoryStorageTests()
        {
            storage = new MemoryStorage();
        }

        public void Dispose()
        {
            storage = new MemoryStorage();
        }

        [Fact]
        public async Task MemoryStorage_CreateObjectTest()
        {
            await CreateObjectTest(storage);
        }

        [Fact]
        public void MemoryParamTest()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new MemoryStorage((JsonSerializer)null));
        }

        [Fact]
        public async Task MemoryStorage_ReadUnknownTest()
        {
            await ReadUnknownTest(storage);
        }

        [Fact]
        public async Task MemoryStorage_UpdateObjectTest()
        {
            await UpdateObjectTest(storage);
        }

        [Fact]
        public async Task MemoryStorage_DeleteObjectTest()
        {
            await DeleteObjectTest(storage);
        }

        [Fact]
        public async Task MemoryStorage_HandleCrazyKeys()
        {
            await HandleCrazyKeys(storage);
        }

        [Fact]
        public async Task StatePersistsThroughMultiTurn_TypeNameHandlingNone()
        {
            storage = new MemoryStorage(new JsonSerializer() { TypeNameHandling = TypeNameHandling.None });
            await StatePersistsThroughMultiTurn(storage);
        }
    }
}
