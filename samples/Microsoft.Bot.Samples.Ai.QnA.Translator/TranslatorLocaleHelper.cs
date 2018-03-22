using Microsoft.Bot.Builder;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Builder.Core.Extensions;
using Microsoft.Bot.Builder.Core.State;

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
        private static readonly string[] _supportedLocales = new string[] { "fr-fr", "en-us" }; //Define supported locales
        private static string currentLanguage = null;
        private static string currentLocale = null;
        public static async Task SetLanguage(IConversationStateManager conversationState, string language)
        {
            var currentUserState = await conversationState.Get<CurrentUserState>();
            currentUserState.Language = language;

            conversationState.Set(currentUserState);
            await conversationState.SaveChanges();
        }

        public static async Task SetLocale(IConversationStateManager conversationState, string locale)
        {
            var currentUserState = await conversationState.GetOrCreate<CurrentUserState>();
            currentUserState.Locale = locale;

            conversationState.Set(currentUserState);
            await conversationState.SaveChanges();
        }
        public static bool IsSupportedLanguage(string language) => _supportedLanguages.Contains(language);
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
                        await SetLanguage(context.ConversationState(), newLang);
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
        public static async Task<string> GetActiveLanguage(ITurnContext context)
        {
            var conversationState = context.ConversationState();
            var currentUserState = await conversationState.Get<CurrentUserState>();

            if (currentLanguage != null)
            {
                //user has specified a different language so update the bot state
                if (currentUserState != null && currentLanguage != currentUserState.Language)
                {
                    await SetLanguage(conversationState, currentLanguage);
                }
            }
            if (context.Activity.Type == ActivityTypes.Message
                && currentUserState != null && currentUserState.Language != null)
            {
                return currentUserState.Language;
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
                            && IsSupportedLanguage(newLocale))
                    {
                        await SetLocale(context.ConversationState(), newLocale);
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
        public static async Task<string> GetActiveLocale(ITurnContext context)
        {
            var conversationState = context.ConversationState();
            var currentUserState = await conversationState.Get<CurrentUserState>();

            if (currentLocale != null)
            {
                //the user has specified a different locale so update the bot state
                if (currentUserState != null
                    && currentLocale != currentUserState.Locale)
                {
                    await SetLocale(conversationState, currentLocale);
                }
            }
            if (context.Activity.Type == ActivityTypes.Message
                && currentUserState != null && currentUserState.Locale != null)
            {
                return currentUserState.Locale;
            }

            return "en-us";
        }
    }
}
