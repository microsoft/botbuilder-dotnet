using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Bot.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RichardSzalay.MockHttp;

namespace Microsoft.Bot.Builder.AI.TextTranslator.Tests
{
    [TestClass]
    public class TextTranslatorTests
    {
        private string _subscriptionKey = "dummy";
        private string _deeplAuthKey = "dummy";

        [TestMethod]
        [TestCategory("AI")]
        [TestCategory("TextTranslator")]
        public async Task TextTranslator_MsTranslatorWithSource()
        {
            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When(HttpMethod.Post, "https://api.cognitive.microsofttranslator.com/*")
                    .Respond("application/json", GetResponse("MsTranslation_ItalResponse.json"));

            var translator = new TextTranslator(new TextTranslatorEndpoint(TranslatorEngine.MicrosoftTranslator)
            {
                SubscriptionKey = _subscriptionKey
            }, mockHttp.ToHttpClient());
            var result = await translator.TranslateAsync("Ich bin ein Berliner", "it");
            Assert.IsNotNull(result);
        }

        [TestMethod]
        [TestCategory("AI")]
        [TestCategory("TextTranslator")]
        public async Task TextTranslator_MsTranslatorWithoutSource()
        {
            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When(HttpMethod.Post, "https://api.cognitive.microsofttranslator.com/*")
                    .Respond("application/json", GetResponse("MsTranslation_ItalResponse.json"));

            var translator = new TextTranslator(new TextTranslatorEndpoint(TranslatorEngine.MicrosoftTranslator)
            {
                SubscriptionKey = _subscriptionKey
            }, mockHttp.ToHttpClient());
            var result = await translator.TranslateAsync("Ich bin ein Berliner", "it");
            Assert.IsNotNull(result);
        }

        [TestMethod]
        [TestCategory("AI")]
        [TestCategory("TextTranslator")]
        public async Task TextTranslator_DeeplTranslatorWithoutSource()
        {
            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When(HttpMethod.Post, "https://api.deepl.com/v1/translate")
                    .Respond("application/json", GetResponse("Deepl_ItalResponse.json"));

            var translator = new TextTranslator(new TextTranslatorEndpoint(TranslatorEngine.Deepl)
            {
                SubscriptionKey = _deeplAuthKey
            }, mockHttp.ToHttpClient());
            var result = await translator.TranslateAsync("Ich bin ein Berliner", "it");
            Assert.IsNotNull(result);
        }

        [TestMethod]
        [TestCategory("AI")]
        [TestCategory("TextTranslator")]
        public async Task TextTranslator_DeeplTranslatorWithSource()
        {
            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When(HttpMethod.Post, "https://api.deepl.com/v1/translate")
                    .Respond("application/json", GetResponse("Deepl_ItalResponse.json"));

            var translator = new TextTranslator(new TextTranslatorEndpoint(TranslatorEngine.Deepl)
            {
                SubscriptionKey = _deeplAuthKey
            }, mockHttp.ToHttpClient());
            var result = await translator.TranslateAsync("Ich bin ein Berliner", "it", "de");
            Assert.IsNotNull(result);
        }

        private const string _testData = @"..\..\..\TestData\";

        private Stream GetResponse(string fileName)
        {
            var path = Path.Combine(_testData, fileName);
            return File.OpenRead(path);
        }
    }
}
