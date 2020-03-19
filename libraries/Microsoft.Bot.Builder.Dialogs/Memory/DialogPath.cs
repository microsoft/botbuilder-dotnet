using System.Data;

namespace Microsoft.Bot.Builder.Dialogs
{
    public class DialogPath
    {
        /// <summary>
        /// Counter of emitted events.
        /// </summary>
        public const string EventCounter = "dialog.eventCounter";

        /// <summary>
        /// Currently expected properties.
        /// </summary>
        public const string ExpectedProperties = "dialog.expectedProperties";

        /// <summary>
        /// Last surfaced entity ambiguity event.
        /// </summary>
        public const string LastEvent = "dialog.lastEvent";

        /// <summary>
        /// Currently required properties.
        /// </summary>
        public const string RequiredProperties = "dialog.requiredProperties";

        /// <summary>
        /// Number of retries for the current Ask.
        /// </summary>
        public const string Retries = "dialog.retries";

        /// <summary>
        /// Last intent.
        /// </summary>
        public const string LastIntent = "dialog.lastIntent";

        /// <summary>
        /// Last trigger event: defined in FormEvent, ask, clarifyEntity etc..
        /// </summary>
        public const string LastTriggerEvent = "dialog.lastTriggerEvent";

        /// <summary>
        /// Utility function to get just the property name without the memory scope prefix.
        /// </summary>
        /// <param name="property">memory scope property path.</param>
        /// <returns>name of the property without the prefix.</returns>
        public static string GetPropertyName(string property)
        {
            return property.Replace("dialog.", string.Empty);
        }
    }
}
