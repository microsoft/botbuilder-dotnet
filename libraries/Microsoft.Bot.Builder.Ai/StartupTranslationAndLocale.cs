using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Ai
{
    /*
     * StartupTranslationAndLocale base class for delegate functions used to change language for the translator middleware
     * 
     */
    public abstract class StartupTranslationAndLocale
    {
        private void SetLanguage(IBotContext context, string language) => context.State.User[@"Microsoft.API.translateTo"] =  language ;
        private void SetLocale(IBotContext context, string locale) => context.State.User[@"LocaleConverterMiddleware.fromLocale"] = locale;

        protected abstract   bool IsSupportedLanguage(string language);
        protected virtual async Task<bool> SetActiveLanguage(IBotContext context)
        {
            bool changeLang = true;//logic implemented by developper to make a signal for language changing 
            //use a specific message from user to change language
            if (changeLang)
            {
                var newLang = ""; //extracted by the user using user state 
                if (!string.IsNullOrWhiteSpace(newLang)
                        && IsSupportedLanguage(newLang))
                    {
                        SetLanguage(context, newLang);
                        context.Reply($@"Changing your language to {newLang}");
                    }
                    else
                    {
                        context.Reply($@"{newLang} is not a supported language.");
                    }   
                //intercepts message
                return true;
            }

            return false;
        }
        protected virtual string GetActiveLanguage(IBotContext context)
        {
            if (context.Request.Type == ActivityTypes.Message
                && context.State.User.ContainsKey(@"Microsoft.API.translateTo"))
            {
                return (string)context.State.User[@"Microsoft.API.translateTo"];
            }

            return "en";
        }
        protected virtual async Task<bool> SetActiveLocale(IBotContext context)
        {
            bool changeLocale = true;//logic implemented by developper to make a signal for language changing 
            //use a specific message from user to change language
            if (changeLocale)
            {
                var newLocale = ""; //extracted by the user using user state 
                if (!string.IsNullOrWhiteSpace(newLocale)
                        && IsSupportedLanguage(newLocale))
                {
                    SetLocale(context, newLocale);
                    context.Reply($@"Changing your language to {newLocale}");
                }
                else
                {
                    context.Reply($@"{newLocale} is not a supported locale.");
                }
                //intercepts message
                return true;
            }

            return false;
        }
        protected virtual string GetActiveLocale(IBotContext context)
        {
            if (context.Request.Type == ActivityTypes.Message
                && context.State.User.ContainsKey(@"LocaleConverterMiddleware.fromLocale"))
            {
                return (string)context.State.User[@"LocaleConverterMiddleware.fromLocale"];
            }

            return "en-us";
        }
    }
}
