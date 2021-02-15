// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading;

namespace Microsoft.Bot.Builder.Dialogs.Localization
{
    /// <summary>
    /// Rich <see cref="LocaleResolver"/> that obtains locale information from different state scopes and 
    /// resolves the effective locale. Enables richer multilanguage experiences and user experience. 
    /// </summary>
    /// <remarks>
    /// The <see cref="StateLocaleResolver"/> has the following precedence ruyles for locale resolution:
    /// - Conversation locale: explicit locale preferences for the current conversation are the first choice. 
    ///     Example scenario 1: Carlos' preferred language is English, but chooses Spanish for a specific conversation with his family in South America.
    ///     Example scenario 2: Marie prefers French for most conversations, but speaks English for group conversations with an international meetup group.
    /// - User locale: explicit user preferences are the choice when there is no conversation locale. 
    ///     Example scenario: Stephane is fluent in German and English, but for this specific bot prefers English, even though their chat clients are configured for German. 
    /// - Turn locale: This is legacy which we continue to support for backward compatibility. 
    /// - Activity locale: We have no specific locale information at the user or conversation level. In this case, the activity is likely to have valuable locale information from the chat client. 
    /// - Default locale: When no other choice is available, the default locale will be the final choice.
    /// </remarks>
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
