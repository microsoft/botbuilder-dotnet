namespace Microsoft.Bot.Builder.Dialogs
{
    public class TurnPath
    {
        /// <summary>
        /// The result from the last dialog that was called.
        /// </summary>
        public const string LASTRESULT = "turn.lastresult";

        /// <summary>
        /// The current activity for the turn.
        /// </summary>
        public const string ACTIVITY = "turn.activity";

        /// <summary>
        /// The recognized result for the current turn.
        /// </summary>
        public const string RECOGNIZED = "turn.recognized";

         /// <summary>
        /// Path to the top intent.
        /// </summary>
        public const string TOPINTENT = "turn.recognized.intent";

        /// <summary>
        /// Path to the top score.
        /// </summary>
        public const string TOPSCORE = "turn.recognized.score";

        /// <summary>
        /// Original text.
        /// </summary>
        public const string TEXT = "turn.recognized.text";

        /// <summary>
        /// Original utterance split into unrecognized strings.
        /// </summary>
        public const string UNRECOGNIZEDTEXT = "turn.unrecognizedText";

        /// <summary>
        /// Entities that were recognized from text.
        /// </summary>
        public const string RECOGNIZEDENTITIES = "turn.recognizedEntities";

        /// <summary>
        /// If true an interruption has occured.
        /// </summary>
        public const string INTERRUPTED = "turn.interrupted";

        /// <summary>
        /// The current dialog event (set during event processings).
        /// </summary>
        public const string DIALOGEVENT = "turn.dialogEvent";

        /// <summary>
        /// Used to track that we don't end up in infinite loop of RepeatDialogs().
        /// </summary>
        public const string REPEATEDIDS = "turn.repeatedIds";

        /// <summary>
        /// This is a bool which if set means that the turncontext.activity has been consumed by some component in the system.
        /// </summary>
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
