// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Microsoft.Bot.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.AI.Luis.Tests
{
    [TestClass]
    public class LuisApplicationTests
    {
        private const string Endpoint = "https://westus.api.cognitive.microsoft.com";

        [TestMethod]
        public void LuisApplication_Construction()
        {
            var model = GetValidModel();
            Assert.IsNotNull(model);

            Assert.ThrowsException<ArgumentException>(() => new LuisApplication(null, Guid.NewGuid().ToString(), Endpoint));
            Assert.ThrowsException<ArgumentException>(() => new LuisApplication(string.Empty, Guid.NewGuid().ToString(), Endpoint));
            Assert.ThrowsException<ArgumentException>(() => new LuisApplication("0000", Guid.NewGuid().ToString(), Endpoint));
            Assert.ThrowsException<ArgumentException>(() => new LuisApplication(Guid.NewGuid().ToString(), null, Endpoint));
            Assert.ThrowsException<ArgumentException>(() => new LuisApplication(Guid.NewGuid().ToString(), string.Empty, Endpoint));
            Assert.ThrowsException<ArgumentException>(() => new LuisApplication(Guid.NewGuid().ToString(), "0000", Endpoint));
            Assert.ThrowsException<ArgumentException>(() => new LuisApplication(Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), null));

            var luisApp = new LuisApplication(Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), Endpoint);
            Assert.AreEqual(Endpoint, luisApp.Endpoint);
        }

        [TestMethod]
        public void LuisApplication_Serialization()
        {
            var model = GetValidModel();
            var serialized = JsonConvert.SerializeObject(model);
            var deserialized = JsonConvert.DeserializeObject<LuisApplication>(serialized);

            Assert.IsNotNull(deserialized);
            Assert.AreEqual(model.ApplicationId, deserialized.ApplicationId);
            Assert.AreEqual(model.EndpointKey, deserialized.EndpointKey);
            Assert.AreEqual(model.Endpoint, deserialized.Endpoint);
        }

        [TestMethod]
        public void LuisApplication_Configuration()
        {
            var service = new LuisService
            {
                AppId = Guid.NewGuid().ToString(),
                SubscriptionKey = Guid.NewGuid().ToString(),
                Region = "westus"
            };

            var model = new LuisApplication(service);

            Assert.AreEqual(service.AppId, model.ApplicationId);
            Assert.AreEqual(service.SubscriptionKey, model.EndpointKey);
            Assert.AreEqual(service.GetEndpoint(), model.Endpoint);
        }

        private LuisApplication GetValidModel()
            => new LuisApplication(Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), Endpoint);
    }
}
