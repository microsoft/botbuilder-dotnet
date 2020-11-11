using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Xunit;
using Xunit.Sdk;

namespace Microsoft.Bot.Configuration.Tests
{
    public class ConnectionTests
    {
        private readonly string testBotFileName = NormalizePath(@"..\..\..\test.bot");

        public static string NormalizePath(string ambiguousPath)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                // map linux/mac sep -> windows
                return ambiguousPath.Replace("/", "\\");
            }
            else
            {
                // map windows sep -> linux/mac
                return ambiguousPath.Replace("\\", "/");
            }
        }

        [Fact]
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
                Assert.DoesNotContain(service.Id, hashset);
                hashset.Add(service.Id);
            }
        }

        [Fact]
        public async Task FindServices()
        {
            var config = await BotConfiguration.LoadAsync(testBotFileName);
            Assert.NotNull(config.FindServiceByNameOrId("3"));
            Assert.NotNull(config.FindServiceByNameOrId("testInsights"));
            Assert.NotNull(config.FindService("3"));
            Assert.Null(config.FindService("testInsights"));

            var service = config.FindServiceByNameOrId<GenericService>("testAbs");
            Assert.NotNull(service);
            Assert.Equal("12", service.Id);

            Assert.Null(config.FindServiceByNameOrId<CosmosDbService>("testAbs"));
        }

        [Fact]
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

            Assert.Empty(config2.Services);
        }

        [Fact]
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

            Assert.Empty(config2.Services);
        }

        [Fact]
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

            Assert.Empty(config2.Services);
        }

        [Fact]
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
            Assert.True(duplicateServices.Length > 1, "Should have at least two services with this name.");

            var botService = config2.DisconnectServiceByNameOrId<BotService>(name);
            Assert.NotNull(botService);

            // Make sure this operation is not order dependent.
            config2.ConnectService(botService);
            var genericService = config2.DisconnectServiceByNameOrId<GenericService>(name);
            Assert.NotNull(genericService);
        }

        [Fact]
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

            Assert.True(duplicatedNames.Count > 0, "The config file should have at least one duplicated service name.");
            foreach (var name in uniqueNames)
            {
                config2.DisconnectServiceByNameOrId(name);
            }

            Assert.Equal(config2.Services.Count, duplicatedNames.Count);
            foreach (var name in duplicatedNames)
            {
                config2.DisconnectServiceByNameOrId(name);
            }

            Assert.Empty(config2.Services);
        }

        [Fact]
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
                throw new XunitException("generic failed with empty values");
            }

            try
            {
                var file = new FileService();
                file.Encrypt(secret);
                file.Decrypt(secret);
            }
            catch (Exception)
            {
                throw new XunitException("file failed with empty values");
            }

            try
            {
                var luis = new LuisService();
                luis.Encrypt(secret);
                luis.Decrypt(secret);
            }
            catch (Exception)
            {
                throw new XunitException("luis failed with empty values");
            }

            try
            {
                var dispatch = new DispatchService();
                dispatch.Encrypt(secret);
                dispatch.Decrypt(secret);
            }
            catch (Exception)
            {
                throw new XunitException("dispatch failed with empty values");
            }

            try
            {
                var insights = new AppInsightsService();
                insights.Encrypt(secret);
                insights.Decrypt(secret);
            }
            catch (Exception)
            {
                throw new XunitException("insights failed with empty values");
            }

            try
            {
                var bot = new BotService();
                bot.Encrypt(secret);
                bot.Decrypt(secret);
            }
            catch (Exception)
            {
                throw new XunitException("bot failed with empty values");
            }

            try
            {
                var cosmos = new CosmosDbService();
                cosmos.Encrypt(secret);
                cosmos.Decrypt(secret);
            }
            catch (Exception)
            {
                throw new XunitException("cosmos failed with empty values");
            }

            try
            {
                var qna = new QnAMakerService();
                qna.Encrypt(secret);
                qna.Decrypt(secret);
            }
            catch (Exception)
            {
                throw new XunitException("qna failed with empty values");
            }

            try
            {
                var blob = new BlobStorageService();
                blob.Encrypt(secret);
                blob.Decrypt(secret);
            }
            catch (Exception)
            {
                throw new XunitException("blob failed with empty values");
            }

            try
            {
                var endpoint = new EndpointService();
                endpoint.Encrypt(secret);
                endpoint.Decrypt(secret);
            }
            catch (Exception)
            {
                throw new XunitException("endpoint failed with empty values");
            }
        }

        [Fact]
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
                throw new XunitException("generic failed with empty values");
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
                throw new XunitException("file failed with empty values");
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
                throw new XunitException("luis failed with empty values");
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
                throw new XunitException("dispatch failed with empty values");
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
                throw new XunitException("insights failed with empty values");
            }

            try
            {
                var bot = new BotService();
                bot.Encrypt(secret);
                bot.Decrypt(secret);
            }
            catch (Exception)
            {
                throw new XunitException("bot failed with empty values");
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
                throw new XunitException("cosmos failed with empty values");
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
                throw new XunitException("qna failed with empty values");
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
                throw new XunitException("blob failed with empty values");
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
                throw new XunitException("endpoint failed with empty values");
            }
        }

        [Fact]
        public void TestLuisEndpoint()
        {
            var luisApp = new LuisService() { Region = "westus" };
            Assert.Equal($"https://{luisApp.Region}.api.cognitive.microsoft.com", luisApp.GetEndpoint());
        }
    }
}
