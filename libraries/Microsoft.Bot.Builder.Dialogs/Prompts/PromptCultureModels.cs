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
        private static readonly string[] SupportedCultureCodes = GetSupportedCultures().Select(c => c.Locale).ToArray();

        public static IPromptCultureModel Chinese =>
            new IPromptCultureModel
            {
                InlineOr = " 要么 ",
                InlineOrMore = "， 要么 ",
                Locale = Culture.Chinese,
                NoInLanguage = "不",
                Separator = "， ",
                YesInLanguage = "是的",
            };

        public static IPromptCultureModel Danish =>
            new IPromptCultureModel
            {
                InlineOr = " eller ",
                InlineOrMore = "， eller ",
                Locale = "da-DK",
                NoInLanguage = "Nej",
                Separator = "， ",
                YesInLanguage = "Ja",
            };

        public static IPromptCultureModel Dutch =>
            new IPromptCultureModel
            {
                InlineOr = " of ",
                InlineOrMore = ", of ",
                Locale = Culture.Dutch,
                NoInLanguage = "Nee",
                Separator = ", ",
                YesInLanguage = "Ja",
            };

        public static IPromptCultureModel English =>
            new IPromptCultureModel
            {
                InlineOr = " or ",
                InlineOrMore = ", or ",
                Locale = Culture.English,
                NoInLanguage = "No",
                Separator = ", ",
                YesInLanguage = "Yes",
            };

        public static IPromptCultureModel French =>
            new IPromptCultureModel
            {
                InlineOr = " ou ",
                InlineOrMore = ", ou ",
                Locale = Culture.French,
                NoInLanguage = "Non",
                Separator = ", ",
                YesInLanguage = "Oui",
            };

        public static IPromptCultureModel German =>
            new IPromptCultureModel
            {
                InlineOr = " oder ",
                InlineOrMore = ", oder ",
                Locale = Culture.German,
                NoInLanguage = "Nein",
                Separator = ", ",
                YesInLanguage = "Ja",
            };

        public static IPromptCultureModel Japanese =>
            new IPromptCultureModel
            {
                InlineOr = " または ",
                InlineOrMore = "、 または ",
                Locale = Culture.Japanese,
                NoInLanguage = "いいえ",
                Separator = "、 ",
                YesInLanguage = "はい",
            };

        public static IPromptCultureModel Portuguese =>
            new IPromptCultureModel
            {
                InlineOr = " ou ",
                InlineOrMore = ", ou ",
                Locale = Culture.Portuguese,
                NoInLanguage = "Não",
                Separator = ", ",
                YesInLanguage = "Sim",
            };

        public static IPromptCultureModel Spanish =>
            new IPromptCultureModel
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
        /// This is more or less a copy/paste of Recognizers-Text.Culture's MapToNearestLanguage, but needed as an override
        /// here so that we can support additional languages.
        /// </summary>
        /// <param name="cultureCode">Represents locale. Examples: "en-US, en-us, EN".</param>
        /// <returns>Normalized locale.</returns>
        public static string MapToNearestLanguage(string cultureCode)
        {
            cultureCode = cultureCode.ToLowerInvariant();

            if (SupportedCultureCodes.All(o => o != cultureCode))
            {
                // Handle cases like EnglishOthers with cultureCode "en-*"
                var fallbackCultureCodes = SupportedCultureCodes
                    .Where(o => o.EndsWith("*", StringComparison.Ordinal) &&
                                cultureCode.StartsWith(o.Split('-').First(), StringComparison.Ordinal)).ToList();

                if (fallbackCultureCodes.Count == 1)
                {
                    return fallbackCultureCodes.First();
                }

                // If there is no cultureCode like "-*", map only the prefix
                // For example, "es-mx" will be mapped to "es-es"
                fallbackCultureCodes = SupportedCultureCodes
                    .Where(o => cultureCode.StartsWith(o.Split('-').First(), StringComparison.Ordinal)).ToList();

                if (fallbackCultureCodes.Any())
                {
                    return fallbackCultureCodes.First();
                }
            }

            return cultureCode;
        }

        public static IPromptCultureModel[] GetSupportedCultures() => new IPromptCultureModel[]
            {
                Chinese,
                Danish,
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
