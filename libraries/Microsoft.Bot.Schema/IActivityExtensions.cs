// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Schema
{
    /// <summary>
    /// Provides additional methods to work with <see cref="IActivity"/>.
    /// </summary>
    public static class IActivityExtensions
    {
        /// <summary>
        /// Gets the locale for the activity.
        /// </summary>
        /// <param name="activity">The <see cref="Activity"/> instance.</param>
        /// <returns>The locale for the activity.</returns>
        /// <remarks>
        /// The locale name is a combination of an ISO 639 two- or three-letter
        /// culture code associated with a language
        /// and an ISO 3166 two-letter subculture code associated with a
        /// country or region.
        /// The locale name can also correspond to a valid BCP-47 language tag.
        /// </remarks>
        public static string GetLocale(this IActivity activity)
        {
            return ((Activity)activity).Locale;
        }

        /// <summary>
        /// Sets the locale for the activity.
        /// </summary>
        /// <param name="activity">The <see cref="Activity"/> instance.</param>
        /// <param name="locale">The locale for the activity specified as a combination of an ISO 639 two- or three-letter
        /// culture code associated with a language.</param>
        public static void SetLocale(this IActivity activity, string locale)
        {
            ((Activity)activity).Locale = locale;
        }
    }
}
