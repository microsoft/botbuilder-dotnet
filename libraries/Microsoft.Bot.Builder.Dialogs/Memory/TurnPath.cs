using System;

namespace Microsoft.Bot.Builder.Dialogs
{
    public class TurnPath
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
        /// The current dialog event (set during event processings).
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

        [Obsolete]
        public const string LASTRESULT = "turn.lastresult";
        [Obsolete]
        public const string ACTIVITY = "turn.activity";
        [Obsolete]
        public const string RECOGNIZED = "turn.recognized";
        [Obsolete]
        public const string TOPINTENT = "turn.recognized.intent";
        [Obsolete]
        public const string TOPSCORE = "turn.recognized.score";
        [Obsolete]
        public const string TEXT = "turn.recognized.text";
        [Obsolete]
        public const string UNRECOGNIZEDTEXT = "turn.unrecognizedText";
        [Obsolete]
        public const string RECOGNIZEDENTITIES = "turn.recognizedEntities";
        [Obsolete]
        public const string INTERRUPTED = "turn.interrupted";
        [Obsolete]
        public const string DIALOGEVENT = "turn.dialogEvent";
        [Obsolete]
        public const string REPEATEDIDS = "turn.repeatedIds";
        [Obsolete]
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
