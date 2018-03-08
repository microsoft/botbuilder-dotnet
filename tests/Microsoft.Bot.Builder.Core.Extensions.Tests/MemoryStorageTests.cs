// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading.Tasks;
using Microsoft.Bot.Builder.Core.Extensions; 
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Core.Extensions.Tests
{
    [TestClass]
    [TestCategory("Storage")]
    [TestCategory("Storage - Memory")]
    public class MemoryStorageTests : StorageBaseTests
    {
        private IStorage storage;

        public MemoryStorageTests() { }

        [TestInitialize]
        public void initialize()
        {
            storage = new MemoryStorage();
        }

        [TestMethod]
        public async Task MemoryStorage_CreateObjectTest()
        {
            await base._createObjectTest(storage);
        }

        [TestMethod]
        public async Task MemoryStorage_ReadUnknownTest()
        {
            await base._readUnknownTest(storage);
        }

        [TestMethod]
        public async Task MemoryStorage_UpdateObjectTest()
        {
            await base._updateObjectTest(storage);
        }

        [TestMethod]
        public async Task MemoryStorage_DeleteObjectTest()
        {
            await base._deleteObjectTest(storage);
        }

        [TestMethod]
        public async Task MemoryStorage_HandleCrazyKeys()
        {
            await base._handleCrazyKeys(storage);
        }

        [TestMethod]
        public async Task MemoryStorage_TypedSerialization()
        {
            await base._typedSerialization(this.storage);
        }
    }
}
