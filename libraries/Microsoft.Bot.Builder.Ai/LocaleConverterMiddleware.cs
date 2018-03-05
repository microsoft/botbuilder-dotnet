using Microsoft.Bot.Builder.Middleware;
using Microsoft.Bot.Schema;
using System;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Ai
{
    /// <summary>
    /// Middleware to convert messages between different locales specified
    /// </summary>
    public class LocaleConverterMiddleware : IReceiveActivity
    {
        private LocaleConverter localeConverter; 
        private string toLocale;
        private readonly Func<IBotContext, string> _getUserLocale;
        private readonly Func<IBotContext, Task<bool>> _setUserLocale;

        public LocaleConverterMiddleware(Func<IBotContext, string> getUserLocale, Func<IBotContext, Task<bool>> setUserLocale, string toLocale)
        {
            localeConverter = new LocaleConverter(); 
            this.toLocale = toLocale;
            _getUserLocale = getUserLocale;
            _setUserLocale = setUserLocale;
        }

        public async Task ReceiveActivity(IBotContext context, MiddlewareSet.NextDelegate next)
        {
            IMessageActivity message = context.Request.AsMessageActivity();
            if (message != null)
            {
                if (!String.IsNullOrWhiteSpace(message.Text))
                {
                    string fromLocale = _getUserLocale(context);
                    ((BotContext)context)["LocaleConversionOriginalMessage"] = message.Text;
                    await ConvertLocaleMessageAsync(message, fromLocale);
                    var localeWasChanged = await _setUserLocale(context);
                    if (!localeWasChanged)
                    {   // if what the user said wasn't a directive to change the locale (or that directive failed), continue the pipeline
                        await next();
                    }
                }
            }
            await next().ConfigureAwait(false);
        }

        public static string GetOriginalMessage(IBotContext context)
        {
            string message =   ((BotContext)context)["LocaleConversionOriginalMessage"] as string;
            return message;
        }

        private async Task ConvertLocaleMessageAsync(IMessageActivity message,string fromLocale)
        {
            
            if (localeConverter.IsLocaleAvailable(fromLocale) && localeConverter.IsLocaleAvailable(toLocale) && fromLocale != toLocale)
            {
                message.Text = await localeConverter.Convert(message.Text, fromLocale, toLocale);
            }
        }
    }
}
