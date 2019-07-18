// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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

        [TestMethod]
        public async Task MemoryStorage_CreateObjectTest()
        {
            await CreateObjectTest(storage);
        }

        [TestMethod]
        public async Task MemoryStorage_ReadUnknownTest()
        {
            await ReadUnknownTest(storage);
        }

        [TestMethod]
        public async Task MemoryStorage_UpdateObjectTest()
        {
            await UpdateObjectTest(storage);
        }

        [TestMethod]
        public async Task MemoryStorage_DeleteObjectTest()
        {
            await DeleteObjectTest(storage);
        }

        [TestMethod]
        public async Task MemoryStorage_HandleCrazyKeys()
        {
            await HandleCrazyKeys(storage);
        }
    }
}
