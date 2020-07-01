// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Linq;
using Microsoft.Recognizers.Text;

namespace Microsoft.Bot.Builder.Dialogs.Prompts
{
    /// <summary>
    /// Class container for currently-supported Culture Models in Confirm and Choice Prompt.
    /// </summary>
    public static class PromptCultureModels
    {
        private static readonly string[] SupportedLocales = GetSupportedCultures().Select(c => c.Locale).ToArray();

        public static PromptCultureModel Bulgarian =>
            new PromptCultureModel
            {
                InlineOr = " или ",
                InlineOrMore = ", или ",
                Locale = Culture.Bulgarian,
                NoInLanguage = "Не",
                Separator = ", ",
                YesInLanguage = "да",
            };

        public static PromptCultureModel Chinese =>
            new PromptCultureModel
            {
                InlineOr = " 要么 ",
                InlineOrMore = "， 要么 ",
                Locale = Culture.Chinese,
                NoInLanguage = "不",
                Separator = "， ",
                YesInLanguage = "是的",
            };

        public static PromptCultureModel Dutch =>
            new PromptCultureModel
            {
                InlineOr = " of ",
                InlineOrMore = ", of ",
                Locale = Culture.Dutch,
                NoInLanguage = "Nee",
                Separator = ", ",
                YesInLanguage = "Ja",
            };

        public static PromptCultureModel English =>
            new PromptCultureModel
            {
                InlineOr = " or ",
                InlineOrMore = ", or ",
                Locale = Culture.English,
                NoInLanguage = "No",
                Separator = ", ",
                YesInLanguage = "Yes",
            };

        public static PromptCultureModel French =>
            new PromptCultureModel
            {
                InlineOr = " ou ",
                InlineOrMore = ", ou ",
                Locale = Culture.French,
                NoInLanguage = "Non",
                Separator = ", ",
                YesInLanguage = "Oui",
            };

        public static PromptCultureModel German =>
            new PromptCultureModel
            {
                InlineOr = " oder ",
                InlineOrMore = ", oder ",
                Locale = Culture.German,
                NoInLanguage = "Nein",
                Separator = ", ",
                YesInLanguage = "Ja",
            };

        public static PromptCultureModel Hindi =>
            new PromptCultureModel
            {
                InlineOr = " या ",
                InlineOrMore = ", या ",
                Locale = Culture.Hindi,
                NoInLanguage = "नहीं",
                Separator = ", ",
                YesInLanguage = "हां",
            };

        public static PromptCultureModel Italian =>
            new PromptCultureModel
            {
                InlineOr = " o ",
                InlineOrMore = " o ",
                Locale = Culture.Italian,
                NoInLanguage = "No",
                Separator = ", ",
                YesInLanguage = "Si",
            };

        public static PromptCultureModel Japanese =>
            new PromptCultureModel
            {
                InlineOr = " または ",
                InlineOrMore = "、 または ",
                Locale = Culture.Japanese,
                NoInLanguage = "いいえ",
                Separator = "、 ",
                YesInLanguage = "はい",
            };

        public static PromptCultureModel Korean =>
            new PromptCultureModel
            {
                InlineOr = " 또는 ",
                InlineOrMore = " 또는 ",
                Locale = Culture.Korean,
                NoInLanguage = "아니",
                Separator = ", ",
                YesInLanguage = "예",
            };

        public static PromptCultureModel Portuguese =>
            new PromptCultureModel
            {
                InlineOr = " ou ",
                InlineOrMore = ", ou ",
                Locale = Culture.Portuguese,
                NoInLanguage = "Não",
                Separator = ", ",
                YesInLanguage = "Sim",
            };

        public static PromptCultureModel Spanish =>
            new PromptCultureModel
            {
                InlineOr = " o ",
                InlineOrMore = ", o ",
                Locale = Culture.Spanish,
                NoInLanguage = "No",
                Separator = ", ",
                YesInLanguage = "Sí",
            };

        public static PromptCultureModel Swedish =>
            new PromptCultureModel
            {
                InlineOr = " eller ",
                InlineOrMore = " eller ",
                Locale = Culture.Swedish,
                NoInLanguage = "Nej",
                Separator = ", ",
                YesInLanguage = "Ja",
            };

        public static PromptCultureModel Turkish =>
            new PromptCultureModel
            {
                InlineOr = " veya ",
                InlineOrMore = " veya ",
                Locale = Culture.Turkish,
                NoInLanguage = "Hayır",
                Separator = ", ",
                YesInLanguage = "Evet",
            };

        /// <summary>
        /// Use Recognizers-Text to normalize various potential Locale strings to a standard.
        /// </summary>
        /// <remarks>
        /// This is mostly a copy/paste from https://github.com/microsoft/Recognizers-Text/blob/master/.NET/Microsoft.Recognizers.Text/Culture.cs#L66
        /// This doesn't directly use Recognizers-Text's MapToNearestLanguage because if they add language support before we do, it will break our prompts.
        /// </remarks>
        /// <param name="cultureCode">Represents locale. Examples: "en-US, en-us, EN".</param>
        /// <returns>Normalized locale.</returns>
        public static string MapToNearestLanguage(string cultureCode)
        {
#pragma warning disable CA1308 // Normalize strings to uppercase (culture codes are expected to be in lowercase, ignoring)
            cultureCode = cultureCode.ToLowerInvariant();
#pragma warning restore CA1308 // Normalize strings to uppercase

            if (SupportedLocales.All(o => o != cultureCode))
            {
                // Handle cases like EnglishOthers with cultureCode "en-*"
                var fallbackCultureCodes = SupportedLocales
                    .Where(o => o.EndsWith("*", StringComparison.Ordinal) &&
                                cultureCode.StartsWith(o.Split('-').First(), StringComparison.Ordinal)).ToList();

                if (fallbackCultureCodes.Count == 1)
                {
                    return fallbackCultureCodes.First();
                }

                // If there is no cultureCode like "-*", map only the prefix
                // For example, "es-mx" will be mapped to "es-es"
                fallbackCultureCodes = SupportedLocales
                    .Where(o => cultureCode.StartsWith(o.Split('-').First(), StringComparison.Ordinal)).ToList();

                if (fallbackCultureCodes.Any())
                {
                    return fallbackCultureCodes.First();
                }
            }

            return cultureCode;
        }

        public static PromptCultureModel[] GetSupportedCultures() => new PromptCultureModel[]
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
    }
}
