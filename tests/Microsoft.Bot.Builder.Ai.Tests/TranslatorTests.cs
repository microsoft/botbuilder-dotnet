// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Storage;
using Microsoft.Bot.Builder.Tests;
using Microsoft.Bot.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Ai.Tests
{
    [TestClass]
    public class TranslatorTests
    {
        public string translatorKey = TestUtilities.GetKey("TRANSLATORKEY");

        [TestMethod]
        [TestCategory("AI")]
        [TestCategory("Translator")]
        public async Task Translator_DetectAndTranslateToEnglish()
        {
            Translator translator = new Translator(translatorKey);

            var sentence = "salut";
            var detectedLanguage = await translator.Detect(sentence);
            Assert.IsNotNull(detectedLanguage);
            Assert.AreEqual("fr", detectedLanguage, "should detect french language");

            var translatedSentence = await translator.Translate(sentence, detectedLanguage, "en");
            Assert.IsNotNull(translatedSentence);
            Assert.AreEqual("Hello", translatedSentence);
        }

        [TestMethod]
        [TestCategory("AI")]
        [TestCategory("Translator")]
        public async Task Translator_TranslateFrenchToEnglish()
        {

            Translator translator = new Translator(translatorKey);

            var sentence = "salut 20-10";
            var translatedSentence = await translator.Translate(sentence, "fr", "en");
            Assert.IsNotNull(translatedSentence);
            Assert.AreEqual("Hi 20-10", translatedSentence);
        }

        [TestMethod]
        [TestCategory("AI")]
        [TestCategory("Translator")]
        public async Task Translator_TranslateFrenchToEnglishArray()
        {

            Translator translator = new Translator(translatorKey);

            var sentences = new string[] { "salut", "au revoir" };
            var translatedSentences = await translator.TranslateArray(sentences, "fr", "en");
            Assert.IsNotNull(translatedSentences);
            Assert.AreEqual(translatedSentences.Length, 2, "should be 2 sentences");
            Assert.AreEqual("Hello", translatedSentences[0]);
            Assert.AreEqual("Good bye", translatedSentences[1]);
        }

        [TestMethod]
        [TestCategory("AI")]
        [TestCategory("Translator")]
        public async Task Translator_TranslateEnglishToFrench()
        {

            Translator translator = new Translator(translatorKey);

            var sentence = "hello";
            var translatedSentence = await translator.Translate(sentence, "en", "fr");
            Assert.IsNotNull(translatedSentence);
            Assert.AreEqual("Salut", translatedSentence);
        }

        [TestMethod]
        [TestCategory("AI")]
        [TestCategory("Translator")]
        public async Task Translator_TranslateEnglishToFrenchArray()
        {

            Translator translator = new Translator(translatorKey);

            var sentences = new string[] { "Hello", "Good bye" };
            var translatedSentences = await translator.TranslateArray(sentences, "en", "fr");
            Assert.IsNotNull(translatedSentences);
            Assert.AreEqual(translatedSentences.Length, 2, "should be 2 sentences");
            Assert.AreEqual("Salut", translatedSentences[0]);
            Assert.AreEqual("Au revoir", translatedSentences[1]);
        }

        [TestMethod]
        [TestCategory("AI")]
        [TestCategory("Translator")]
        public async Task Translator_InvalidSourceLanguage()
        {

            Translator translator = new Translator(translatorKey);

            var sentence = "Arrange an appointment for tomorrow";
            await Assert.ThrowsExceptionAsync<ArgumentException>(async () =>
                await translator.Translate(sentence, "na", "de"));
        }

        [TestMethod]
        [TestCategory("AI")]
        [TestCategory("Translator")]
        public async Task Translator_InvalidTargetLanguage()
        {

            Translator translator = new Translator(translatorKey);

            var sentence = "Arrange an appointment for tomorrow";
            await Assert.ThrowsExceptionAsync<ArgumentException>(async () =>
                await translator.Translate(sentence, "en", "na"));
        }
    }
}
