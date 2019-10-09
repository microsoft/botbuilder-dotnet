namespace Microsoft.Bot.Builder.Dialogs
{
    public class TurnPath
    {
        /// <summary>
        /// The result from the last dialog that was called
        /// </summary>
        public const string LASTRESULT = "turn.lastresult";

        /// <summary>
        /// The current activity for the turn
        /// </summary>
        public const string ACTIVITY = "turn.activity";

        /// <summary>
        /// The recognized result for the current turn
        /// </summary>
        public const string RECOGNIZED = "turn.recognized";

        /// <summary>
        /// Path to the top intent
        /// </summary>
        public const string TOPINTENT = "turn.recognized.intent";

        /// <summary>
        /// Path to the top score
        /// </summary>
        public const string TOPSCORE = "turn.recognized.score";

        /// <summary>
        /// If true an interruption has occured
        /// </summary>
        public const string INTERRUPTED = "turn.interrupted";

        /// <summary>
        /// The current dialog event (set during event processings)
        /// </summary>
        public const string DIALOGEVENT = "turn.dialogEvent";

        /// <summary>
        /// Used to track that we don't end up in infinite loop of RepeatDialogs()
        /// </summary>
        public const string REPEATEDIDS = "turn.repeatedIds";
    }
}
