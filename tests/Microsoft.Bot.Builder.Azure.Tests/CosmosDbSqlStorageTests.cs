// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.Documents.Client;
using Microsoft.Bot.Builder.Core.Extensions;
using Microsoft.Bot.Builder.Core.Extensions.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Azure.Tests
{
    [TestClass]
    [TestCategory("Storage")]
    [TestCategory("Storage - CosmosDB SQL")]
    public class CosmosDbSqlStorageTests : StorageBaseTests
    {
        private const string CosmosServiceEndpoint = "https://localhost:8081";
        private const string CosmosAuthKey = "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==";
        private const string CosmosDatabaseName = "BotSqlStorage";
        private const string CosmosCollectionName = "botstorage";

        private static string emulatorPath = Environment.ExpandEnvironmentVariables(@"%ProgramFiles%\Azure Cosmos DB Emulator\CosmosDB.Emulator.exe");
        private const string noEmulatorMessage = "This test requires CosmosDB Emulator! go to https://aka.ms/documentdb-emulator-docs to download and install.";
        private static Lazy<bool> hasEmulator = new Lazy<bool>(() =>
        {
            if (File.Exists(emulatorPath))
            {
                Process p = new Process();
                p.StartInfo.UseShellExecute = true;
                p.StartInfo.FileName = emulatorPath;
                p.StartInfo.Arguments = "/GetStatus";
                p.Start();
                p.WaitForExit();
                return p.ExitCode == 2;
            }

            return false;
        });

        private IStorage storage;

        [TestInitialize]
        public void TestInit()
        {
            if (hasEmulator.Value)
            {
                storage = new CosmosDbSqlStorage(new Uri(CosmosServiceEndpoint), CosmosAuthKey, CosmosDatabaseName, CosmosCollectionName);
            }
        }

        [TestCleanup]
        public async Task TestCleanUp()
        {
            if (storage != null)
            {
                var client = new DocumentClient(new Uri(CosmosServiceEndpoint), CosmosAuthKey);
                await client.DeleteDatabaseAsync(UriFactory.CreateDatabaseUri(CosmosDatabaseName)).ConfigureAwait(false);
            }
        }

        // NOTE: THESE TESTS REQUIRE THAT THE COSMOS DB EMULATOR IS INSTALLED AND STARTED !!!!!!!!!!!!!!!!!
        [TestMethod]
        public async Task DocumentDb_CreateObjectTest()
        {
            if (CheckEmulator())
            {
                await base._createObjectTest(storage);
            }
        }

        // NOTE: THESE TESTS REQUIRE THAT THE COSMOS DB EMULATOR IS INSTALLED AND STARTED !!!!!!!!!!!!!!!!!
        [TestMethod]
        public async Task DocumentDb_ReadUnknownTest()
        {
            if (CheckEmulator())
            {
                await base._readUnknownTest(storage);
            }
        }

        // NOTE: THESE TESTS REQUIRE THAT THE COSMOS DB EMULATOR IS INSTALLED AND STARTED !!!!!!!!!!!!!!!!!
        [TestMethod]
        public async Task DocumentDb_UpdateObjectTest()
        {
            if (CheckEmulator())
            {
                await base._updateObjectTest(storage);
            }
        }

        // NOTE: THESE TESTS REQUIRE THAT THE COSMOS DB EMULATOR IS INSTALLED AND STARTED !!!!!!!!!!!!!!!!!
        [TestMethod]
        public async Task DocumentDb_DeleteObjectTest()
        {
            if (CheckEmulator())
            {
                await base._deleteObjectTest(storage);
            }
        }

        // NOTE: THESE TESTS REQUIRE THAT THE COSMOS DB EMULATOR IS INSTALLED AND STARTED !!!!!!!!!!!!!!!!!
        [TestMethod]
        public async Task DocumentDb_HandleCrazyKeys()
        {
            if (CheckEmulator())
            {
                await base._handleCrazyKeys(storage);
            }
        }

        [TestMethod]
        public async Task DocumentDb_TypedSerialization()
        {
            if (CheckEmulator())
            {
                await base._typedSerialization(this.storage);
            }
        }

        public bool CheckEmulator()
        {
            if (!hasEmulator.Value)
                Debug.WriteLine(noEmulatorMessage);
            if (Debugger.IsAttached)
                Assert.IsTrue(hasEmulator.Value, noEmulatorMessage);

            return hasEmulator.Value;
        }
    }
}
