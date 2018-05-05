using Microsoft.Bot.Builder;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Builder.Core.Extensions;

namespace Microsoft.Bot.Samples.Ai.QnA.Translator
{
    class CurrentUserState
    {
        public string Language { get; set; }
        public string Locale { get; set; }
    }
    public static class TranslatorLocaleHelper
    {
        private static readonly string[] _supportedLanguages = new string[] { "fr", "en" }; //Define supported Languages
        private static readonly  string[] _supportedLocales = new string[] { "fr-fr", "en-us" }; //Define supported locales
        private static string currentLanguage = null;
        private static string currentLocale = null;
        public static void SetLanguage(ITurnContext context, string language) => context.GetConversationState<CurrentUserState>().Language = language;
        public static void SetLocale(ITurnContext context, string locale) => context.GetConversationState<CurrentUserState>().Locale = locale;
        public static bool IsSupportedLanguage(string language) => _supportedLanguages.Contains(language);
        public static bool IsSupportedLocale(string locale) => _supportedLocales.Contains(locale);
        public static async Task<bool> CheckUserChangedLanguage(ITurnContext context)
        {
            bool changeLang = false;//logic implemented by developper to make a signal for language changing 
            //use a specific message from user to change language
            if (context.Activity.Type == ActivityTypes.Message)
            {
                var messageActivity = context.Activity.AsMessageActivity();
                if (messageActivity.Text.ToLower().StartsWith("set my language to"))
                {
                    changeLang = true;
                }
                if (changeLang)
                {
                    var newLang = messageActivity.Text.ToLower().Replace("set my language to", "").Trim();
                    if (!string.IsNullOrWhiteSpace(newLang)
                            && IsSupportedLanguage(newLang))
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
            }

            return false;
        }
        public static string GetActiveLanguage(ITurnContext context)
        {
            if (currentLanguage != null)
            {
                //user has specified a different language so update the bot state
                if (context.GetConversationState<CurrentUserState>() != null && currentLanguage != context.GetConversationState<CurrentUserState>().Language)
                {
                    SetLanguage(context, currentLanguage);
                }
            }
            if (context.Activity.Type == ActivityTypes.Message
                && context.GetConversationState<CurrentUserState>() != null && context.GetConversationState<CurrentUserState>().Language != null)
            {
                return context.GetConversationState<CurrentUserState>().Language;
            }

            return "en";
        }
        public static async Task<bool> CheckUserChangedLocale(ITurnContext context)
        {
            bool changeLocale = false;//logic implemented by developper to make a signal for language changing 
            //use a specific message from user to change language
            if (context.Activity.Type == ActivityTypes.Message)
            {
                var messageActivity = context.Activity.AsMessageActivity();
                if (messageActivity.Text.ToLower().StartsWith("set my locale to"))
                {
                    changeLocale = true;
                }
                if (changeLocale)
                {
                    var newLocale = messageActivity.Text.ToLower().Replace("set my locale to", "").Trim(); //extracted by the user using user state 
                    if (!string.IsNullOrWhiteSpace(newLocale)
                            && IsSupportedLocale(newLocale))
                    {
                        SetLocale(context, newLocale);
                        await context.SendActivity($@"Changing your language to {newLocale}");
                    }
                    else
                    {
                        await context.SendActivity($@"{newLocale} is not a supported locale.");
                    }
                    //intercepts message
                    return true;
                }
            }

            return false;
        }
        public static string GetActiveLocale(ITurnContext context)
        {
            if (currentLocale != null)
            {
                //the user has specified a different locale so update the bot state
                if (context.GetConversationState<CurrentUserState>() != null
                    && currentLocale != context.GetConversationState<CurrentUserState>().Locale)
                {
                    SetLocale(context, currentLocale);
                }
            }
            if (context.Activity.Type == ActivityTypes.Message
                && context.GetConversationState<CurrentUserState>() != null && context.GetConversationState<CurrentUserState>().Locale != null)
            {
                return context.GetConversationState<CurrentUserState>().Locale;
            }

            return "en-us";
        }
    }
}
