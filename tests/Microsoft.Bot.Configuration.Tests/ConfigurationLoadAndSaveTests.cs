using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Xunit;
using Xunit.Sdk;

namespace Microsoft.Bot.Configuration.Tests
{
    public class ConfigurationLoadAndSaveTests
    {
        private const string OutputBotFileName = "save.bot";
        private readonly string testBotFileName = NormalizePath(@"..\..\..\test.bot");

        [Fact]
        public async Task DeserializeBotFile()
        {
            var config = await BotConfiguration.LoadAsync(testBotFileName);
            Assert.Equal("test", config.Name);
            Assert.Equal("test description", config.Description);
            Assert.Equal(string.Empty, config.Padlock);
            Assert.Equal(12, config.Services.Count);
            dynamic properties = config.Properties;
            Assert.True((bool)properties.extra);

            // verify types are right
            foreach (var service in config.Services)
            {
                switch (service.Type)
                {
                    case ServiceTypes.AppInsights:
                        Assert.Equal(typeof(AppInsightsService), service.GetType());
                        break;
                    case ServiceTypes.Bot:
                        Assert.Equal(typeof(BotService), service.GetType());
                        break;
                    case ServiceTypes.BlobStorage:
                        Assert.Equal(typeof(BlobStorageService), service.GetType());
                        break;
                    case ServiceTypes.CosmosDB:
                        Assert.Equal(typeof(CosmosDbService), service.GetType());
                        break;
                    case ServiceTypes.Generic:
                        Assert.Equal(typeof(GenericService), service.GetType());
                        break;
                    case ServiceTypes.Dispatch:
                        Assert.Equal(typeof(DispatchService), service.GetType());
                        break;
                    case ServiceTypes.Endpoint:
                        Assert.Equal(typeof(EndpointService), service.GetType());
                        break;
                    case ServiceTypes.File:
                        Assert.Equal(typeof(FileService), service.GetType());
                        break;
                    case ServiceTypes.Luis:
                        Assert.Equal(typeof(LuisService), service.GetType());
                        break;
                    case ServiceTypes.QnA:
                        Assert.Equal(typeof(QnAMakerService), service.GetType());
                        break;
                    case "unknown":
                        // this is cool, because we want to round-trip unknown service types for future proofing
                        break;
                    default:
                        throw new Exception("Unknown service type!");
                }
            }
        }

        [Fact]
        public async Task LoadAndSaveUnencryptedBotFile()
        {
            var config = await BotConfiguration.LoadAsync(testBotFileName);
            await config.SaveAsAsync(OutputBotFileName);

            var config2 = await BotConfiguration.LoadAsync(OutputBotFileName);

            Assert.Equal(JsonConvert.SerializeObject(config2), JsonConvert.SerializeObject(config));
        }

        [Fact]
        public void LoadAndSaveUnencryptedBotFileSync()
        {
            var config = BotConfiguration.Load(testBotFileName);
            config.SaveAs(OutputBotFileName);

            var config2 = BotConfiguration.Load(OutputBotFileName);
            Assert.Equal(JsonConvert.SerializeObject(config2), JsonConvert.SerializeObject(config));
        }

        [Fact]
        public async Task CantLoadWithoutSecret()
        {
            var secret = BotConfiguration.GenerateKey();
            var config = await BotConfiguration.LoadAsync(testBotFileName);
            await config.SaveAsAsync(OutputBotFileName, secret);

            try
            {
                await BotConfiguration.LoadAsync(OutputBotFileName);
                throw new XunitException("Load should have thrown due to no secret");
            }
            catch
            {
            }
        }

        [Fact]
        public async Task LoadFromFolderWithSecret()
        {
            var secret = BotConfiguration.GenerateKey();
            var config = await BotConfiguration.LoadAsync(testBotFileName);
            await config.SaveAsAsync(OutputBotFileName, secret);
            await BotConfiguration.LoadFromFolderAsync(".", secret);
        }

