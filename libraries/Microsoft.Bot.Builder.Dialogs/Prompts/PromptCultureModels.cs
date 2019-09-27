using System;
using System.Globalization;
using System.Linq;
using Microsoft.Recognizers.Text;

namespace Microsoft.Bot.Builder.Dialogs.Prompts
{
    /// <summary>
    /// Class container for currently-supported Culture Models in Confirm and Choice Prompt.
    /// </summary>
    public static class PromptCultureModels
    {
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
        /// This is more or less a copy/paste of Recognizers-Text.Culture's MapToNearestLanguage, but needed as an override
        /// here so that we can support additional languages.
        /// </summary>
        /// <param name="cultureCode">Represents locale. Examples: "en-US, en-us, EN".</param>
        /// <returns>Normalized locale.</returns>
        public static string MapToNearestLanguage(string cultureCode) => Culture.MapToNearestLanguage(cultureCode);

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
