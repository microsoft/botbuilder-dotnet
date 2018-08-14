using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Bot.Configuration;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Microsoft.Bot.Configuration.Tests
{
    [TestClass]
    public class ConfingurationLoadAndSaveTests
    {
        [TestMethod]
        public async Task SerializeBotFile()
        {
            var config = await BotConfiguration.LoadAsync(@"..\..\test.bot");
            Assert.AreEqual("test", config.Name);
            Assert.AreEqual("test description", config.Description);
            Assert.AreEqual("", config.SecretKey);
            Assert.AreEqual(6, config.Services.Count);
        }


        [TestMethod]
        public async Task LoadAndSaveUnencryptedBotFile()
        {
            var config = await BotConfiguration.LoadAsync(@"..\..\test.bot");
            await config.SaveAsync("test.bot");

            var config2 = await BotConfiguration.LoadAsync("test.bot");
            Assert.AreEqual(JsonConvert.SerializeObject(config2), JsonConvert.SerializeObject(config), "saved should be the same");
        }

        [TestMethod]
        public async Task LoadAndSaveEncrypted()
        {
            string secret = "test";
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
                    case ServiceTypes.AzureBotService:
                        break;

                    case ServiceTypes.Dispatch:
                        {
                            var dispatch = (DispatchService)config2.Services[i];
                            Assert.IsTrue(dispatch.AuthoringKey.Contains("test"), "failed to decrypt authoringkey");
                            Assert.IsTrue(dispatch.SubscriptionKey.Contains("test"), "failed to decrypt subscriptionKey");
                        }
                        break;

                    case ServiceTypes.Endpoint:
                        {
                            var endpoint = (EndpointService)config2.Services[i];
                            Assert.IsTrue(endpoint.AppPassword.Contains("test"), "failed to decrypt appPassword");
                        }
                        break;

                    case ServiceTypes.File:
                        break;

                    case ServiceTypes.Luis:
                        {
                            var luis = (LuisService)config2.Services[i];
                            Assert.IsTrue(luis.AuthoringKey.Contains("test"), "failed to encrypt authoringkey");
                            Assert.IsTrue(luis.SubscriptionKey.Contains("test"), "failed to encrypt subscriptionKey");
                        }
                        break;

                    case ServiceTypes.QnA:
                        {
                            var qna = (QnAMakerService)config2.Services[i];
                            Assert.IsTrue(qna.KbId.Contains("test"), "kbId should not be changed");
                            Assert.IsTrue(qna.EndpointKey.Contains("test"), "failed to decrypt EndpointKey");
                            Assert.IsTrue(qna.SubscriptionKey.Contains("test"), "failed to decrypt SubscriptionKey");
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
                    case ServiceTypes.AzureBotService:
                        Assert.AreEqual(JsonConvert.SerializeObject(config.Services[i]), JsonConvert.SerializeObject(config2.Services[i]));
                        break;

                    case ServiceTypes.Dispatch:
                        {
                            Assert.AreNotEqual(JsonConvert.SerializeObject(config.Services[i]), JsonConvert.SerializeObject(config2.Services[i]));
                            var dispatch = (DispatchService)config2.Services[i];
                            Assert.IsFalse(dispatch.AuthoringKey.Contains("test"), "failed to encrypt authoringkey");
                            Assert.IsFalse(dispatch.SubscriptionKey.Contains("test"), "failed to encrypt subscriptionKey");
                        }
                        break;

                    case ServiceTypes.Endpoint:
                        {
                            Assert.AreNotEqual(JsonConvert.SerializeObject(config.Services[i]), JsonConvert.SerializeObject(config2.Services[i]));
                            var endpoint = (EndpointService)config2.Services[i];
                            Assert.IsTrue(endpoint.AppId.Contains("test"), "appId should not be changed");
                            Assert.IsFalse(endpoint.AppPassword.Contains("test"), "failed to encrypt appPassword");
                        }
                        break;

                    case ServiceTypes.File:
                        Assert.AreEqual(JsonConvert.SerializeObject(config.Services[i]), JsonConvert.SerializeObject(config2.Services[i]));
                        break;

                    case ServiceTypes.Luis:
                        {
                            Assert.AreNotEqual(JsonConvert.SerializeObject(config.Services[i]), JsonConvert.SerializeObject(config2.Services[i]));
                            var luis = (LuisService)config2.Services[i];
                            Assert.IsFalse(luis.AuthoringKey.Contains("test"), "failed to encrypt authoringkey");
                            Assert.IsFalse(luis.SubscriptionKey.Contains("test"), "failed to encrypt subscriptionKey");
                        }
                        break;

                    case ServiceTypes.QnA:
                        {
                            Assert.AreNotEqual(JsonConvert.SerializeObject(config.Services[i]), JsonConvert.SerializeObject(config2.Services[i]));
                            var qna = (QnAMakerService)config2.Services[i];
                            Assert.IsTrue(qna.KbId.Contains("test"), "kbId should not be changed");
                            Assert.IsFalse(qna.EndpointKey.Contains("test"), "failed to encrypt EndpointKey");
                            Assert.IsFalse(qna.SubscriptionKey.Contains("test"), "failed to encrypt SubscriptionKey");
                        }
                        break;

                    default:
                        throw new ArgumentException($"Unknown service type {config.Services[i].Type}");
                }
            }
        }
    }
}
