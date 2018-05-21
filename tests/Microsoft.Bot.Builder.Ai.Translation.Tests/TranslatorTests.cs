// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Builder.Ai.Translation.PostProcessor;
using Microsoft.Bot.Builder.Core.Extensions.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Ai.Translation.Tests
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
            if (!EnvironmentVariablesDefined())
            {
                Assert.Inconclusive("Missing Translator Environment variables - Skipping test");
                return;
            }

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
        public async Task Translator_LiteralTagTest()
        {
            if (!EnvironmentVariablesDefined())
            {
                Assert.Inconclusive("Missing Translator Environment variables - Skipping test");
                return;
            }

            Translator translator = new Translator(translatorKey);

            var sentence = "salut <literal>Jean Bouchier mon ami</literal>";

            var translatedSentence = await translator.TranslateArray(new string[] { sentence }, "fr", "en");
            Assert.IsNotNull(translatedSentence);
            Assert.AreEqual("Hi Jean Bouchier mon ami", translatedSentence[0]);
        }

        [TestMethod]
        [TestCategory("AI")]
        [TestCategory("Translator")]
        public async Task Translator_PatternsTest()
        {
            if (!EnvironmentVariablesDefined())
            {
                Assert.Inconclusive("Missing Translator Environment variables - Skipping test");
                return;
            }

            Translator translator = new Translator(translatorKey);
            Dictionary<string, List<string>> patterns = new Dictionary<string, List<string>>();
            List<string> spanishPatterns = new List<string> { "perr[oa]" };
            patterns.Add("es", spanishPatterns);
            List<string> frenchPatterns = new List<string> { "mon nom est (.+)" };
            patterns.Add("fr", frenchPatterns);


            IPostProcessor patternsPostProcessor = new PatternsPostProcessor(patterns);
            var sentence = "mi perro se llama Enzo";

            var translatedDocuments = await translator.TranslateArray(new string[] { sentence }, "es", "en");
            Assert.IsNotNull(translatedDocuments);
            string postProcessedMessage = patternsPostProcessor.Process(translatedDocuments[0], "es").PostProcessedMessage;
            Assert.IsNotNull(postProcessedMessage);
            Assert.AreEqual("My perro's name is Enzo", postProcessedMessage);

            
            sentence = "mon nom est l'etat";
            translatedDocuments = await translator.TranslateArray(new string[] { sentence }, "fr", "en");
            Assert.IsNotNull(translatedDocuments);
            postProcessedMessage = patternsPostProcessor.Process(translatedDocuments[0], "fr").PostProcessedMessage;
            Assert.IsNotNull(postProcessedMessage);
            Assert.AreEqual("My name is l'etat", postProcessedMessage);
        }

        [TestMethod]
        [TestCategory("AI")]
        [TestCategory("Translator")]
        public async Task Translator_TranslateFrenchToEnglish()
        {
            if (!EnvironmentVariablesDefined())
            {
                Assert.Inconclusive("Missing Translator Environment variables - Skipping test");
                return;
            }

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
            if (!EnvironmentVariablesDefined())
            {
                Assert.Inconclusive("Missing Translator Environment variables - Skipping test");
                return;
            }

            Translator translator = new Translator(translatorKey);

            var sentences = new string[] { "mon nom est", "salut", "au revoir" };
            var translatedSentences = await translator.TranslateArray(sentences, "fr", "en");
            Assert.IsNotNull(translatedSentences);
            Assert.AreEqual(translatedSentences.Count, 3, "should be 3 sentences");
            Assert.AreEqual("Hello", translatedSentences[0].TargetMessage);
            Assert.AreEqual("Hello", translatedSentences[1].TargetMessage);
            Assert.AreEqual("Good bye", translatedSentences[2].TargetMessage);
        }

        [TestMethod]
        [TestCategory("AI")]
        [TestCategory("Translator")]
        public async Task Translator_TranslateEnglishToFrench()
        {
            if (!EnvironmentVariablesDefined())
            {
                Assert.Inconclusive("Missing Translator Environment variables - Skipping test");
                return;
            }

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
            if (!EnvironmentVariablesDefined())
            {
                Assert.Inconclusive("Missing Translator Environment variables - Skipping test");
                return;
            }

            Translator translator = new Translator(translatorKey);

            var sentences = new string[] { "Hello", "Good bye" };
            var translatedSentences = await translator.TranslateArray(sentences, "en", "fr");
            Assert.IsNotNull(translatedSentences);
            Assert.AreEqual(translatedSentences.Count, 2, "should be 2 sentences");
            Assert.AreEqual("Salut", translatedSentences[0]);
            Assert.AreEqual("Au revoir", translatedSentences[1]);
        }

        [TestMethod]
        [TestCategory("AI")]
        [TestCategory("Translator")]
        public async Task Translator_InvalidSourceLanguage()
        {
            if (!EnvironmentVariablesDefined())
            {
                Assert.Inconclusive("Missing Translator Environment variables - Skipping test");
                return;
            }

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
            if (!EnvironmentVariablesDefined())
            {
                Assert.Inconclusive("Missing Translator Environment variables - Skipping test");
                return;
            }

            Translator translator = new Translator(translatorKey);

            var sentence = "Arrange an appointment for tomorrow";
            await Assert.ThrowsExceptionAsync<ArgumentException>(async () =>
                await translator.Translate(sentence, "en", "na"));
        }

        private bool EnvironmentVariablesDefined()
        {
            return translatorKey != null;
        }
    }
}
