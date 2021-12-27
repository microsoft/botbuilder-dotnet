// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Tests.Common.Storage;
using Newtonsoft.Json;
using Xunit;

namespace Microsoft.Bot.Builder.Tests
{
    public class MemoryStorageTests : StorageTestsBase, IDisposable
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
        public async Task MemoryStorage_TestTyped()
        {
            await TestTypedObjects(new MemoryStorage(new JsonSerializer() { TypeNameHandling = TypeNameHandling.All }), expectTyped: true);
        }

        [Fact]
        public async Task MemoryStorage_TestUnTyped()
        {
            await TestTypedObjects(new MemoryStorage(new JsonSerializer() { TypeNameHandling = TypeNameHandling.None }), expectTyped: false);
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

        [Theory]
        [InlineData(TypeNameHandling.None)]
        [InlineData(TypeNameHandling.All)]
        [InlineData(TypeNameHandling.Auto)]
        [InlineData(TypeNameHandling.Objects)]
        public async Task StatePersistsThroughMultiTurn_TypeNameHandlingNone(TypeNameHandling typeNameHandling)
        {
            storage = new MemoryStorage(new JsonSerializer() { TypeNameHandling = typeNameHandling });
            await StatePersistsThroughMultiTurn(storage);
        }
    }
}
