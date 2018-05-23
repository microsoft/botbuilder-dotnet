// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Ai.Translation;
using Microsoft.Bot.Builder.Core.Extensions;
using Microsoft.Bot.Builder.Core.Extensions.Tests;
using Microsoft.Bot.Builder.Testing;
using Microsoft.Bot.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Ai.QnA.Tests
{
    class LanguageState { 
        public string Language { get; set; }
    }

    [TestClass]
    public class TranslatorMiddlewareTests
    {
        public string translatorKey = TestUtilities.GetKey("TRANSLATORKEY");

        [TestMethod]
        [TestCategory("AI")]
        [TestCategory("Translator")]
        public async Task TranslatorMiddleware_DetectAndTranslateToEnglish()
        {
            if (!EnvironmentVariablesDefined())
            {
                Assert.Inconclusive("Missing Translator Environment variables - Skipping test");
                return;
            }

            TestAdapter adapter = new TestAdapter() 
            .Use(new TranslationMiddleware(new string[] { "en-us" }, translatorKey));

            await new TestFlow(adapter, (context) =>
            {
                if (!context.Responded)
                {
                    context.SendActivity(context.Activity.AsMessageActivity().Text);
                }
                return Task.CompletedTask;
            })  
            .Send("salut")
                .AssertReply("Hello")
            .Send("salut 10-20")
                .AssertReply("Hi 10-20")
                .StartTest();
        }

        [TestMethod]
        [TestCategory("AI")]
        [TestCategory("Translator")]
        public async Task TranslatorMiddleware_TranslateFrenchToEnglish()
        {
            if (!EnvironmentVariablesDefined())
            {
                Assert.Inconclusive("Missing Translator Environment variables - Skipping test");
                return;
            }

            TestAdapter adapter = new TestAdapter()
                .Use(new UserState<LanguageState>(new MemoryStorage()))
                .Use(new TranslationMiddleware(new string[] { "en-us" }, translatorKey, new Dictionary<string, List<string>>(), GetActiveLanguage, SetActiveLanguage));

            await new TestFlow(adapter, (context) =>
            {
                if (!context.Responded)
                {
                    context.SendActivity(context.Activity.AsMessageActivity().Text);  
                }
                return Task.CompletedTask;
            })
            .Send("set my language to fr")
                .AssertReply("Changing your language to fr")
            .Send("salut")
                .AssertReply("Hello")
                .StartTest();
        }

        [TestMethod]
        [TestCategory("AI")]
        [TestCategory("Translator")]
        public async Task TranslatorMiddleware_TranslateFrenchToEnglishToUserLanguage()
        {
            if (!EnvironmentVariablesDefined())
            {
                Assert.Inconclusive("Missing Translator Environment variables - Skipping test");
                return;
            }

            TestAdapter adapter = new TestAdapter()
                .Use(new UserState<LanguageState>(new MemoryStorage()))
                .Use(new TranslationMiddleware(new string[] { "en-us" }, translatorKey, new Dictionary<string, List<string>>(), GetActiveLanguage, SetActiveLanguage,true));

            await new TestFlow(adapter, (context) =>
            {
                if (!context.Responded)
                {
                    context.SendActivity(context.Activity.AsMessageActivity().Text);
                }
                return Task.CompletedTask;
            })
            .Send("set my language to fr")
                .AssertReply("Changing your language to fr")
            .Send("salut")
                .AssertReply("Salut")
                .StartTest();
        }

        private void SetLanguage(ITurnContext context, string language) =>context.GetUserState<LanguageState>().Language = language ; 
       
        protected async Task<bool> SetActiveLanguage(ITurnContext context)
        {
            bool changeLang = false;//logic implemented by developper to make a signal for language changing 
            //use a specific message from user to change language 
            var messageActivity = context.Activity.AsMessageActivity();
            if (messageActivity.Text.ToLower().StartsWith("set my language to"))
            {
                changeLang = true;
            }
            if (changeLang)
            {
                var newLang = messageActivity.Text.ToLower().Replace("set my language to", "").Trim();
                if (!string.IsNullOrWhiteSpace(newLang))
                {
                    SetLanguage(context, newLang);
                    await context.SendActivity($@"Changing your language to {newLang}");
                }
                else
                {
                    await context.SendActivity($@"{newLang} is not a supported language.");
                }
                //intercepts message
                return true;
            }

            return false;
        }
        protected string GetActiveLanguage(ITurnContext context)
        {
            if (context.Activity.Type == ActivityTypes.Message
                && !string.IsNullOrEmpty(context.GetUserState<LanguageState>().Language))
            {
                return context.GetUserState<LanguageState>().Language;
            }

            return "en";
        }

        private bool EnvironmentVariablesDefined()
        {
            return translatorKey != null;
        }
    }
}
