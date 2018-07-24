// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Microsoft.Azure.CognitiveServices.Language.LUIS.Runtime;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Ai.Luis.Tests
{
    [TestClass]
    public class LuisApplicationTests
    {
        [TestMethod]
        public void LuisApplication_Construction()
        {
            var model = GetValidModel();
            Assert.IsNotNull(model);

            Assert.ThrowsException<ArgumentException>(() => new LuisApplication(null, Guid.Empty.ToString(), "westus"));
            Assert.ThrowsException<ArgumentException>(() => new LuisApplication(string.Empty, Guid.Empty.ToString(), "Westus"));
            Assert.ThrowsException<ArgumentException>(() => new LuisApplication("0000", Guid.Empty.ToString(), "Westus"));
            Assert.ThrowsException<ArgumentException>(() => new LuisApplication(Guid.Empty.ToString(), null, "Westus"));
            Assert.ThrowsException<ArgumentException>(() => new LuisApplication(Guid.Empty.ToString(), string.Empty, "Westus"));
            Assert.ThrowsException<ArgumentException>(() => new LuisApplication(Guid.Empty.ToString(), "0000", "Westus"));
            Assert.ThrowsException<ArgumentException>(() => new LuisApplication(Guid.Empty.ToString(), Guid.Empty.ToString(), null));
            Assert.ThrowsException<ArgumentException>(() => new LuisApplication(Guid.Empty.ToString(), Guid.Empty.ToString(), "westus55"));
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
            Assert.AreEqual(model.AzureRegion, deserialized.AzureRegion);
        }

        private LuisApplication GetValidModel()
            => new LuisApplication(Guid.Empty.ToString(), Guid.Empty.ToString(), "Westus");
    }
}
