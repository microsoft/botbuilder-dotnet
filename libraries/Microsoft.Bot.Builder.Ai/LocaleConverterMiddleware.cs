// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.


using Microsoft.Bot.Schema;
using System;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Ai
{
    /// <summary>
    /// Middleware to convert messages between different locales specified
    /// </summary>
    public class LocaleConverterMiddleware : IMiddleware
    {
        private ILocaleConverter localeConverter; 
        private readonly string toLocale;
        private readonly Func<ITurnContext, string> _getUserLocale;
        private readonly Func<ITurnContext, Task<bool>> _setUserLocale;

        /// <summary>
        /// Constructor for developer defined detection of user messages
        /// </summary>
        /// <param name="getUserLocale">Delegate for getting the user locale</param>
        /// <param name="setUserLocale">Delegate for setting the user language, returns true if the locale was changed (implements logic to change locale by intercepting the message)</param>
        /// <param name="toLocale">Target Locale</param>
        /// <param name="localeConverter">An ILocaleConverter instance</param>
        public LocaleConverterMiddleware(Func<ITurnContext, string> getUserLocale, Func<ITurnContext, Task<bool>> setUserLocale, string toLocale, ILocaleConverter localeConverter)
        {
            this.localeConverter = localeConverter ?? throw new ArgumentNullException(nameof(localeConverter));
            if (string.IsNullOrEmpty(toLocale))
                throw new ArgumentNullException(nameof(toLocale));
            this.toLocale = toLocale;
            this._getUserLocale = getUserLocale ?? throw new ArgumentNullException(nameof(getUserLocale)); 
            this._setUserLocale = setUserLocale ?? throw new ArgumentNullException(nameof(setUserLocale)); 
        }

        /// <summary>
        /// Incoming activity
        /// </summary>
        /// <param name="context"></param>
        /// <param name="next"></param>
        /// <returns></returns>
        public async Task OnProcessRequest(ITurnContext context, MiddlewareSet.NextDelegate next)
        {
            IMessageActivity message = context.Activity.AsMessageActivity();
            if (message != null)
            {
                if (!String.IsNullOrWhiteSpace(message.Text))
                {
                    string fromLocale = _getUserLocale(context);
                    await ConvertLocaleMessageAsync(message, fromLocale);
                    await _setUserLocale(context);
                }
            }
            await next().ConfigureAwait(false);
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
