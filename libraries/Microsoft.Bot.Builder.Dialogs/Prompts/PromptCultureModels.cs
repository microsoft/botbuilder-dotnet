using Microsoft.Recognizers.Text;

namespace Microsoft.Bot.Builder.Dialogs.Prompts
{
    /// <summary>
    /// Class container for currently-supported Culture Models in Confirm and Choice Prompt.
    /// </summary>
    public static class PromptCultureModels
    {
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
        /// This mostly exists so that you don't need to directly include Recognizers-Text in other class files (there are some name conflicts).
        /// </summary>
        /// <param name="culture">Represents locale. Examples: "en-US, en-us, EN".</param>
        /// <returns>Normalized locale.</returns>
        public static string MapToNearestLanguage(string culture) => Culture.MapToNearestLanguage(culture);

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
