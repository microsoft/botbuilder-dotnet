using System;
using Microsoft.Bot.Builder.AI.LanguageGeneration.Resolver;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.AI.LanguageGeneration.Tests
{
    [TestClass]
    public class LanguageGenerationApplicationTests
    {
        private const string Endpoint = "https://platform.bing.com/speechdx/lg-dev/v1/lg";

        [TestMethod]
        public void LanguageGenerationApplication_Construction()
        {

            Assert.ThrowsException<ArgumentException>(() => new LanguageGenerationApplication(null, Guid.NewGuid().ToString(), Endpoint));
            Assert.ThrowsException<ArgumentException>(() => new LanguageGenerationApplication(string.Empty, Guid.NewGuid().ToString(), Endpoint));
            Assert.ThrowsException<ArgumentException>(() => new LanguageGenerationApplication(Guid.NewGuid().ToString(), null, Endpoint));
            Assert.ThrowsException<ArgumentException>(() => new LanguageGenerationApplication(Guid.NewGuid().ToString(), string.Empty, Endpoint));
            Assert.ThrowsException<ArgumentException>(() => new LanguageGenerationApplication(Guid.NewGuid().ToString(), "0000", Endpoint));
            Assert.ThrowsException<ArgumentException>(() => new LanguageGenerationApplication(Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), null));

            var languageGenerationApplication = new LanguageGenerationApplication(Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), Endpoint);
            Assert.AreEqual(Endpoint, languageGenerationApplication.Endpoint);
        }
    }
}
