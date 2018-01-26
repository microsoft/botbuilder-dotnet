// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Storage;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Tests
{
    [TestClass]
    [TestCategory("Storage")]
    [TestCategory("Storage - File")]
    public class Storage_FileTests : Storage_BaseTests, IStorageTests
    {
        private IStorage storage;
        public Storage_FileTests() { }

        [TestInitialize]
        public void initialize()
        {
            string path = Path.Combine(Environment.GetEnvironmentVariable("temp"), "FileStorageTest");
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            foreach (var file in Directory.GetFiles(path))
                File.Delete(file);
            storage = new FileStorage(path);
        }

        [TestMethod]
        public async Task CreateObjectTest()
        {
            await base._createObjectTest(this.storage);
        }

        [TestMethod]
        public async Task ReadUnknownTest()
        {
            await base._readUnknownTest(this.storage);
        }

        [TestMethod]
        public async Task UpdateObjectTest()
        {
            await base._updateObjectTest(this.storage);
        }

        [TestMethod]
        public async Task DeleteObjectTest()
        {
            await base._deleteObjectTest(this.storage);
        }

        [TestMethod]
        public async Task HandleCrazyKeys()
        {
            await base._handleCrazyKeys(this.storage);
        }

        [TestMethod]
        public async Task TypedSerialization()
        {
            await base._typedSerialization(this.storage);
        }
    }
}
