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
        private string fromLocale;
        private string toLocale;

        public LocaleConverterMiddleware(string fromLocale, string toLocale)
        {
            localeConverter = new LocaleConverter();
            this.fromLocale = fromLocale;
            this.toLocale = toLocale;
        }

        public async Task ReceiveActivity(IBotContext context, MiddlewareSet.NextDelegate next)
        {
            IMessageActivity message = context.Request.AsMessageActivity();
            if (message != null)
            {
                if (!String.IsNullOrWhiteSpace(message.Text))
                {
                    ((BotContext)context)["LocaleConversionOriginalMessage"] = message.Text;
                    await ConvertLocaleMessageAsync(message);
                }
            }
            await next().ConfigureAwait(false);
        }

        public static string GetOriginalMessage(IBotContext context)
        {
            string message =   ((BotContext)context)["LocaleConversionOriginalMessage"] as string;
            return message;
        }

        private async Task ConvertLocaleMessageAsync(IMessageActivity message)
        {
            if (localeConverter.IsLocaleAvailable(fromLocale) && localeConverter.IsLocaleAvailable(toLocale) && fromLocale != toLocale)
            {
                message.Text = await localeConverter.Convert(message.Text, fromLocale, toLocale);
            }
        }
    }
}
