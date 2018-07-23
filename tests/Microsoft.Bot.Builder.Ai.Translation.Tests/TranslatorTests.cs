// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Ai.Translation.PostProcessor;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RichardSzalay.MockHttp;

namespace Microsoft.Bot.Builder.Ai.Translation.Tests
{
    [TestClass]
    public class TranslatorTests
    {
        private const string _translatorKey = "dummy-key";

        [TestMethod]
        [TestCategory("AI")]
        [TestCategory("Translator")]
        public void Translator_InvalidArguments_NullTranslatorKey()
        {
            string translatorKey = null;
            Assert.ThrowsException<ArgumentNullException>(() => new Translator(translatorKey));
        }

        [TestMethod]
        [TestCategory("AI")]
        [TestCategory("Translator")]
        public void Translator_InvalidArguments_EmptyTranslatorKey()
        {
            var translatorKey = "";
            Assert.ThrowsException<ArgumentNullException>(() => new Translator(translatorKey));
        }

        [TestMethod]
        [TestCategory("AI")]
        [TestCategory("Translator")]
        public async Task Translator_DetectAndTranslateToEnglish()
        {
            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When(HttpMethod.Post, "https://api.cognitive.microsoft.com/sts/v1.0/issueToken")
                .Respond("application/jwt", "<--valid-bearer-token-->");
            mockHttp.When(HttpMethod.Get, GetRequestDetect("salut"))
                .Respond("application/xml", GetResponseDetect("fr"));
            mockHttp.When(HttpMethod.Get, GetRequestTranslate("salut", "fr", "en"))
                .Respond("application/xml", GetResponseTranslate("Hello"));

            var translator = new Translator(_translatorKey, mockHttp.ToHttpClient());

            var sentence = "salut";
            var detectedLanguage = await translator.DetectAsync(sentence);
            Assert.IsNotNull(detectedLanguage);
            Assert.AreEqual("fr", detectedLanguage, "should detect french language");

            var translatedSentence = await translator.TranslateAsync(sentence, detectedLanguage, "en");
            Assert.IsNotNull(translatedSentence);
            Assert.AreEqual("Hello", translatedSentence.TargetMessage);
        }

        [TestMethod]
        [TestCategory("AI")]
        [TestCategory("Translator")]
        public async Task Translator_LiteralTagTest()
        {
            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When(HttpMethod.Post, "https://api.cognitive.microsoft.com/sts/v1.0/issueToken")
                .Respond("application/jwt", "<--valid-bearer-token-->");
            mockHttp.When(HttpMethod.Post, @"https://api.microsofttranslator.com/v2/Http.svc/TranslateArray2")
                .Respond("application/xml", GetResponse("Translator_LiteralTagTest.xml"));

            var translator = new Translator(_translatorKey, mockHttp.ToHttpClient());

            var sentence = "salut <literal>Jean Bouchier mon ami</literal>";

            var translatedSentence = await translator.TranslateArrayAsync(new string[] { sentence }, "fr", "en");
            var patterns = new Dictionary<string, List<string>>();
            patterns.Add("fr", new List<string>());
            var postProcessor = new PatternsPostProcessor(patterns);
            var postProcessedDocument = postProcessor.Process(translatedSentence[0], "fr");
            Assert.IsNotNull(translatedSentence);
            Assert.AreEqual("Hi Jean Bouchier mon ami", postProcessedDocument.PostProcessedMessage);
        }

        [TestMethod]
        [TestCategory("AI")]
        [TestCategory("Translator")]
        public async Task Translator_TranslateFrenchToEnglish()
        {
            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When(HttpMethod.Post, "https://api.cognitive.microsoft.com/sts/v1.0/issueToken")
                .Respond("application/jwt", "<--valid-bearer-token-->");
            mockHttp.When(HttpMethod.Get, GetRequestTranslate("salut 20-10", "fr", "en"))
                .Respond("application/xml", GetResponseTranslate("Hi 20-10"));

            var translator = new Translator(_translatorKey, mockHttp.ToHttpClient());

            var sentence = "salut 20-10";
            var translatedSentence = await translator.TranslateAsync(sentence, "fr", "en");
            Assert.IsNotNull(translatedSentence);
            Assert.AreEqual("Hi 20-10", translatedSentence.TargetMessage);
        }

        [TestMethod]
        [TestCategory("AI")]
        [TestCategory("Translator")]
        public async Task Translator_TranslateFrenchToEnglishArray()
        {
            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When(HttpMethod.Post, "https://api.cognitive.microsoft.com/sts/v1.0/issueToken")
                .Respond("application/jwt", "<--valid-bearer-token-->");
            mockHttp.When(HttpMethod.Post, @"https://api.microsofttranslator.com/v2/Http.svc/TranslateArray2")
                .Respond("application/xml", GetResponse("Translator_TranslateFrenchToEnglishArray.xml"));

            var translator = new Translator(_translatorKey, mockHttp.ToHttpClient());

            var sentences = new string[] { "mon nom est", "salut", "au revoir" };
            var translatedSentences = await translator.TranslateArrayAsync(sentences, "fr", "en");
            Assert.IsNotNull(translatedSentences);
            Assert.AreEqual(3, translatedSentences.Count, "should be 3 sentences");
            Assert.AreEqual("My name is", translatedSentences[0].TargetMessage);
            Assert.AreEqual("Hello", translatedSentences[1].TargetMessage);
            Assert.AreEqual("Good bye", translatedSentences[2].TargetMessage);
        }

