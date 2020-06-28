using System.Globalization;
using Newtonsoft.Json;
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
        public static CultureInfo EvalLocaleFromDialogContext(this DialogContext dialogContext)
        {
            string locale = null;
            var turnExists = dialogContext.Context.TurnState.TryGetValue(Turn, out var turnContent);
            if (turnExists == true && turnContent != null)
            {
                var localeValue = (turnContent as JObject).SelectToken(LocalePath) as JValue;
                if (localeValue != null)
                {
                    locale = localeValue.ToObject<string>();
                }
            }

            try
            {
                return new CultureInfo(locale);
            }
            catch
            {
                // do nothing if locale is illegal
            }

            return null;
        }
    }
}
