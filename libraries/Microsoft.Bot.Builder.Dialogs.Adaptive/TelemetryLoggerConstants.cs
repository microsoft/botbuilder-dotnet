// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Builder.Dialogs.Adaptive
{
    /// <summary>
    /// Defines names of common adaptive dialog events for use with a <see cref="IBotTelemetryClient"/> object.
    /// </summary>
    public static class TelemetryLoggerConstants
    {
        /// <summary>
        /// The name of the event when an adaptive dialog trigger occurs.
        /// </summary>
        public static readonly string TriggerEvent = "AdaptiveDialogTrigger";
        
        /// <summary>
        /// The name of the event when an adaptive dialog complete occurs.
        /// </summary>
        public static readonly string CompleteEvent = "AdaptiveDialogComplete";

        /// <summary>
        /// The name of the event when an adaptive dialog cancel occurs.
        /// </summary>
        public static readonly string DialogCancelEvent = "AdaptiveDialogCancel";

        /// <summary>
        /// The name of the event when an adaptive dialog start occurs.
        /// </summary>
        public static readonly string DialogStartEvent = "AdaptiveDialogStart";

        /// <summary>
        /// The name of the event when an adaptive dialog action occurs.
        /// </summary>
        public static readonly string DialogActionEvent = "AdaptiveDialogAction";

        /// <summary>
        /// The name of the event when a Log Action result occurs.
        /// </summary>
        public static readonly string LogActionResultEvent = "LogActionResult";

        /// <summary>
        /// The name of the event when a Sent Activity result occurs.
        /// </summary>
        public static readonly string SendActivityResultEvent = "SendActivityResult";

        /// <summary>
        /// The name of the event when an Update Activity result occurs.
        /// </summary>
        public static readonly string UpdateActivityResultEvent = "UpdateActivityResult";

        /// <summary>
        /// The name of the event when an Input result occurs.
        /// </summary>
        public static readonly string InputDialogResultEvent = "InputDialogResult";

        /// <summary>
        /// The name of the event when an OAuth Input result occurs.
        /// </summary>
        public static readonly string OAuthInputResultEvent = "OAuthInputResult";

        /// <summary>
        /// The name of the event when a cross trained recognier set result occurs.
        /// </summary>
        public static readonly string CrossTrainedRecognizerSetResultEvent = "CrossTrainedRecognizerSetResult";

        /// <summary>
        /// The name of the event when a multi language recognier result occurs.
        /// </summary>
        public static readonly string MultiLanguageRecognizerResultEvent = "MultiLanguageRecognizerResult";

        /// <summary>
        /// The name of the event when a recognier set result occurs.
        /// </summary>
        public static readonly string RecognizerSetResultEvent = "RecognizerSetResult";

        /// <summary>
        /// The name of the event when a regex recognier result occurs.
        /// </summary>
        public static readonly string RegexRecognizerResultEvent = "RegexRecognizerResult";

        /// <summary>
        /// The name of the event when a value recognier result occurs.
        /// </summary>
        public static readonly string ValueRecognizerResultEvent = "ValueRecognizerResult";
    }
}
