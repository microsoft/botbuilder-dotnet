using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Configuration.Tests
{
    [TestClass]
    public class ConnectionTests
    {
        private string testBotFileName = NormalizePath(@"..\..\..\test.bot");

        public static string NormalizePath(string ambigiousPath)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                // map linux/mac sep -> windows
                return ambigiousPath.Replace("/", "\\");
            }
            else
            {
                // map windows sep -> linux/mac
                return ambigiousPath.Replace("\\", "/");
            }
        }

        [TestMethod]
        public async Task ConnectAssignsUniqueIds()
        {
            var config = await BotConfiguration.LoadAsync(testBotFileName);
            var config2 = new BotConfiguration();
            foreach (var service in config.Services)
            {
                service.Id = string.Empty;
                config2.ConnectService(service);
            }

            var hashset = new HashSet<string>();
            foreach (var service in config2.Services)
            {
                Assert.IsFalse(hashset.Contains(service.Id), "the id assigned is not unique for the collection");
                hashset.Add(service.Id);
            }
        }

        [TestMethod]
        public async Task FindServices()
        {
            var config = await BotConfiguration.LoadAsync(testBotFileName);
            Assert.IsNotNull(config.FindServiceByNameOrId("3"), "Should find by id");
            Assert.IsNotNull(config.FindServiceByNameOrId("testInsights"), "Should find by name");
            Assert.IsNotNull(config.FindService("3"), "Should find by id");
            Assert.IsNull(config.FindService("testInsights"), "Should not find by name ");

            var service = config.FindServiceByNameOrId<GenericService>("testAbs");
            Assert.IsNotNull(service, "Should find a service with this type and name.");
            Assert.IsTrue(service.Id.Equals("12"), "Should find the correct service.");

            Assert.IsNull(
                config.FindServiceByNameOrId<CosmosDbService>("testAbs"),
                "Should not find a service of this type and name.");
        }

        [TestMethod]
        public async Task DisconnectServicesById()
        {
            var config = await BotConfiguration.LoadAsync(testBotFileName);
            var config2 = new BotConfiguration();
            foreach (var service in config.Services)
            {
                config2.ConnectService(service);
            }

            var servicesIds = config2.Services.Select(s => s.Id).ToArray();

            foreach (var key in servicesIds)
            {
                config2.DisconnectService(key);
            }

            Assert.AreEqual(config2.Services.Count, 0, "didn't remove all services");
        }

        [TestMethod]
        public async Task DisconnectServicesByNameOrId_UsingId()
        {
            var config = await BotConfiguration.LoadAsync(testBotFileName);
            var config2 = new BotConfiguration();
            foreach (var service in config.Services)
            {
                config2.ConnectService(service);
            }

            var servicesIds = config2.Services.Select(s => s.Id).ToArray();

            foreach (var id in servicesIds)
            {
                config2.DisconnectServiceByNameOrId(id);
            }

            Assert.AreEqual(config2.Services.Count, 0, "didn't remove all services");
        }

        [TestMethod]
        public async Task DisconnectByNameOrId_UsingName()
        {
            var config = await BotConfiguration.LoadAsync(testBotFileName);
            var config2 = new BotConfiguration();
            foreach (var service in config.Services)
            {
                config2.ConnectService(service);
            }

            var serviceNames = config2.Services.Select(s => s.Name).ToArray();

            foreach (var name in serviceNames)
            {
                config2.DisconnectServiceByNameOrId(name);
            }

            Assert.AreEqual(config2.Services.Count, 0, "didn't remove all services");
        }

        [TestMethod]
        public async Task DisconnectService_UsingNameAndType()
        {
            var config = await BotConfiguration.LoadAsync(testBotFileName);
            var config2 = new BotConfiguration();
            foreach (var service in config.Services)
            {
                config2.ConnectService(service);
            }

            // Choose a duplicated service and remove the correct one.
            // We should have at least an ABS and generic service with the name "testAbs".
            const string name = "testAbs";
            var duplicateServices = config2.Services.Where(s => s.Name.Equals(name)).ToArray();
            Assert.IsTrue(duplicateServices.Length > 1, "Should have at least two services with this name.");

            var botService = config2.DisconnectServiceByNameOrId<BotService>(name);
            Assert.IsNotNull(botService, "Should have removed an ABS service.");

            // Make sure this operation is not order dependent.
            config2.ConnectService(botService);
            var genericService = config2.DisconnectServiceByNameOrId<GenericService>(name);
            Assert.IsNotNull(genericService, "Should have removed a generic service.");
        }

        [TestMethod]
        public async Task DisconnectByNameOrId_UsingName_WithDuplicates()
        {
            // We have a least one duplicate name in the config.
            var config = await BotConfiguration.LoadAsync(testBotFileName);
            var config2 = new BotConfiguration();
            var uniqueNames = new List<string>();
            var duplicatedNames = new List<string>();
            foreach (var service in config.Services)
            {
                config2.ConnectService(service);
                var name = service.Name;
                if (uniqueNames.Contains(name))
                {
                    duplicatedNames.Add(name);
                }
                else
                {
                    uniqueNames.Add(name);
                }
            }

            Assert.IsTrue(duplicatedNames.Count > 0, "The config file should have at least one duplicated service name.");
            foreach (var name in uniqueNames)
            {
                config2.DisconnectServiceByNameOrId(name);
            }

            Assert.AreEqual(config2.Services.Count, duplicatedNames.Count, "Extra services (with a duplicated name) should still be connected.");
            foreach (var name in duplicatedNames)
            {
                config2.DisconnectServiceByNameOrId(name);
            }

            Assert.AreEqual(config2.Services.Count, 0, "Didn't remove remaining services.");
        }

        [TestMethod]
        public void EncryptWithNullPropertiesOK()
        {
            // all of these objects should have null properties, this should not cause secret to blow up
            var secret = Guid.NewGuid().ToString("n");

            try
            {
                var generic = new GenericService();
                generic.Encrypt(secret);
                generic.Decrypt(secret);
            }
            catch (Exception)
            {
                Assert.Fail("generic failed with empty values");
            }

            try
            {
                var file = new FileService();
                file.Encrypt(secret);
                file.Decrypt(secret);
            }
            catch (Exception)
            {
                Assert.Fail("file failed with empty values");
            }

            try
            {
                var luis = new LuisService();
                luis.Encrypt(secret);
                luis.Decrypt(secret);
            }
            catch (Exception)
            {
                Assert.Fail("luis failed with empty values");
            }

            try
            {
                var dispatch = new DispatchService();
                dispatch.Encrypt(secret);
                dispatch.Decrypt(secret);
            }
            catch (Exception)
            {
                Assert.Fail("dispatch failed with empty values");
            }

            try
            {
                var insights = new AppInsightsService();
                insights.Encrypt(secret);
                insights.Decrypt(secret);
            }
            catch (Exception)
            {
                Assert.Fail("insights failed with empty values");
            }

            try
            {
                var bot = new BotService();
                bot.Encrypt(secret);
                bot.Decrypt(secret);
            }
            catch (Exception)
            {
                Assert.Fail("bot failed with empty values");
            }

            try
            {
                var cosmos = new CosmosDbService();
                cosmos.Encrypt(secret);
                cosmos.Decrypt(secret);
            }
            catch (Exception)
            {
                Assert.Fail("cosmos failed with empty values");
            }

            try
            {
                var qna = new QnAMakerService();
                qna.Encrypt(secret);
                qna.Decrypt(secret);
            }
            catch (Exception)
            {
                Assert.Fail("qna failed with empty values");
            }

            try
            {
                var blob = new BlobStorageService();
                blob.Encrypt(secret);
                blob.Decrypt(secret);
            }
            catch (Exception)
            {
                Assert.Fail("blob failed with empty values");
            }

            try
            {
                var endpoint = new EndpointService();
                endpoint.Encrypt(secret);
                endpoint.Decrypt(secret);
            }
            catch (Exception)
            {
                Assert.Fail("endpoint failed with empty values");
            }
        }

        [TestMethod]
        public void EncryptWithEmptyPropertiesOK()
        {
            // all of these objects should have null properties, this should not cause secret to blow up
            var secret = Guid.NewGuid().ToString("n");

            try
            {
                var generic = new GenericService();
                generic.Configuration["test"] = string.Empty;
                generic.Encrypt(secret);
                generic.Decrypt(secret);
            }
            catch (Exception)
            {
                Assert.Fail("generic failed with empty values");
            }

            try
            {
                var file = new FileService
                {
                    Path = string.Empty,
                };
                file.Encrypt(secret);
                file.Decrypt(secret);
            }
            catch (Exception)
            {
                Assert.Fail("file failed with empty values");
            }

            try
            {
                var luis = new LuisService
                {
                    SubscriptionKey = string.Empty,
                };
                luis.Encrypt(secret);
                luis.Decrypt(secret);
            }
            catch (Exception)
            {
                Assert.Fail("luis failed with empty values");
            }

            try
            {
                var dispatch = new DispatchService
                {
                    SubscriptionKey = string.Empty,
                };
                dispatch.Encrypt(secret);
                dispatch.Decrypt(secret);
            }
            catch (Exception)
            {
                Assert.Fail("dispatch failed with empty values");
            }

            try
            {
                var insights = new AppInsightsService
                {
                    InstrumentationKey = string.Empty,
                };
                insights.Encrypt(secret);
                insights.Decrypt(secret);
            }
            catch (Exception)
            {
                Assert.Fail("insights failed with empty values");
            }

            try
            {
                var bot = new BotService();
                bot.Encrypt(secret);
                bot.Decrypt(secret);
            }
            catch (Exception)
            {
                Assert.Fail("bot failed with empty values");
            }

            try
            {
                var cosmos = new CosmosDbService
                {
                    Key = string.Empty,
                };
                cosmos.Encrypt(secret);
                cosmos.Decrypt(secret);
            }
            catch (Exception)
            {
                Assert.Fail("cosmos failed with empty values");
            }

            try
            {
                var qna = new QnAMakerService
                {
                    SubscriptionKey = string.Empty,
                };
                qna.Encrypt(secret);
                qna.Decrypt(secret);
            }
            catch (Exception)
            {
                Assert.Fail("qna failed with empty values");
            }

            try
            {
                var blob = new BlobStorageService
                {
                    ConnectionString = string.Empty,
                };
                blob.Encrypt(secret);
                blob.Decrypt(secret);
            }
            catch (Exception)
            {
                Assert.Fail("blob failed with empty values");
            }

            try
            {
                var endpoint = new EndpointService
                {
                    AppPassword = string.Empty,
                };
                endpoint.Encrypt(secret);
                endpoint.Decrypt(secret);
            }
            catch (Exception)
            {
                Assert.Fail("endpoint failed with empty values");
            }
        }

        [TestMethod]
        public void TestLuisEndpoint()
        {
            var luisApp = new LuisService() { Region = "westus" };
            Assert.AreEqual(luisApp.GetEndpoint(), $"https://{luisApp.Region}.api.cognitive.microsoft.com");
        }
    }
}
