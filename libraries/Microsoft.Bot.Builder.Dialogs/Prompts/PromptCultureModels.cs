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

        /// <summary>
        /// Gets the bulgarian prompt culture model.
        /// </summary>
        /// <value>Bulgarian prompt culture model.</value>
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

        /// <summary>
        /// Gets the chinese prompt culture model.
        /// </summary>
        /// <value>Chinese prompt culture model.</value>
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

        /// <summary>
        /// Gets the dutch prompt culture model.
        /// </summary>
        /// <value>Dutch prompt culture model.</value>
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

        /// <summary>
        /// Gets the english prompt culture model.
        /// </summary>
        /// <value>English prompt culture model.</value>
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

        /// <summary>
        /// Gets the french prompt culture model.
        /// </summary>
        /// <value>French prompt culture model.</value>
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

        /// <summary>
        /// Gets the german prompt culture model.
        /// </summary>
        /// <value>German prompt culture model.</value>
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

        /// <summary>
        /// Gets the hindi prompt culture model.
        /// </summary>
        /// <value>Hindi prompt culture model.</value>
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

        /// <summary>
        /// Gets the italian prompt culture model.
        /// </summary>
        /// <value>Italian prompt culture model.</value>
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

        /// <summary>
        /// Gets the japanese prompt culture model.
        /// </summary>
        /// <value>Japanese prompt culture model.</value>
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

        /// <summary>
        /// Gets the korean prompt culture model.
        /// </summary>
        /// <value>Korean prompt culture model.</value>
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

        /// <summary>
        /// Gets the portuguese prompt culture model.
        /// </summary>
        /// <value>Portuguese prompt culture model.</value>
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

        /// <summary>
        /// Gets the spanish prompt culture model.
        /// </summary>
        /// <value>Spanish prompt culture model.</value>
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
        /// Gets the swedish prompt culture model.
        /// </summary>
        /// <value>Swedish prompt culture model.</value>
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

        /// <summary>
        /// Gets the turkish prompt culture model.
        /// </summary>
        /// <value>Turkish prompt culture model.</value>
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
            if (!string.IsNullOrWhiteSpace(cultureCode))
            {
                cultureCode = cultureCode.ToLowerInvariant();

                if (!SupportedLocales.Contains(cultureCode))
                {
                    var culturePrefix = cultureCode.Split('-').First();
                    var fallbackLocales = SupportedLocales.Where(locale => locale.StartsWith(culturePrefix, StringComparison.Ordinal)).ToList();

                    if (fallbackLocales.Any())
                    {
                        // Handle cases like EnglishOthers with cultureCode "en-*"
                        if (fallbackLocales.FirstOrDefault(locale => locale.EndsWith("*", StringComparison.Ordinal)) is string genericLocale)
                        {
                            return genericLocale;
                        }

                        // If there is no cultureCode like "-*", map only the prefix
                        // For example, "es-mx" will be mapped to "es-es"
                        return fallbackLocales.First();
                    }
                }
            }

            return cultureCode;
        }

        /// <summary>
        /// Gets a list of the supported culture models.
        /// </summary>
        /// <returns>Array of <see cref="PromptCultureModel"/> with the supported cultures.</returns>
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
