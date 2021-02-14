using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading;

namespace Microsoft.Bot.Builder.Dialogs.Localization
{
    /// <summary>
    /// fds.
    /// </summary>
    internal class StateLocaleResolver : ActivityLocaleResolver
    {
        private const string ConversationLocaleProperty = "conversation.locale";
        private const string UserLocaleProperty = "user.locale";
        private const string TurnLocaleProperty = "turn.locale";

        /// <inheritdoc/>
        public override CultureInfo Resolve(DialogContext dc)
        {
            string locale;

            try
            {
                // Conversation locale is the preferred choice if available
                if (dc.State.TryGetValue<string>(ConversationLocaleProperty, out locale) && !string.IsNullOrEmpty(locale))
                {
                    return new CultureInfo(locale);
                }

                // User locale is the preferred locale when there is no conversation specific locale
                if (dc.State.TryGetValue<string>(UserLocaleProperty, out locale) && !string.IsNullOrEmpty(locale))
                {
                    return new CultureInfo(locale);
                }

                // Fallback to turn locale for legacy reasons, supporting the deprecated TurnContext.Locale property
                if (dc.State.TryGetValue<string>(TurnLocaleProperty, out locale) && !string.IsNullOrEmpty(locale))
                {
                    return new CultureInfo(locale);
                }

                // If we found no locale in state, default to activity locale
                return base.Resolve(dc);
            }
            catch (CultureNotFoundException)
            {
                // If the activity didn't have locale information, stay the course to whatever was set as default locale
                return Thread.CurrentThread.CurrentCulture;
            }
        }
    }
}
