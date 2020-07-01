// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Builder.Dialogs
{
#pragma warning disable CA1052 // Static holder types should be Static or NotInheritable (we can't change this without breaking binary compat)
    public class DialogPath
#pragma warning restore CA1052 // Static holder types should be Static or NotInheritable
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
        /// Default operation to use for entities where there is no identified operation entity.
        /// </summary>
        public const string DefaultOperation = "dialog.defaultOperation";

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
