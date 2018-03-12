// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Core.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Core.Extensions.Tests
{
    [TestClass]
    public class FileStorageTests
    {
        [TestClass]
        [TestCategory("Storage")]
        [TestCategory("Storage - File")]
        public class Storage_FileTests : StorageBaseTests
        {
            private IStorage storage;
            public Storage_FileTests() { }

            [TestInitialize]
            public void Initialize()
            {
                string path = Path.Combine(Environment.GetEnvironmentVariable("temp"), "FileStorageTest");
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
                foreach (var file in Directory.GetFiles(path))
                    File.Delete(file);
                storage = new FileStorage(path);
            }

            [TestMethod]
            public async Task FileStorage_CreateObjectTest()
            {
                await base._createObjectTest(this.storage);
            }

            [TestMethod]
            public async Task FileStorage_ReadUnknownTest()
            {
                await base._readUnknownTest(this.storage);
            }

            [TestMethod]
            public async Task FileStorage_UpdateObjectTest()
            {
                await base._updateObjectTest(this.storage);
            }

            [TestMethod]
            public async Task FileStorage_DeleteObjectTest()
            {
                await base._deleteObjectTest(this.storage);
            }

            [TestMethod]
            public async Task FileStorage_HandleCrazyKeys()
            {
                await base._handleCrazyKeys(this.storage);
            }

            [TestMethod]
            public async Task FileStorage_TypedSerialization()
            {
                await base._typedSerialization(this.storage);
            }
        }
    }
}
