// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;

namespace Microsoft.Bot.Builder.Dialogs
{
#pragma warning disable CA1052 // Static holder types should be Static or NotInheritable (We can't change this without breaking binary compat)
    public class TurnPath
#pragma warning restore CA1052 // Static holder types should be Static or NotInheritable
    {
        /// <summary>
        /// The result from the last dialog that was called.
        /// </summary>
        public const string LastResult = "turn.lastresult";

        /// <summary>
        /// The current activity for the turn.
        /// </summary>
        public const string Activity = "turn.activity";

        /// <summary>
        /// The recognized result for the current turn.
        /// </summary>
        public const string Recognized = "turn.recognized";

        /// <summary>
        /// Path to the top intent.
        /// </summary>
        public const string TopIntent = "turn.recognized.intent";

        /// <summary>
        /// Path to the top score.
        /// </summary>
        public const string TopScore = "turn.recognized.score";

        /// <summary>
        /// Original text.
        /// </summary>
        public const string Text = "turn.recognized.text";

        /// <summary>
        /// Original utterance split into unrecognized strings.
        /// </summary>
        public const string UnrecognizedText = "turn.unrecognizedText";

        /// <summary>
        /// Entities that were recognized from text.
        /// </summary>
        public const string RecognizedEntities = "turn.recognizedEntities";

        /// <summary>
        /// If true an interruption has occured.
        /// </summary>
        public const string Interrupted = "turn.interrupted";

        /// <summary>
        /// The current dialog event (set during event processing).
        /// </summary>
        public const string DialogEvent = "turn.dialogEvent";

        /// <summary>
        /// Used to track that we don't end up in infinite loop of RepeatDialogs().
        /// </summary>
        public const string RepeatedIds = "turn.repeatedIds";

        /// <summary>
        /// This is a bool which if set means that the turncontext.activity has been consumed by some component in the system.
        /// </summary>
        public const string ActivityProcessed = "turn.activityProcessed";

        [Obsolete("This property is deprecated, use TurnPath.LastResult instead.")]
        public const string LASTRESULT = "turn.lastresult";

        [Obsolete("This property is deprecated, use TurnPath.Activity instead.")]
        public const string ACTIVITY = "turn.activity";

        [Obsolete("This property is deprecated, use TurnPath.Recognized instead.")]
        public const string RECOGNIZED = "turn.recognized";

        [Obsolete("This property is deprecated, use TurnPath.TopIntent instead.")]
        public const string TOPINTENT = "turn.recognized.intent";

        [Obsolete("This property is deprecated, use TurnPath.TopScore instead.")]
        public const string TOPSCORE = "turn.recognized.score";

        [Obsolete("This property is deprecated, use TurnPath.Text instead.")]
        public const string TEXT = "turn.recognized.text";

        [Obsolete("This property is deprecated, use TurnPath.UnrecognizedText instead.")]
        public const string UNRECOGNIZEDTEXT = "turn.unrecognizedText";

        [Obsolete("This property is deprecated, use TurnPath.RecognizedEntities instead.")]
        public const string RECOGNIZEDENTITIES = "turn.recognizedEntities";

        [Obsolete("This property is deprecated, use TurnPath.Interrupted instead.")]
        public const string INTERRUPTED = "turn.interrupted";

        [Obsolete("This property is deprecated, use TurnPath.DialogEvent instead.")]
        public const string DIALOGEVENT = "turn.dialogEvent";

        [Obsolete("This property is deprecated, use TurnPath.RepeatedIds instead.")]
        public const string REPEATEDIDS = "turn.repeatedIds";

        [Obsolete("This property is deprecated, use TurnPath.ActivityProcessed instead.")]
        public const string ACTIVITYPROCESSED = "turn.activityProcessed";

        /// <summary>
        /// Utility function to get just the property name without the memory scope prefix.
        /// </summary>
        /// <param name="property">memory scope property path.</param>
        /// <returns>name of the property without the prefix.</returns>
        public static string GetPropertyName(string property)
        {
            return property.Replace("turn.", string.Empty);
        }
    }
}
