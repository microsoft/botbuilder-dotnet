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
        private readonly ILocaleConverter _localeConverter; 
        private readonly string _toLocale;
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
            _localeConverter = localeConverter ?? throw new ArgumentNullException(nameof(localeConverter));
            if (string.IsNullOrEmpty(toLocale))
                throw new ArgumentNullException(nameof(toLocale));
            else if( !localeConverter.IsLocaleAvailable(toLocale))
                throw new ArgumentNullException("The locale " +nameof(toLocale)+" is unavailable");
            _toLocale = toLocale;
            _getUserLocale = getUserLocale ?? throw new ArgumentNullException(nameof(getUserLocale)); 
            _setUserLocale = checkUserLocaleChanged ?? throw new ArgumentNullException(nameof(checkUserLocaleChanged));
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
                    bool localeChanged = await _setUserLocale(context);
                    if (!localeChanged)
                    {
                        string fromLocale = _getUserLocale(context);
                        ConvertLocaleMessage(context, fromLocale);
                    }
                    
                }
            }
            await next().ConfigureAwait(false);
        }

        private void ConvertLocaleMessage(ITurnContext context,string fromLocale)
        {
            IMessageActivity message = context.Activity.AsMessageActivity();
            if (message != null)
            {
                if (_localeConverter.IsLocaleAvailable(fromLocale) && fromLocale != _toLocale)
                {
                    string localeConvertedText = _localeConverter.Convert(message.Text, fromLocale, _toLocale);
                    message.Text = localeConvertedText;
                }
            }
        }


    }
}
