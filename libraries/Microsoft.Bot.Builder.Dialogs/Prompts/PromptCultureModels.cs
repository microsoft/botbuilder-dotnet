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
            cultureCode = cultureCode.ToLowerInvariant();

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
            Chinese,
            Dutch,
            English,
            French,
            German,
            Japanese,
            Portuguese,
            Spanish,
        };
    }
}
