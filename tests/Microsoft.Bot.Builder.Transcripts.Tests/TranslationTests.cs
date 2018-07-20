// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Ai.Translation;
using Microsoft.Bot.Builder.Tests;
using Microsoft.Bot.Schema;
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

            var activities = await TranscriptUtilities.GetFromTestContextAsync(TestContext);

            var userState = new UserState(new MemoryStorage());
            var userLangProp = userState.CreateProperty<string>("language");

            TestAdapter adapter = new TestAdapter()
                .Use(userState)
                .Use(new TranslationMiddleware(nativeLanguages, translatorKey, patterns, new CustomDictionary(), userLangProp, false));

            var flow = new TestFlow(adapter, async (context) =>
            {
                if (!context.Responded)
                {
                    await context.SendActivityAsync($"message: {(context.Activity as MessageActivity).Text}");
                }
            });

            await flow.Test(activities).StartTestAsync();
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

            var activities = await TranscriptUtilities.GetFromTestContextAsync(TestContext);
            var userState = new UserState(new MemoryStorage());
            var userLangProp = userState.CreateProperty<string>("language");

            TestAdapter adapter = new TestAdapter()
                .Use(userState)
                .Use(new TranslationMiddleware(nativeLanguages, translatorKey, patterns, new CustomDictionary(), userLangProp, true));

            var flow = new TestFlow(adapter, async (context) =>
            {
                if (!context.Responded)
                {
                    await context.SendActivityAsync($"message: {(context.Activity as MessageActivity).Text}");
                }
            });

            await flow.Test(activities).StartTestAsync();
        }

        [TestMethod]
        public async Task LocaleConvertToEnglish()
        {
            var botLocale = "en-us";

            var activities = await TranscriptUtilities.GetFromTestContextAsync(TestContext);

            var userState = new UserState(new MemoryStorage());
            var userLangProp = userState.CreateProperty<string>("language", () => "en-us");

            TestAdapter adapter = new TestAdapter()
                .Use(userState)
                .Use(new LocaleConverterMiddleware(userLangProp, botLocale, LocaleConverter.Converter));

            var flow = new TestFlow(adapter, async (context) =>
            {
                if (context.Activity is MessageActivity userMessage)
                {
                    var userMessageText = userMessage.Text;
                    if (userMessageText.StartsWith("set language ", StringComparison.OrdinalIgnoreCase))
                    {
                        await userLangProp.SetAsync(context, userMessageText.Substring(13, 5));
                    }
                    else
                    {
                        await context.SendActivityAsync($"message: {userMessageText}");
                    }
                }
            });

            await flow.Test(activities, (expected, actual) =>
            {
                Assert.AreEqual((expected as MessageActivity).Text, (actual as MessageActivity).Text);
            }).StartTestAsync();
        }

        private bool EnvironmentVariablesDefined()
        {
            return translatorKey != null;
        }
    }
}
