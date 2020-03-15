using System;

namespace Microsoft.Bot.Builder.Dialogs
{
    public class ScopePath
    {
        /// <summary>
        /// User memory scope root path.
        /// </summary>
        public const string User = "user";

        /// <summary>
        /// Conversation memory scope root path.
        /// </summary>
        public const string Conversation = "conversation";

        /// <summary>
        /// Dialog memory scope root path.
        /// </summary>
        public const string Dialog = "dialog";

        /// <summary>
        /// DialogClass memory scope root path.
        /// </summary>
        public const string DialogClass = "dialogclass";

        /// <summary>
        /// This memory scope root path.
        /// </summary>
        public const string This = "this";

        /// <summary>
        /// Class memory scope root path.
        /// </summary>
        public const string Class = "class";

        /// <summary>
        /// Settings memory scope root path.
        /// </summary>
        public const string Settings = "settings";

        /// <summary>
        /// Turn memory scope root path.
        /// </summary>
        public const string Turn = "turn";

        [Obsolete]
        public const string USER = "user";
        [Obsolete]
        public const string CONVERSATION = "conversation";
        [Obsolete]
        public const string DIALOG = "dialog";
        [Obsolete]
        public const string DIALOGCLASS = "dialogclass";
        [Obsolete]
        public const string THIS = "this";
        [Obsolete]
        public const string CLASS = "class";
        [Obsolete]
        public const string SETTINGS = "settings";
        [Obsolete]
        public const string TURN = "turn";
    }
}
