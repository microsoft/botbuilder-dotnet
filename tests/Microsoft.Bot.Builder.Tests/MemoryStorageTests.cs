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
        private IStorage _storage;

        public MemoryStorageTests()
        {
            _storage = new MemoryStorage();
        }

        public void Dispose()
        {
            _storage = new MemoryStorage();
        }

        [Fact]
        public async Task MemoryStorage_CreateObjectTest()
        {
            await CreateObjectTest(_storage);
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
            await ReadUnknownTest(_storage);
        }

        [Fact]
        public async Task MemoryStorage_UpdateObjectTest()
        {
            await UpdateObjectTest(_storage);
        }

        [Fact]
        public async Task MemoryStorage_DeleteObjectTest()
        {
            await DeleteObjectTest(_storage);
        }

        [Fact]
        public async Task MemoryStorage_HandleCrazyKeys()
        {
            await HandleCrazyKeys(_storage);
        }

        [Fact]
        public async Task StatePersistsThroughMultiTurn_TypeNameHandlingNone()
        {
            _storage = new MemoryStorage(new JsonSerializer() { TypeNameHandling = TypeNameHandling.None });
            await StatePersistsThroughMultiTurn(_storage);
        }
    }
}
