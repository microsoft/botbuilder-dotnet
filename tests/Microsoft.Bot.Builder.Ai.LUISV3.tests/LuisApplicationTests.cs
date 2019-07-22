// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Microsoft.Bot.Builder.AI.Luis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.AI.LuisV3.Tests
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
        public void ListApplicationFromLuisEndpoint()
        {
            // Arrange
            // Note this is NOT a real LUIS application ID nor a real LUIS subscription-key
            // theses are GUIDs edited to look right to the parsing and validation code.
            var endpoint = "https://westus.api.cognitive.microsoft.com/luis/v3.0-preview/apps/b31aeaf3-3511-495b-a07f-571fc873214b/slots/production/predict?verbose=true&timezoneOffset=-360&subscription-key=048ec46dc58e495482b0c447cfdbd291&q=";

            // Act
            var app = new LuisApplication(endpoint);

            // Assert
            Assert.AreEqual("b31aeaf3-3511-495b-a07f-571fc873214b", app.ApplicationId);
            Assert.AreEqual("048ec46dc58e495482b0c447cfdbd291", app.EndpointKey);
            Assert.AreEqual("https://westus.api.cognitive.microsoft.com", app.Endpoint);
        }

        [TestMethod]
        public void ListApplicationFromLuisEndpointBadArguments()
        {
            Assert.ThrowsException<ArgumentException>(() => new LuisApplication("this.is.not.a.uri"));
            Assert.ThrowsException<ArgumentException>(() => new LuisApplication("https://westus.api.cognitive.microsoft.com/luis/v3.0-preview/apps/b31aeaf3-3511-495b-a07f-571fc873214b/slots/production/predict?verbose=true&timezoneOffset=-360&q="));
            Assert.ThrowsException<ArgumentException>(() => new LuisApplication("https://westus.api.cognitive.microsoft.com?verbose=true&timezoneOffset=-360&subscription-key=048ec46dc58e495482b0c447cfdbd291&q="));
        }

        private LuisApplication GetValidModel()
            => new LuisApplication(Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), Endpoint);
    }
}
