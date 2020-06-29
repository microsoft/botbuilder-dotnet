using System.Globalization;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Dialogs
{
    /// <summary>
    /// Provides extension methods for <see cref="DialogContext"/>.
    /// </summary>
    public static class DialogContextExtension
    {
        private const string Turn = "turn";
        private const string LocalePath = "$.activity.locale";

        /// <summary>
        /// Obtain the CultureInfo in DialogContext.
        /// </summary>
        /// <param name="dialogContext">The dialogContext to extract information.</param>
        /// <returns>A <see cref="CultureInfo"/> representing the current locale.</returns>
        public static string GetLocale(this DialogContext dialogContext)
        {
            string locale = null;
            object turnContent = null;
            var turnExists = dialogContext?.Context?.TurnState?.TryGetValue(Turn, out turnContent);
            if (turnExists == true && turnContent != null)
            {
                if ((turnContent as JObject)?.SelectToken(LocalePath) is JValue localeValue)
                {
                    locale = localeValue.ToObject<string>();
                }
            }

            return locale;
        }
    }
}
