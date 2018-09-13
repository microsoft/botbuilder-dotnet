// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.AI.Luis.Tests
{
    [TestClass]
    public class LuisApplicationTests
    {
        private const string Endpoint = "https://westus.api.cognitive.microsoft.com/luis/v2.0/";

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

            // test valid cases
            var luisApp = new LuisApplication(Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), "eastus");
            Assert.AreEqual("https://eastus.api.cognitive.microsoft.com/luis/v2.0/", luisApp.Endpoint);

            luisApp = new LuisApplication(Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), Endpoint);
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

        private LuisApplication GetValidModel()
            => new LuisApplication(Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), Endpoint);
    }
}
