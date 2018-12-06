using System;
using Microsoft.Bot.Builder.AI.LanguageGeneration.Resolver;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.AI.LanguageGeneration.Tests
{
    [TestClass]
    public class LanguageGenerationApplicationTests
    {
        private readonly string applicationId = "test_application";
        private readonly string applicationRegion = "westus";
        private readonly string applicationLocale = "en-us";
        private readonly string applicationVersion = "0.1";
        private readonly string subscriptionKey = Guid.NewGuid().ToString();


        [TestMethod]
        public void LanguageGenerationApplication_Construction()
        {
            var languageGenerationApplication = new LanguageGenerationApplication(applicationId, applicationRegion, applicationLocale, applicationVersion, subscriptionKey);

            Assert.AreEqual(applicationId, languageGenerationApplication.ApplicationId);
            Assert.AreEqual(applicationRegion, languageGenerationApplication.ApplicationRegion);
            Assert.AreEqual(applicationLocale, languageGenerationApplication.ApplicationLocale);
            Assert.AreEqual(applicationVersion, languageGenerationApplication.ApplicationVersion);
            Assert.AreEqual(subscriptionKey, languageGenerationApplication.SubscriptionKey);
        }

        [TestMethod]
        public void LanguageGenerationApplication_Construction_InvalidApplicationId()
        {
            var expectedParameterName = "applicationId";

            ValidateThrownArgumentException(() => new LanguageGenerationApplication(null, applicationRegion, applicationLocale, applicationVersion, subscriptionKey), expectedParameterName);
            ValidateThrownArgumentException(() => new LanguageGenerationApplication(string.Empty, applicationRegion, applicationLocale, applicationVersion, subscriptionKey), expectedParameterName);
        }

        [TestMethod]
        public void LanguageGenerationApplication_Construction_InvalidApplicationRegion()
        {
            var expectedParameterName = "applicationRegion";

            ValidateThrownArgumentException(() => new LanguageGenerationApplication(applicationId, null, applicationLocale, applicationVersion, subscriptionKey), expectedParameterName);
            ValidateThrownArgumentException(() => new LanguageGenerationApplication(applicationId, string.Empty, applicationLocale, applicationVersion, subscriptionKey), expectedParameterName);
        }

        [TestMethod]
        public void LanguageGenerationApplication_Construction_InvalidApplicationLocale()
        {
            var expectedParameterName = "applicationLocale";

            ValidateThrownArgumentException(() => new LanguageGenerationApplication(applicationId, applicationRegion, null, applicationVersion, subscriptionKey), expectedParameterName);
            ValidateThrownArgumentException(() => new LanguageGenerationApplication(applicationId, applicationRegion, string.Empty, applicationVersion, subscriptionKey), expectedParameterName);
        }

        [TestMethod]
        public void LanguageGenerationApplication_Construction_InvalidApplicationVersion()
        {
            var expectedParameterName = "applicationVersion";

            ValidateThrownArgumentException(() => new LanguageGenerationApplication(applicationId, applicationRegion, applicationLocale, null, subscriptionKey), expectedParameterName);
            ValidateThrownArgumentException(() => new LanguageGenerationApplication(applicationId, applicationRegion, applicationLocale, string.Empty, subscriptionKey), expectedParameterName);
        }

        [TestMethod]
        public void LanguageGenerationApplication_Construction_InvalidSubscriptionKey()
        {
            var expectedParameterName = "subscriptionKey";

            ValidateThrownArgumentException(() => new LanguageGenerationApplication(applicationId, applicationRegion, applicationLocale, applicationVersion, null), expectedParameterName);
            ValidateThrownArgumentException(() => new LanguageGenerationApplication(applicationId, applicationRegion, applicationLocale, applicationVersion, string.Empty), expectedParameterName);
            ValidateThrownArgumentException(() => new LanguageGenerationApplication(applicationId, applicationRegion, applicationLocale, applicationVersion, "InvalidSubscriptionKey"), expectedParameterName);
        }

        private void ValidateThrownArgumentException(Func<LanguageGenerationApplication> factory, string expectedParameterName)
        {
            var exception = Assert.ThrowsException<ArgumentException>(factory);
            Assert.AreEqual(expectedParameterName, exception.ParamName);
        }
    }
}
