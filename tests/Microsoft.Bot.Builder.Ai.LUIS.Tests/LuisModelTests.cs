// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Ai.LUIS.Tests
{
    [TestClass]
    public class LuisModelTests
    {

        [TestMethod]
        public void LuisModel_Construction()
        {
            var model = GetValidModel();
            Assert.IsNotNull(model);

            Assert.ThrowsException<ArgumentNullException>(() => new LuisModel(null, "abc", new Uri("https://luis.ai"), LuisApiVersion.V2));
            Assert.ThrowsException<ArgumentNullException>(() => new LuisModel(string.Empty, "abc", new Uri("https://luis.ai"), LuisApiVersion.V2));
            Assert.ThrowsException<ArgumentNullException>(() => new LuisModel(Guid.Empty.ToString(), null, new Uri("https://luis.ai"), LuisApiVersion.V2));
            Assert.ThrowsException<ArgumentNullException>(() => new LuisModel(Guid.Empty.ToString(), string.Empty, new Uri("https://luis.ai"), LuisApiVersion.V2));
            Assert.ThrowsException<ArgumentNullException>(() => new LuisModel(Guid.Empty.ToString(), "abc", null, LuisApiVersion.V2));
        }

        [TestMethod]
        public void LuisModel_Serialization()
        {
            var model = GetValidModel();
            var serialized = JsonConvert.SerializeObject(model);
            var deserialized = JsonConvert.DeserializeObject<LuisModel>(serialized);

            Assert.IsNotNull(deserialized);
            Assert.AreEqual(model.ApiVersion, deserialized.ApiVersion);
            Assert.AreEqual(model.ModelID, deserialized.ModelID);
            Assert.AreEqual(model.SubscriptionKey, deserialized.SubscriptionKey);
            Assert.AreEqual(model.Threshold, deserialized.Threshold);
            Assert.AreEqual(model.UriBase.Host, deserialized.UriBase.Host);
        }

        private LuisModel GetValidModel()
        {
            return new LuisModel(Guid.Empty.ToString(), "abc", new Uri("https://luis.ai"), LuisApiVersion.V2);
        }
    }
}
