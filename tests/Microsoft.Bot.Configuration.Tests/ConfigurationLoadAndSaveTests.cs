using System;
using System.Threading.Tasks;
using Microsoft.Bot.Configuration.Encryption;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace Microsoft.Bot.Configuration.Tests
{
    [TestClass]
    public class ConfingurationLoadAndSaveTests
    {
        [TestMethod]
        public async Task BasicLoad()
        {
            var config = await BotConfiguration.LoadAsync(@"..\..\test.bot");
            Assert.AreEqual("test", config.Name);
            Assert.AreEqual("test description", config.Description);
            Assert.AreEqual("", config.SecretKey);
            Assert.AreEqual(7, config.Services.Count);
            foreach(var service in config.Services)
            {
                switch(service.Type)
                {
                    case ServiceTypes.AppInsights:
                        Assert.AreEqual(typeof(AppInsightsService), service.GetType());
                        break;
                    case ServiceTypes.AzureBot:
                        Assert.AreEqual(typeof(AzureBotService), service.GetType());
                        break;
                    case ServiceTypes.AzureStorage:
                        Assert.AreEqual(typeof(AzureStorageService), service.GetType());
                        break;
                    case ServiceTypes.Dispatch:
                        Assert.AreEqual(typeof(DispatchService), service.GetType());
                        break;
                    case ServiceTypes.Endpoint:
                        Assert.AreEqual(typeof(EndpointService), service.GetType());
                        break;
                    case ServiceTypes.File:
                        Assert.AreEqual(typeof(FileService), service.GetType());
                        break;
                    case ServiceTypes.Luis:
                        Assert.AreEqual(typeof(LuisService), service.GetType());
                        break;
                    case ServiceTypes.QnA:
                        Assert.AreEqual(typeof(QnAMakerService), service.GetType());
                        break;
                    default:
                        throw new Exception("Unknown service type!");
                }
            }
        }


        [TestMethod]
        public async Task LoadAndSaveUnencryptedBotFile()
        {
            var config = await BotConfiguration.LoadAsync(@"..\..\test.bot");
            await config.SaveAsync("save.bot");

            var config2 = await BotConfiguration.LoadAsync(@"..\..\test.bot");
            Assert.AreEqual(JsonConvert.SerializeObject(config2), JsonConvert.SerializeObject(config), "saved should be the same");
        }

        [TestMethod]
        public async Task CantLoadWithoutSecret()
        {
            string secret = BotConfiguration.GenerateKey();
            var config = await BotConfiguration.LoadAsync(@"..\..\test.bot");
            await config.SaveAsync("save.bot", secret);

            try
            {
                await BotConfiguration.LoadAsync(@"save.bot");
                Assert.Fail("Load should have thrown due to no secret");
            }
            catch { }
        }

        [TestMethod]
        public async Task CantSaveWithoutSecret()
        {
            string secret = BotConfiguration.GenerateKey();
            var config = await BotConfiguration.LoadAsync(@"..\..\test.bot");
            await config.SaveAsync("save.bot", secret);

            var config2 = await BotConfiguration.LoadAsync(@"save.bot", secret);
            try
            {
                await config2.SaveAsync("save.bot");
                Assert.Fail("Save() should have thrown due to no secret");
            }
            catch { }
            config2.ClearSecret();
            await config2.SaveAsync("save.bot", secret);
        }


        [TestMethod]
        public async Task LoadAndSaveEncrypted()
        {
            string secret = BotConfiguration.GenerateKey();
            var config = await BotConfiguration.LoadAsync(@"..\..\test.bot");
            Assert.AreEqual("", config.SecretKey, "There should be no secretKey");

            // save with secret
            await config.SaveAsync("savesecret.bot", secret);
            Assert.IsTrue(config.SecretKey?.Length > 0, "There should be a secretKey");

            // load with secret
            var config2 = await BotConfiguration.LoadAsync("savesecret.bot", secret);
            Assert.IsTrue(config2.SecretKey?.Length > 0, "There should be a secretKey");
            Assert.AreEqual(config.SecretKey, config2.SecretKey, "SecretKeys should be the same");

            // make sure these were decrypted
            for (int i = 0; i < config.Services.Count; i++)
            {
                Assert.AreEqual(JsonConvert.SerializeObject(config.Services[i]), JsonConvert.SerializeObject(config2.Services[i]));

                switch (config.Services[i].Type)
                {
                    case ServiceTypes.AzureBot:
                        break;

                    case ServiceTypes.AppInsights:
                        {
                            var appInsights = (AppInsightsService)config2.Services[i];
                            Assert.IsTrue(appInsights.InstrumentationKey.Contains("0000"), "failed to decrypt instrumentationKey");
                        }
                        break;

                    case ServiceTypes.AzureStorage:
                        {
                            var azureStorage = (AzureStorageService)config2.Services[i];
                            Assert.AreEqual("UseDevelopmentStorage=true;", azureStorage.ConnectionString, "failed to decrypt connectionString");
                        }
                        break;

                    case ServiceTypes.Dispatch:
                        {
                            var dispatch = (DispatchService)config2.Services[i];
                            Assert.IsTrue(dispatch.AuthoringKey.Contains("0000"), "failed to decrypt authoringkey");
                            Assert.IsTrue(dispatch.SubscriptionKey.Contains("0000"), "failed to decrypt subscriptionKey");
                        }
                        break;

                    case ServiceTypes.Endpoint:
                        {
                            var endpoint = (EndpointService)config2.Services[i];
                            Assert.AreEqual("testpassword", endpoint.AppPassword, "failed to decrypt appPassword");
                        }
                        break;

                    case ServiceTypes.File:
                        break;

                    case ServiceTypes.Luis:
                        {
                            var luis = (LuisService)config2.Services[i];
                            Assert.IsTrue(luis.AuthoringKey.Contains("0000"), "failed to encrypt authoringkey");
                            Assert.IsTrue(luis.SubscriptionKey.Contains("0000"), "failed to encrypt subscriptionKey");
                        }
                        break;

                    case ServiceTypes.QnA:
                        {
                            var qna = (QnAMakerService)config2.Services[i];
                            Assert.IsTrue(qna.KbId.Contains("0000"), "kbId should not be changed");
                            Assert.IsTrue(qna.EndpointKey.Contains("0000"), "failed to decrypt EndpointKey");
                            Assert.IsTrue(qna.SubscriptionKey.Contains("0000"), "failed to decrypt SubscriptionKey");
                        }
                        break;

                    default:
                        throw new ArgumentException($"Unknown service type {config.Services[i].Type}");
                }
            }

            // encrypt in memory copy
            config2.Encrypt(secret);

            // make sure these are all true
            for (int i = 0; i < config.Services.Count; i++)
            {
                switch (config.Services[i].Type)
                {
                    case ServiceTypes.AppInsights:
                        {
                            var appInsights = (AppInsightsService)config2.Services[i];
                            Assert.IsFalse(appInsights.InstrumentationKey.Contains("0000"), "failed to encrypt instrumentationKey");
                        }
                        break;

                    case ServiceTypes.AzureStorage:
                        {
                            var azureStorage = (AzureStorageService)config2.Services[i];
                            Assert.AreNotEqual("UseDevelopmentStorage=true;", azureStorage.ConnectionString, "failed to encrypt connectionString");
                        }
                        break;

                    case ServiceTypes.AzureBot:
                        Assert.AreEqual(JsonConvert.SerializeObject(config.Services[i]), JsonConvert.SerializeObject(config2.Services[i]));
                        break;

                    case ServiceTypes.Dispatch:
                        {
                            Assert.AreNotEqual(JsonConvert.SerializeObject(config.Services[i]), JsonConvert.SerializeObject(config2.Services[i]));
                            var dispatch = (DispatchService)config2.Services[i];
                            Assert.IsFalse(dispatch.AuthoringKey.Contains("0000"), "failed to encrypt authoringkey");
                            Assert.IsFalse(dispatch.SubscriptionKey.Contains("0000"), "failed to encrypt subscriptionKey");
                        }
                        break;

                    case ServiceTypes.Endpoint:
                        {
                            Assert.AreNotEqual(JsonConvert.SerializeObject(config.Services[i]), JsonConvert.SerializeObject(config2.Services[i]));
                            var endpoint = (EndpointService)config2.Services[i];
                            Assert.IsTrue(endpoint.AppId.Contains("0000"), "appId should not be changed");
                            Assert.IsFalse(endpoint.AppPassword.Contains("testpassword"), "failed to encrypt appPassword");
                        }
                        break;

                    case ServiceTypes.File:
                        Assert.AreEqual(JsonConvert.SerializeObject(config.Services[i]), JsonConvert.SerializeObject(config2.Services[i]));
                        break;

                    case ServiceTypes.Luis:
                        {
                            Assert.AreNotEqual(JsonConvert.SerializeObject(config.Services[i]), JsonConvert.SerializeObject(config2.Services[i]));
                            var luis = (LuisService)config2.Services[i];
                            Assert.IsFalse(luis.AuthoringKey.Contains("0000"), "failed to encrypt authoringkey");
                            Assert.IsFalse(luis.SubscriptionKey.Contains("0000"), "failed to encrypt subscriptionKey");
                        }
                        break;

                    case ServiceTypes.QnA:
                        {
                            Assert.AreNotEqual(JsonConvert.SerializeObject(config.Services[i]), JsonConvert.SerializeObject(config2.Services[i]));
                            var qna = (QnAMakerService)config2.Services[i];
                            Assert.IsTrue(qna.KbId.Contains("0000"), "kbId should not be changed");
                            Assert.IsFalse(qna.EndpointKey.Contains("0000"), "failed to encrypt EndpointKey");
                            Assert.IsFalse(qna.SubscriptionKey.Contains("0000"), "failed to encrypt SubscriptionKey");
                        }
                        break;

                    default:
                        throw new ArgumentException($"Unknown service type {config.Services[i].Type}");
                }
            }
        }
    }
}
