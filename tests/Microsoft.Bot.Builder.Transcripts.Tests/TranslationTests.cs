// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Ai.Translation;
using Microsoft.Bot.Builder.Core.Extensions;
using Microsoft.Bot.Builder.Core.Extensions.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Transcripts.Tests
{
    [TestClass]
    public class TranslationTests
    {
        private string translatorKey = TestUtilities.GetKey("TRANSLATORKEY_TRANSCRIPT");

        public TestContext TestContext { get; set; }

        [TestMethod]
        public async Task TranslateToEnglish()
        {
            if (!EnvironmentVariablesDefined())
            {
                Assert.Inconclusive("Missing Translator Environment variables - Skipping test");
                return;
            }

            var nativeLanguages = new string[] { "en-us" };
            var patterns = new Dictionary<string, List<string>>();

            var activities = TranscriptUtilities.GetFromTestContext(TestContext);

            TestAdapter adapter = new TestAdapter()
                .Use(new UserState<LanguageState>(new MemoryStorage()))
                .Use(new TranslationMiddleware(nativeLanguages, translatorKey, patterns, GetUserLanguage, SetUserLanguage, false));

            var flow = new TestFlow(adapter, async (context) => {
                if (!context.Responded)
                {
                    await context.SendActivity($"message: {context.Activity.Text}");
                }
            });

            await flow.Test(activities).StartTest();
        }
        
        [TestMethod]
        public async Task TranslateToUserLanguage()
        {
            if (!EnvironmentVariablesDefined())
            {
                Assert.Inconclusive("Missing Translator Environment variables - Skipping test");
                return;
            }

            var nativeLanguages = new string[] { };
            var patterns = new Dictionary<string, List<string>>();

            var activities = TranscriptUtilities.GetFromTestContext(TestContext);

            TestAdapter adapter = new TestAdapter()
                .Use(new UserState<LanguageState>(new MemoryStorage()))
                .Use(new TranslationMiddleware(nativeLanguages, translatorKey, patterns, GetUserLanguage, SetUserLanguage, true));

            var flow = new TestFlow(adapter, async (context) => {
                if (!context.Responded)
                {
                    await context.SendActivity($"message: {context.Activity.Text}");
                }
            });

            await flow.Test(activities).StartTest();
        }

        [TestMethod]
        public async Task LocaleConvertToEnglish()
        {
            var botLocale = "en-us";

            var activities = TranscriptUtilities.GetFromTestContext(TestContext);

            TestAdapter adapter = new TestAdapter()
                .Use(new UserState<LanguageState>(new MemoryStorage()))
                .Use(new LocaleConverterMiddleware(GetUserLanguage, SetUserLanguage, botLocale, LocaleConverter.Converter));

            var flow = new TestFlow(adapter, async (context) => {
                if (!context.Responded)
                {
                    await context.SendActivity($"message: {context.Activity.Text}");
                }
            });

            await flow.Test(activities).StartTest();
        }

        private Task<bool> SetUserLanguage(ITurnContext context)
        {
            var userMessage = context.Activity.Text.ToLowerInvariant();
            if (userMessage.StartsWith("set language "))
            {
                context.GetUserState<LanguageState>().Language = userMessage.Substring(13, 5);
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }

        private string GetUserLanguage(ITurnContext context)
        {
            return context.GetUserState<LanguageState>().Language ?? "en-us";
        }

        internal class LanguageState
        {
            public string Language { get; set; }
        }

        private bool EnvironmentVariablesDefined()
        {
            return translatorKey != null;
        }
    }
}
