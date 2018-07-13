// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Ai.Translation
{
    /// <summary>
    /// Middleware that translates from the input locale to a specified locale.
    /// </summary>
    public class LocaleConverterMiddleware : IMiddleware
    {
        private readonly ILocaleConverter _localeConverter;
        private readonly string _toLocale;
        private readonly Func<ITurnContext, string> _getUserLocale;
        private readonly Func<ITurnContext, Task<bool>> _setUserLocale;

        /// <summary>
        /// Initializes a new instance of the <see cref="LocaleConverterMiddleware"/> class.
        /// </summary>
        /// <param name="getUserLocale">Delegate for getting the user locale.</param>
        /// <param name="checkUserLocaleChanged">Delegate that returns true if the locale was
        /// changed (implements logic to change locale by intercepting the message).</param>
        /// <param name="toLocale">The target locale.</param>
        /// <param name="localeConverter">The locale converter to use.</param>
        /// <exception cref="ArgumentNullException">Thrown when any of the parameters is null.</exception>
        public LocaleConverterMiddleware(Func<ITurnContext, string> getUserLocale, Func<ITurnContext, Task<bool>> checkUserLocaleChanged, string toLocale, ILocaleConverter localeConverter)
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
            _getUserLocale = getUserLocale ?? throw new ArgumentNullException(nameof(getUserLocale));
            _setUserLocale = checkUserLocaleChanged ?? throw new ArgumentNullException(nameof(checkUserLocaleChanged));
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
            if (context.Activity.Type == ActivityTypes.Message)
            {
                var message = context.Activity.AsMessageActivity();
                if (message != null)
                {
                    if (!string.IsNullOrWhiteSpace(message.Text))
                    {
                        var localeChanged = await _setUserLocale(context).ConfigureAwait(false);
                        if (!localeChanged)
                        {
                            var fromLocale = _getUserLocale(context);
                            ConvertLocaleMessage(context, fromLocale);
                        }
                        else
                        {
                            // skip routing in case of user changed the locale
                            return;
                        }
                    }
                }
            }

            await next(cancellationToken).ConfigureAwait(false);
        }

        private void ConvertLocaleMessage(ITurnContext context, string fromLocale)
        {
            var message = context.Activity.AsMessageActivity();
            if (message != null)
            {
                if (_localeConverter.IsLocaleAvailable(fromLocale) && fromLocale != _toLocale)
                {
                    var localeConvertedText = _localeConverter.Convert(message.Text, fromLocale, _toLocale);
                    message.Text = localeConvertedText;
                }
            }
        }
    }
}
