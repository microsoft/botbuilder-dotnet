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
        private readonly IStatePropertyAccessor<string> _userLocaleProperty;

        /// <summary>
        /// Constructor for developer defined detection of user messages
        /// </summary>
        /// <param name="userLocaleProperty">PropertyAccessor for the users preferred locale</param>
        /// <param name="toLocale">Target Locale</param>
        /// <param name="localeConverter">An ILocaleConverter instance</param>
        public LocaleConverterMiddleware(IStatePropertyAccessor<string> userLocaleProperty, string toLocale, ILocaleConverter localeConverter)
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
        /// Processess an incoming activity.
        /// </summary>
        /// <param name="context">The context object for this turn.</param>
        /// <param name="next">The delegate to call to continue the bot middleware pipeline.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <remarks>This middleware converts the text of incoming message activities to the target locale
        /// on the leading edge of the middleware pipeline.</remarks>
        public async Task OnTurnAsync(ITurnContext context, NextDelegate next, CancellationToken cancellationToken)
        {
            if (context.Activity is MessageActivity message)
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

            await next(cancellationToken).ConfigureAwait(false);
        }

        private void ConvertLocaleMessage(ITurnContext context, string fromLocale)
        {
            if (context.Activity is MessageActivity message)
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
