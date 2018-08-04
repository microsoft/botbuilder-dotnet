// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Ai.Translation
{
    /// <summary>
    /// Middleware to convert messages between different locales specified.
    /// </summary>
    public class LocaleConverterMiddleware : IMiddleware
    {
        private readonly ILocaleConverter _localeConverter;
        private readonly string _toLocale;
        private readonly IStatePropertyAccessor<string> _userLocaleProperty;

        /// <summary>
        /// Initializes a new instance of the <see cref="LocaleConverterMiddleware"/> class.
        /// </summary>
        /// <param name="userLocaleProperty">PropertyAccessor for the users preferred locale</param>
        /// <param name="toLocale">Target Locale</param>
        /// <param name="localeConverter">An ILocaleConverter instance </param>
        /// <param name="defaultLocale">Default locale to use when underlying user locale is undefined.</param>
        public LocaleConverterMiddleware(IStatePropertyAccessor<string> userLocaleProperty, string toLocale, ILocaleConverter localeConverter, string defaultLocale = "en-us")
        {
            if (string.IsNullOrWhiteSpace(defaultLocale))
            {
                throw new ArgumentNullException(nameof(defaultLocale));
            }

            _localeConverter = localeConverter ?? throw new ArgumentNullException(nameof(localeConverter));
            if (string.IsNullOrEmpty(toLocale))
            {
                throw new ArgumentNullException(nameof(toLocale));
            }
            else if (!localeConverter.IsLocaleAvailable(toLocale))
            {
                throw new ArgumentNullException("The locale " + nameof(toLocale) + " is unavailable");
            }

            DefaultLocale = defaultLocale;

            _toLocale = toLocale;
            _userLocaleProperty = userLocaleProperty ?? throw new ArgumentNullException(nameof(userLocaleProperty));
        }

        /// <summary>
        /// Gets the default locale to use when underlying user locale is undefined.
        /// </summary>
        /// <value>The default locale that will be used when the underlying user locale is undefined.</value>
        public string DefaultLocale { get; }

        /// <summary>
        /// Invoked on an incoming activity from the user in the context of the Bot Middleware pipeline.
        /// </summary>
        /// <param name="context">Context object containing information for a single turn of conversation with a user.</param>
        /// <param name="next">Used to invoke the next stage of the Middleware pipeline.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        public async Task OnTurnAsync(ITurnContext context, NextDelegate next, CancellationToken cancellationToken)
        {
            if (context.Activity.Type == ActivityTypes.Message)
            {
                IMessageActivity message = context.Activity.AsMessageActivity();
                if (message != null)
                {
                    if (!string.IsNullOrWhiteSpace(message.Text))
                    {
                        string userLocale = await _userLocaleProperty.GetAsync(context, () => DefaultLocale).ConfigureAwait(false);
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
