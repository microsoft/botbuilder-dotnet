// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Ai.Translation;
using Microsoft.Bot.Builder.Ai.Translation.PostProcessor;
using Microsoft.Bot.Builder.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.IntegrationTests.Ai.Translation
{
#if !RUNINTEGRATIONTESTS
    [Ignore("These integration tests run only when RUNINTEGRATIONTESTS is defined")]
#endif
    [TestClass]
    public class TranslatorTests
    {
        public string translatorKey = TestUtilities.GetKey("TRANSLATORKEY");

        [TestMethod]
        [TestCategory("AI")]
        [TestCategory("Translator")]
        public void Translator_InvalidArguments_NullTranslatorKey()
        {
            if (!EnvironmentVariablesDefined())
            {
                Assert.Inconclusive("Missing Translator Environment variables - Skipping test");
                return;
            }
            string translatorKey = null;
            Assert.ThrowsException<ArgumentNullException>(() => new Translator(translatorKey));
        }

        [TestMethod]
        [TestCategory("AI")]
        [TestCategory("Translator")]
        public void Translator_InvalidArguments_EmptyTranslatorKey()
        {
            if (!EnvironmentVariablesDefined())
            {
                Assert.Inconclusive("Missing Translator Environment variables - Skipping test");
                return;
            }
            string translatorKey = string.Empty;
            Assert.ThrowsException<ArgumentNullException>(() => new Translator(translatorKey));
        }

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
            if (!EnvironmentVariablesDefined())
            {
                Assert.Inconclusive("Missing Translator Environment variables - Skipping test");
                return;
            }

            Translator translator = new Translator(translatorKey);

            var sentence = "salut <literal>Jean Bouchier mon ami</literal>";

            var translatedSentence = await translator.TranslateArrayAsync(new string[] { sentence }, "fr", "en");
            Dictionary<string, List<string>> patterns = new Dictionary<string, List<string>>();
            patterns.Add("fr", new List<string>());
            PatternsPostProcessor postProcessor = new PatternsPostProcessor(patterns);
            PostProcessedDocument postProcessedDocument = postProcessor.Process(translatedSentence[0], "fr");
            Assert.IsNotNull(translatedSentence);
            Assert.AreEqual("Hi Jean Bouchier mon ami", postProcessedDocument.PostProcessedMessage);
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
            var translatedSentence = await translator.TranslateAsync(sentence, "fr", "en");
            Assert.IsNotNull(translatedSentence);
            Assert.AreEqual("Hi 20-10", translatedSentence.TargetMessage);
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
            if (!EnvironmentVariablesDefined())
            {
                Assert.Inconclusive("Missing Translator Environment variables - Skipping test");
                return;
            }

            Translator translator = new Translator(translatorKey);

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
            if (!EnvironmentVariablesDefined())
            {
                Assert.Inconclusive("Missing Translator Environment variables - Skipping test");
                return;
            }

            Translator translator = new Translator(translatorKey);

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
            if (!EnvironmentVariablesDefined())
            {
                Assert.Inconclusive("Missing Translator Environment variables - Skipping test");
                return;
            }

            Translator translator = new Translator(translatorKey);

            var sentence = "Arrange an appointment for tomorrow";
            await Assert.ThrowsExceptionAsync<ArgumentException>(async () =>
                await translator.TranslateAsync(sentence, "na", "de"));
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
                await translator.TranslateAsync(sentence, "en", "na"));
        }

        private bool EnvironmentVariablesDefined()
        {
            return translatorKey != null;
        }
    }
}