        [TestMethod]
        [TestCategory("AI")]
        [TestCategory("Translator")]
        public async Task Translator_TranslateEnglishToFrench()
        {
            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When(HttpMethod.Post, "https://api.cognitive.microsoft.com/sts/v1.0/issueToken")
                .Respond("application/jwt", "<--valid-bearer-token-->");
            mockHttp.When(HttpMethod.Get, GetRequestTranslate("hello", "en", "fr"))
                .Respond("application/xml", GetResponseTranslate("Salut"));

            var translator = new Translator(_translatorKey, mockHttp.ToHttpClient());

            var sentence = "hello";
            var translatedSentence = await translator.TranslateAsync(sentence, "en", "fr");
            Assert.IsNotNull(translatedSentence);
            Assert.AreEqual("Salut", translatedSentence.TargetMessage);
        }

        [TestMethod]
        [TestCategory("AI")]
        [TestCategory("Translator")]
        public async Task Translator_TranslateEnglishToFrenchArray()
        {
            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When(HttpMethod.Post, "https://api.cognitive.microsoft.com/sts/v1.0/issueToken")
                .Respond("application/jwt", "<--valid-bearer-token-->");
            mockHttp.When(HttpMethod.Post, @"https://api.microsofttranslator.com/v2/Http.svc/TranslateArray2")
                .Respond("application/xml", GetResponse("Translator_TranslateEnglishToFrenchArray.xml"));

            var translator = new Translator(_translatorKey, mockHttp.ToHttpClient());

            var sentences = new string[] { "Hello", "Good bye" };
            var translatedSentences = await translator.TranslateArrayAsync(sentences, "en", "fr");
            Assert.IsNotNull(translatedSentences);
            Assert.AreEqual(2, translatedSentences.Count, "should be 2 sentences");
            Assert.AreEqual("Salut", translatedSentences[0].TargetMessage);
            Assert.AreEqual("Au revoir", translatedSentences[1].TargetMessage);
        }

        [TestMethod]
        [TestCategory("AI")]
        [TestCategory("Translator")]
        public async Task Translator_InvalidSourceLanguage()
        {
            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When(HttpMethod.Post, "https://api.cognitive.microsoft.com/sts/v1.0/issueToken")
                .Respond("application/jwt", "<--valid-bearer-token-->");
            mockHttp.When(HttpMethod.Get, GetRequestTranslate("Arrange an appointment for tomorrow", "na", "de"))
                .Respond(HttpStatusCode.BadRequest, "application/xml", GetResponse("Translator_InvalidSourceLanguage.xml"));

            Translator translator = new Translator(_translatorKey, mockHttp.ToHttpClient());

            var sentence = "Arrange an appointment for tomorrow";
            await Assert.ThrowsExceptionAsync<ArgumentException>(async () =>
                await translator.TranslateAsync(sentence, "na", "de"));
        }

        [TestMethod]
        [TestCategory("AI")]
        [TestCategory("Translator")]
        public async Task Translator_InvalidTargetLanguage()
        {
            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When(HttpMethod.Post, "https://api.cognitive.microsoft.com/sts/v1.0/issueToken")
                .Respond("application/jwt", "<--valid-bearer-token-->");
            mockHttp.When(HttpMethod.Get, GetRequestTranslate("Arrange an appointment for tomorrow", "en", "na"))
                .Respond(HttpStatusCode.BadRequest, "application/xml", GetResponse("Translator_InvalidTargetLanguage.xml"));

            Translator translator = new Translator(_translatorKey, mockHttp.ToHttpClient());

            var sentence = "Arrange an appointment for tomorrow";
            await Assert.ThrowsExceptionAsync<ArgumentException>(async () =>
                await translator.TranslateAsync(sentence, "en", "na"));
        }

        private string GetRequestDetect(string text)
        {
            return "http://api.microsofttranslator.com/v2/Http.svc/Detect?text=" + text;
        }

        private string GetRequestTranslate(string text, string from, string to)
        {
            return "http://api.microsofttranslator.com/v2/Http.svc/Translate?text=" + text + "&from=" + from + "&to=" + to;
        }

        private string GetResponseDetect(string text)
        {
            return $"<string xmlns=\"http://schemas.microsoft.com/2003/10/Serialization/\">{text}</string>";
        }

        private string GetResponseTranslate(string text)
        {
            return $"<string xmlns=\"http://schemas.microsoft.com/2003/10/Serialization/\">{text}</string>";
        }

        private Stream GetResponse(string fileName)
        {
            var path = Path.Combine(Environment.CurrentDirectory, "TestData", fileName);
            return File.OpenRead(path);
        }
    }
}