        [Fact]
        public void LoadFromFolderWithSecretSync()
        {
            var secret = BotConfiguration.GenerateKey();
            var config = BotConfiguration.Load(testBotFileName);
            config.SaveAs(OutputBotFileName, secret);
            BotConfiguration.LoadFromFolder(".", secret);
        }

        [Fact]
        public async Task FailLoadFromFolderWithNoSecret()
        {
            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                var secret = BotConfiguration.GenerateKey();
                var config = await BotConfiguration.LoadAsync(testBotFileName);
                await config.SaveAsAsync(OutputBotFileName, secret);
                await BotConfiguration.LoadFromFolderAsync(".");
            });
        }

        [Fact]
        public async Task LoadFromFolderNoSecret()
        {
            var config = await BotConfiguration.LoadAsync(testBotFileName);
            await config.SaveAsAsync(OutputBotFileName);
            await BotConfiguration.LoadFromFolderAsync(".");
        }

        [Fact]
        public void LoadFromFolderNoSecretSync()
        {
            var config = BotConfiguration.Load(testBotFileName);
            config.SaveAs(OutputBotFileName);
            BotConfiguration.LoadFromFolder(".");
        }

        [Fact]
        public async Task LoadNotExistentFile()
        {
            await Assert.ThrowsAsync<FileNotFoundException>(() => BotConfiguration.LoadAsync(NormalizePath(@"..\..\..\filedoesntexist.bot")));
        }

        [Fact]
        public async Task NullFile()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => BotConfiguration.LoadAsync(null));
        }

        [Fact]
        public async Task LoadNonExistentFolder()
        {
            await Assert.ThrowsAsync<DirectoryNotFoundException>(() => BotConfiguration.LoadFromFolderAsync(NormalizePath(@"\prettysurethisdoesnotexist")));
        }

        [Fact]
        public async Task NullFolder()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => BotConfiguration.LoadFromFolderAsync(null));
        }

        [Fact]
        public async Task CantSaveWithoutSecret()
        {
            var secret = BotConfiguration.GenerateKey();
            var config = await BotConfiguration.LoadAsync(testBotFileName);
            await config.SaveAsAsync(OutputBotFileName, secret);

            var config2 = await BotConfiguration.LoadAsync(OutputBotFileName, secret);
            try
            {
                await config2.SaveAsAsync(OutputBotFileName);
                throw new XunitException("Save() should have thrown due to no secret");
            }
            catch
            {
            }

            config2.ClearSecret();
            await config2.SaveAsAsync(OutputBotFileName, secret);
        }

        [Fact]
        public async Task LoadAndSaveEncrypted()
        {
            var secret = BotConfiguration.GenerateKey();
            var config = await BotConfiguration.LoadAsync(testBotFileName);
            Assert.Equal(string.Empty, config.Padlock);

            // save with secret
            await config.SaveAsAsync("savesecret.bot", secret);
            Assert.True(config.Padlock?.Length > 0);

            // load with secret
            var config2 = await BotConfiguration.LoadAsync("savesecret.bot", secret);
            Assert.True(config2.Padlock?.Length > 0);
            Assert.Equal(config.Padlock, config2.Padlock);

            // make sure these were decrypted
            for (var i = 0; i < config.Services.Count; i++)
            {
                Assert.Equal(JsonConvert.SerializeObject(config.Services[i]), JsonConvert.SerializeObject(config2.Services[i]));

                switch (config.Services[i].Type)
                {
                    case ServiceTypes.Bot:
                        break;

                    case ServiceTypes.AppInsights:
                        {
                            var appInsights = (AppInsightsService)config2.Services[i];
                            Assert.Contains("0000", appInsights.InstrumentationKey);
                            Assert.Equal("00000000-0000-0000-0000-000000000007", appInsights.ApplicationId);
                            Assert.Equal("testKey1", appInsights.ApiKeys["key1"]);
                            Assert.Equal("testKey2", appInsights.ApiKeys["key2"]);
                        }

                        break;

                    case ServiceTypes.BlobStorage:
                        {
                            var blobStorage = (BlobStorageService)config2.Services[i];
                            Assert.Equal("UseDevelopmentStorage=true;", blobStorage.ConnectionString);
                            Assert.Equal("testContainer", blobStorage.Container);
                        }

                        break;

                    case ServiceTypes.CosmosDB:
                        {
                            var cosmosDb = (CosmosDbService)config2.Services[i];
                            Assert.Equal("https://localhost:8081/", cosmosDb.Endpoint);
                            Assert.Equal("C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==", cosmosDb.Key);
                            Assert.Equal("testDatabase", cosmosDb.Database);
                            Assert.Equal("testCollection", cosmosDb.Collection);
                        }

                        break;

                    case ServiceTypes.Dispatch:
                        {
                            var dispatch = (DispatchService)config2.Services[i];
                            Assert.Contains("0000", dispatch.AuthoringKey);
                            Assert.Contains("0000", dispatch.SubscriptionKey);
                        }

                        break;

                    case ServiceTypes.Endpoint:
                        {
                            var endpoint = (EndpointService)config2.Services[i];
                            Assert.Equal("testpassword", endpoint.AppPassword);
                        }

                        break;

                    case ServiceTypes.File:
                        break;

                    case ServiceTypes.Luis:
                        {
                            var luis = (LuisService)config2.Services[i];
                            Assert.Contains("0000", luis.AuthoringKey);
                            Assert.Contains("0000", luis.SubscriptionKey);
                        }

                        break;

                    case ServiceTypes.QnA:
                        {
                            var qna = (QnAMakerService)config2.Services[i];
                            Assert.Contains("0000", qna.KbId);
                            Assert.Contains("0000", qna.EndpointKey);
                            Assert.Contains("0000", qna.SubscriptionKey);
                        }

                        break;

                    case ServiceTypes.Generic:
                        {
                            var generic = (GenericService)config2.Services[i];
                            Assert.Equal("https://bing.com", generic.Url);
                            Assert.Equal("testKey1", generic.Configuration["key1"]);
                            Assert.Equal("testKey2", generic.Configuration["key2"]);
                        }

                        break;

                    default:
                        break;
                }
            }

            // encrypt in memory copy
            config2.Encrypt(secret);

            // make sure these are all true
            for (var i = 0; i < config.Services.Count; i++)
            {
                switch (config.Services[i].Type)
                {
                    case ServiceTypes.AppInsights:
                        {
                            var appInsights = (AppInsightsService)config2.Services[i];
                            Assert.DoesNotContain("0000", appInsights.InstrumentationKey);
                            Assert.Equal("00000000-0000-0000-0000-000000000007", appInsights.ApplicationId);
                            Assert.NotEqual("testKey1", appInsights.ApiKeys["key1"]);
                            Assert.NotEqual("testKey2", appInsights.ApiKeys["key2"]);
                        }

                        break;

                    case ServiceTypes.BlobStorage:
                        {
                            var azureStorage = (BlobStorageService)config2.Services[i];
                            Assert.NotEqual("UseDevelopmentStorage=true;", azureStorage.ConnectionString);
                            Assert.Equal("testContainer", azureStorage.Container);
                        }

                        break;

                    case ServiceTypes.CosmosDB:
                        {
                            var cosmosdb = (CosmosDbService)config2.Services[i];
                            Assert.Equal("https://localhost:8081/", cosmosdb.Endpoint);
                            Assert.NotEqual("C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==", cosmosdb.Key);
                            Assert.Equal("testDatabase", cosmosdb.Database);
                            Assert.Equal("testCollection", cosmosdb.Collection);
                        }

                        break;

                    case ServiceTypes.Bot:
                        Assert.Equal(JsonConvert.SerializeObject(config.Services[i]), JsonConvert.SerializeObject(config2.Services[i]));
                        break;

                    case ServiceTypes.Dispatch:
                        {
                            Assert.NotEqual(JsonConvert.SerializeObject(config.Services[i]), JsonConvert.SerializeObject(config2.Services[i]));
                            var dispatch = (DispatchService)config2.Services[i];
                            Assert.DoesNotContain("0000", dispatch.AuthoringKey);
                            Assert.DoesNotContain("0000", dispatch.SubscriptionKey);
                        }

                        break;

                    case ServiceTypes.Endpoint:
                        {
                            Assert.NotEqual(JsonConvert.SerializeObject(config.Services[i]), JsonConvert.SerializeObject(config2.Services[i]));
                            var endpoint = (EndpointService)config2.Services[i];
                            Assert.Contains("0000", endpoint.AppId);
                            Assert.DoesNotContain("testpassword", endpoint.AppPassword);
                        }

                        break;

                    case ServiceTypes.File:
                        Assert.Equal(JsonConvert.SerializeObject(config.Services[i]), JsonConvert.SerializeObject(config2.Services[i]));
                        break;

                    case ServiceTypes.Luis:
                        {
                            Assert.NotEqual(JsonConvert.SerializeObject(config.Services[i]), JsonConvert.SerializeObject(config2.Services[i]));
                            var luis = (LuisService)config2.Services[i];
                            Assert.DoesNotContain("0000", luis.AuthoringKey);
                            Assert.DoesNotContain("0000", luis.SubscriptionKey);
                        }

                        break;

                    case ServiceTypes.QnA:
                        {
                            Assert.NotEqual(JsonConvert.SerializeObject(config.Services[i]), JsonConvert.SerializeObject(config2.Services[i]));
                            var qna = (QnAMakerService)config2.Services[i];
                            Assert.Contains("0000", qna.KbId);
                            Assert.DoesNotContain("0000", qna.EndpointKey);
                            Assert.DoesNotContain("0000", qna.SubscriptionKey);
                        }

                        break;
                    case ServiceTypes.Generic:
                        {
                            var generic = (GenericService)config2.Services[i];
                            Assert.Equal("https://bing.com", generic.Url);
                            Assert.NotEqual("testKey1", generic.Configuration["key1"]);
                            Assert.NotEqual("testKey2", generic.Configuration["key2"]);
                        }

                        break;
                    default:
                        // ignore unknown service type
                        break;
                }
            }
        }

        [Fact]
        public async Task LegacyEncryption()
        {
            var secretKey = "d+Mhts8yQIJIj9P/l1pO7n1fQExss7vvE8t9rg8qXsc=";
            var config = await BotConfiguration.LoadAsync(NormalizePath(@"..\..\..\legacy.bot"), secretKey);
            Assert.Equal("xyzpdq", ((EndpointService)config.Services[0]).AppPassword);
            Assert.False(string.IsNullOrEmpty(config.Padlock), "padlock should exist");
            Assert.Null(config.Properties["secretKey"]);

            await config.SaveAsAsync(OutputBotFileName, secretKey);
            config = await BotConfiguration.LoadAsync(OutputBotFileName, secretKey);
            File.Delete(OutputBotFileName);
            Assert.False(string.IsNullOrEmpty(config.Padlock), "padlock should exist");
            Assert.Null(config.Properties["secretKey"]);
        }

        [Fact]
        public void LoadAndVerifyChannelServiceSync()
        {
            var publicConfig = BotConfiguration.Load(testBotFileName);
            var endpointSvc = publicConfig.Services.Single(x => x.Type == ServiceTypes.Endpoint) as EndpointService;
            Assert.NotNull(endpointSvc);
            Assert.Null(endpointSvc.ChannelService);

            var govConfig = BotConfiguration.Load(NormalizePath(@"..\..\..\govTest.bot"));
            endpointSvc = govConfig.Services.Single(x => x.Type == ServiceTypes.Endpoint) as EndpointService;
            Assert.NotNull(endpointSvc);
            Assert.Equal("https://botframework.azure.us", endpointSvc.ChannelService);
        }

        private static string NormalizePath(string path) => Path.Combine(path.TrimEnd('\\').Split('\\'));
    }
}
