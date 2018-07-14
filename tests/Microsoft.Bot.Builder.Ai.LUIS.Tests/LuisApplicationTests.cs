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
            var model = this.GetValidModel();
            Assert.IsNotNull(model);

            Assert.ThrowsException<ArgumentNullException>(() => new LuisApplication(null, "abc", "westus"));
            Assert.ThrowsException<ArgumentNullException>(() => new LuisApplication(string.Empty, "abc", "westus"));
            Assert.ThrowsException<ArgumentNullException>(() => new LuisApplication(Guid.Empty.ToString(), null, "westus"));
            Assert.ThrowsException<ArgumentNullException>(() => new LuisApplication(Guid.Empty.ToString(), string.Empty, "westus"));
            Assert.ThrowsException<ArgumentNullException>(() => new LuisApplication(Guid.Empty.ToString(), "abc", null));
        }

        [TestMethod]
        public void LuisApplication_Serialization()
        {
            var model = this.GetValidModel();
            var serialized = JsonConvert.SerializeObject(model);
            var deserialized = JsonConvert.DeserializeObject<LuisApplication>(serialized);

            Assert.IsNotNull(deserialized);
            Assert.AreEqual(model.ApplicationId, deserialized.ApplicationId);
            Assert.AreEqual(model.SubscriptionKey, deserialized.SubscriptionKey);
            Assert.AreEqual(model.AzureRegion, deserialized.AzureRegion);
        }

        private LuisApplication GetValidModel()
        {
            return new LuisApplication(Guid.Empty.ToString(), "abc", "westus");
        }
    }
}
