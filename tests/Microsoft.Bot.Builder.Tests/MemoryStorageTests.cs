// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading.Tasks;
using Xunit;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Tests
{
    [TestClass]
    [TestCategory("Storage")]
    [TestCategory("Storage - Memory")]
    public class MemoryStorageTests : StorageBaseTests
    {
        private IStorage storage;

        public MemoryStorageTests()
        {
        }

        [TestInitialize]
        public void Initialize()
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
            Assert.ThrowsException<ArgumentNullException>(() =>
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
