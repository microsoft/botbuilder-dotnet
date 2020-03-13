// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Linq;
using Microsoft.Bot.Builder.Dialogs.Prompts;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static Microsoft.Bot.Builder.Dialogs.Prompts.PromptCultureModels;

namespace Microsoft.Bot.Builder.Dialogs.Tests
{
    [TestClass]
    [TestCategory("Prompts")]
    [TestCategory("Prompt Culture Models Tests")]
    public class PromptCultureModelsTests
    {
        [TestMethod]
        [DynamicData(nameof(GetLocaleVariationTest), DynamicDataSourceType.Method)]
        public void ShouldCorrectlyMapToNearesLanguage(string localeVariation, string expectedResult)
        {
            var result = MapToNearestLanguage(localeVariation);
            Assert.AreEqual(expectedResult, result);
        }

        [TestMethod]
        public void ShouldReturnAllSupportedCultures()
        {
            var expected = new PromptCultureModel[]
            {
                Bulgarian,
                Chinese,
                Dutch,
                English,
                French,
                German,
                Hindi,
                Italian,
                Japanese,
                Korean,
                Portuguese,
                Spanish,
                Swedish,
                Turkish
            };

            var supportedCultures = GetSupportedCultures();

            Assert.IsTrue(expected.All(expectedCulture =>
            {
                return supportedCultures.Any(supportedCulture => expectedCulture.Locale == supportedCulture.Locale);
            }));

            Assert.IsTrue(supportedCultures.All(supportedCulture =>
            {
                return expected.Any(expectedCulture => supportedCulture.Locale == expectedCulture.Locale);
            }));
        }

        private static IEnumerable<object[]> GetLocaleVariationTest()
        {
            var testLocales = new TestLocale[]
            {
                new TestLocale(Bulgarian),
                new TestLocale(Chinese),
                new TestLocale(Dutch),
                new TestLocale(English),
                new TestLocale(French),
                new TestLocale(German),
                new TestLocale(Hindi),
                new TestLocale(Italian),
                new TestLocale(Japanese),
                new TestLocale(Korean),
                new TestLocale(Portuguese),
                new TestLocale(Spanish),
                new TestLocale(Swedish),
                new TestLocale(Turkish)
            };

            foreach (var locale in testLocales)
            {
                yield return new object[] { locale.ValidLocale, locale.Culture.Locale };
                yield return new object[] { locale.CapEnding, locale.Culture.Locale };
                yield return new object[] { locale.TitleEnding, locale.Culture.Locale };
                yield return new object[] { locale.CapTwoLetter, locale.Culture.Locale };
                yield return new object[] { locale.LowerTwoLetter, locale.Culture.Locale };
            }
        }
    }
}
