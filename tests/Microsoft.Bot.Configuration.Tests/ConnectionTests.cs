using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Configuration.Tests
{
    [TestClass]
    public class ConnectionTests
    {
        private const string TestBotFileName = @"..\..\..\test.bot";

        [TestMethod]
        public async Task ConnectAssignsUniqueIds()
        {
            var config = await BotConfiguration.LoadAsync(TestBotFileName);
            var config2 = new BotConfiguration();
            foreach (var service in config.Services)
            {
                service.Id = "1";
                config2.ConnectService(service);
            }

            HashSet<string> hashset = new HashSet<string>();
            foreach (var service in config2.Services)
            {
                Assert.IsFalse(hashset.Contains(service.Id), "the id assigned is not unique for the collection");
                hashset.Add(service.Id);
            }
        }

        [TestMethod]
        public async Task FindServices()
        {
            var config = await BotConfiguration.LoadAsync(TestBotFileName);
            Assert.IsNotNull(config.FindServiceByNameOrId("3"), "Should find by id");
            Assert.IsNotNull(config.FindServiceByNameOrId("testInsights"), "Should find by name");
            Assert.IsNotNull(config.FindService("3"), "Should find by id");
            Assert.IsNull(config.FindService("testInsights"), "Should not find by name ");
        }

        [TestMethod]
        public async Task DisconnectServicesById()
        {
            var config = await BotConfiguration.LoadAsync(TestBotFileName);
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
            var config = await BotConfiguration.LoadAsync(TestBotFileName);
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
            var config = await BotConfiguration.LoadAsync(TestBotFileName);
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
                generic.Configuration["test"] = String.Empty;
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
                file.Path = String.Empty;
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
                luis.SubscriptionKey = String.Empty;
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
                dispatch.SubscriptionKey = String.Empty;
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
                insights.InstrumentationKey = String.Empty;
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
                cosmos.Key = String.Empty;
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
                qna.SubscriptionKey = String.Empty;
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
                blob.ConnectionString = String.Empty;
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
                endpoint.AppPassword = String.Empty;
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
