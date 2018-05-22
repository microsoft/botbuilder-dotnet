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

            var translatedSentence = await translator.TranslateArray(new string[] { sentence }, "fr", "en");
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
        public async Task Translator_DictionaryTest()
        {
            if (!EnvironmentVariablesDefined())
            {
                Assert.Inconclusive("Missing Translator Environment variables - Skipping test");
                return;
            }

            Translator translator = new Translator(translatorKey);

            Dictionary<string, Dictionary<string, string>> userCustomDictonaries = new Dictionary<string, Dictionary<string, string>>();
            Dictionary<string, string> frenctDictionary = new Dictionary<string, string>();
            frenctDictionary.Add("éclair", "eclairs tart");
            userCustomDictonaries.Add("fr", frenctDictionary);
            Dictionary<string, string> italianDictionary = new Dictionary<string, string>();
            italianDictionary.Add("camera", "bedroom");
            userCustomDictonaries.Add("it", italianDictionary);

            IPostProcessor customDictionaryPostProcessor = new CustomDictionaryPostProcessor(userCustomDictonaries);

            var frenchSentence = "Je veux voir éclair";

            var translatedDocuments = await translator.TranslateArray(new string[] { frenchSentence }, "fr", "en");
            Assert.IsNotNull(translatedDocuments);
            string postProcessedMessage = customDictionaryPostProcessor.Process(translatedDocuments[0], "fr").PostProcessedMessage;
            Assert.IsNotNull(postProcessedMessage);
            Assert.AreEqual("I want to see eclairs tart", postProcessedMessage);

            var italianSentence = "Voglio fare una foto nella camera";

            translatedDocuments = await translator.TranslateArray(new string[] { italianSentence }, "it", "en");
            Assert.IsNotNull(translatedDocuments);
            postProcessedMessage = customDictionaryPostProcessor.Process(translatedDocuments[0], "it").PostProcessedMessage;
            Assert.IsNotNull(postProcessedMessage);
            Assert.AreEqual("I want to take a picture in the bedroom", postProcessedMessage);
        }

        [TestMethod]
        [TestCategory("AI")]
        [TestCategory("Translator")]
        public async Task Translator_PatternsAndDictionaryTest()
        {
            if (!EnvironmentVariablesDefined())
            {
                Assert.Inconclusive("Missing Translator Environment variables - Skipping test");
                return;
            }

            List<IPostProcessor> attachedPostProcessors = new List<IPostProcessor>();
            Translator translator = new Translator(translatorKey);
            Dictionary<string, List<string>> patterns = new Dictionary<string, List<string>>();
            List<string> frenchPatterns = new List<string> { "mon nom est (.+)" };

            patterns.Add("fr", frenchPatterns);
            IPostProcessor patternsPostProcessor = new PatternsPostProcessor(patterns);

            attachedPostProcessors.Add(patternsPostProcessor);

            Dictionary<string, Dictionary<string, string>> userCustomDictonaries = new Dictionary<string, Dictionary<string, string>>();
            Dictionary<string, string> frenctDictionary = new Dictionary<string, string>();
            frenctDictionary.Add("etat", "Eldad");
            userCustomDictonaries.Add("fr", frenctDictionary);
            IPostProcessor customDictionaryPostProcessor = new CustomDictionaryPostProcessor(userCustomDictonaries);

            attachedPostProcessors.Add(customDictionaryPostProcessor);

            var sentence = "mon nom est etat";

            var translatedDocuments = await translator.TranslateArray(new string[] { sentence }, "fr", "en");
            Assert.IsNotNull(translatedDocuments);
            string postProcessedMessage =  null;
            foreach (IPostProcessor postProcessor in attachedPostProcessors)
            {
                postProcessedMessage = postProcessor.Process(translatedDocuments[0], "fr").PostProcessedMessage;
            }
            Assert.IsNotNull(postProcessedMessage);
            Assert.AreEqual("My name is Eldad", postProcessedMessage);
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
            var translatedSentences = await translator.TranslateArray(sentences, "fr", "en");
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
            var translatedSentence = await translator.Translate(sentence, "en", "fr");
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
            var translatedSentences = await translator.TranslateArray(sentences, "en", "fr");
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
