// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Ai.Translation
{
    /// <summary>
    /// Middleware to convert messages between different locales specified
    /// </summary>
    public class LocaleConverterMiddleware : IMiddleware
    {
        private readonly ILocaleConverter _localeConverter;
        private readonly string _toLocale;
        private readonly IPropertyAccessor<string> _userLocaleProperty;

        /// <summary>
        /// Constructor for developer defined detection of user messages
        /// </summary>
        /// <param name="userLocaleProperty">PropertyAccessor for the users preferred locale</param>
        /// <param name="toLocale">Target Locale</param>
        /// <param name="localeConverter">An ILocaleConverter instance</param>
        public LocaleConverterMiddleware(IPropertyAccessor<string> userLocaleProperty, string toLocale, ILocaleConverter localeConverter)
        {
            _localeConverter = localeConverter ?? throw new ArgumentNullException(nameof(localeConverter));
            if (string.IsNullOrEmpty(toLocale))
            {
                throw new ArgumentNullException(nameof(toLocale));
            }
            else if (!localeConverter.IsLocaleAvailable(toLocale))
            {
                throw new ArgumentNullException("The locale " + nameof(toLocale) + " is unavailable");
            }

            _toLocale = toLocale;
            _userLocaleProperty = userLocaleProperty ?? throw new ArgumentNullException(nameof(userLocaleProperty));
        }

        /// <summary>
        /// Incoming activity
        /// </summary>
        /// <param name="context"></param>
        /// <param name="next"></param>
        /// <returns></returns>
        public async Task OnTurnAsync(ITurnContext context, NextDelegate next, CancellationToken cancellationToken)
        {
            if (context.Activity.Type == ActivityTypes.Message)
            {
                IMessageActivity message = context.Activity.AsMessageActivity();
                if (message != null)
                {
                    if (!string.IsNullOrWhiteSpace(message.Text))
                    {
                        string userLocale = await _userLocaleProperty.GetAsync(context).ConfigureAwait(false);
                        if (userLocale != _toLocale)
                        {
                            ConvertLocaleMessage(context, userLocale);
                        }
                    }
                }
            }

            await next(cancellationToken).ConfigureAwait(false);
        }

        private void ConvertLocaleMessage(ITurnContext context, string fromLocale)
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
