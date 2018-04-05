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
        // Endpoint and Authkey for the CosmosDB Emulator running locally
        private const string CosmosServiceEndpoint = "https://localhost:8081";
        private const string CosmosAuthKey = "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==";
        private const string CosmosDatabaseName = "BotSqlStorage";
        private const string CosmosCollectionName = "botstorage";

        private static string _emulatorPath = Environment.ExpandEnvironmentVariables(@"%ProgramFiles%\Azure Cosmos DB Emulator\CosmosDB.Emulator.exe");
        private const string _noEmulatorMessage = "This test requires CosmosDB Emulator! go to https://aka.ms/documentdb-emulator-docs to download and install.";
        private static Lazy<bool> _hasEmulator = new Lazy<bool>(() =>
        {
            if (File.Exists(_emulatorPath))
            {
                Process p = new Process();
                p.StartInfo.UseShellExecute = true;
                p.StartInfo.FileName = _emulatorPath;
                p.StartInfo.Arguments = "/GetStatus";
                p.Start();
                p.WaitForExit();
                return p.ExitCode == 2;
            }

            return false;
        });

        private IStorage _storage;

        [TestInitialize]
        public void TestInit()
        {
            if (_hasEmulator.Value)
            {
                _storage = new CosmosDbSqlStorage(new Uri(CosmosServiceEndpoint), CosmosAuthKey, CosmosDatabaseName, CosmosCollectionName);
            }
        }

        [TestCleanup]
        public async Task TestCleanUp()
        {
            if (_storage != null)
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
                await base._createObjectTest(_storage);
            }
        }

        // NOTE: THESE TESTS REQUIRE THAT THE COSMOS DB EMULATOR IS INSTALLED AND STARTED !!!!!!!!!!!!!!!!!
        [TestMethod]
        public async Task DocumentDb_ReadUnknownTest()
        {
            if (CheckEmulator())
            {
                await base._readUnknownTest(_storage);
            }
        }

        // NOTE: THESE TESTS REQUIRE THAT THE COSMOS DB EMULATOR IS INSTALLED AND STARTED !!!!!!!!!!!!!!!!!
        [TestMethod]
        public async Task DocumentDb_UpdateObjectTest()
        {
            if (CheckEmulator())
            {
                await base._updateObjectTest(_storage);
            }
        }

        // NOTE: THESE TESTS REQUIRE THAT THE COSMOS DB EMULATOR IS INSTALLED AND STARTED !!!!!!!!!!!!!!!!!
        [TestMethod]
        public async Task DocumentDb_DeleteObjectTest()
        {
            if (CheckEmulator())
            {
                await base._deleteObjectTest(_storage);
            }
        }

        // NOTE: THESE TESTS REQUIRE THAT THE COSMOS DB EMULATOR IS INSTALLED AND STARTED !!!!!!!!!!!!!!!!!
        [TestMethod]
        public async Task DocumentDb_HandleCrazyKeys()
        {
            if (CheckEmulator())
            {
                await base._handleCrazyKeys(_storage);
            }
        }

        // NOTE: THESE TESTS REQUIRE THAT THE COSMOS DB EMULATOR IS INSTALLED AND STARTED !!!!!!!!!!!!!!!!!
        [TestMethod]
        public async Task DocumentDb_TypedSerialization()
        {
            if (CheckEmulator())
            {
                await base._typedSerialization(_storage);
            }
        }

        // NOTE: THESE TESTS REQUIRE THAT THE COSMOS DB EMULATOR IS INSTALLED AND STARTED !!!!!!!!!!!!!!!!!
        [TestMethod]
        public void DocumentDb_ConnectionPolicyConfiguratorShouldBeCalled()
        {
            if (CheckEmulator())
            {
                ConnectionPolicy policyRef = null;
                new CosmosDbSqlStorage(new Uri(CosmosServiceEndpoint), CosmosAuthKey, CosmosDatabaseName, CosmosCollectionName, (ConnectionPolicy policy) => policyRef = policy);

                Assert.IsNotNull(policyRef, "ConnectionPolicy configurator was not called.");
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task DocumentDb_ReadingEmptyKeys_Throws()
        {
            if (CheckEmulator())
            {
                await _storage.Read();
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task DocumentDb_WrittingNullStoreItems_Throws()
        {
            if (CheckEmulator())
            {
                await _storage.Write(null);
            }
        }

        public bool CheckEmulator()
        {
            if (!_hasEmulator.Value)
                Debug.WriteLine(_noEmulatorMessage);
            if (Debugger.IsAttached)
                Assert.IsTrue(_hasEmulator.Value, _noEmulatorMessage);

            return _hasEmulator.Value;
        }
    }
}
