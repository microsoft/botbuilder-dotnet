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
        /// <param name="checkUserLocaleChanged">Delegate that returns true if the locale was changed (implements logic to change locale by intercepting the message)</param>
        /// <param name="toLocale">Target Locale</param>
        /// <param name="localeConverter">An ILocaleConverter instance</param>
        public LocaleConverterMiddleware(Func<ITurnContext, string> getUserLocale, Func<ITurnContext, Task<bool>> checkUserLocaleChanged, string toLocale, ILocaleConverter localeConverter)
        {
            this.localeConverter = localeConverter ?? throw new ArgumentNullException(nameof(localeConverter));
            if (string.IsNullOrEmpty(toLocale) || !localeConverter.IsLocaleAvailable(toLocale))
                throw new ArgumentNullException(nameof(toLocale));
            this.toLocale = toLocale;
            this._getUserLocale = getUserLocale ?? throw new ArgumentNullException(nameof(getUserLocale)); 
            this._setUserLocale = checkUserLocaleChanged ?? throw new ArgumentNullException(nameof(checkUserLocaleChanged)); 
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
                    ConvertLocaleMessage(message, fromLocale);
                    await _setUserLocale(context);
                }
            }
            await next().ConfigureAwait(false);
        }

        private void ConvertLocaleMessage(IMessageActivity message,string fromLocale)
        {
            
            if (localeConverter.IsLocaleAvailable(fromLocale) && fromLocale != toLocale)
            {
                message.Text = localeConverter.Convert(message.Text, fromLocale, toLocale);
            }
        }
        
    }
}
