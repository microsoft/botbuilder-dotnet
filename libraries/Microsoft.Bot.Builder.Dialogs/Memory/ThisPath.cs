using System;

namespace Microsoft.Bot.Builder.Dialogs
{
    public class ThisPath
    {
        /// <summary>
        /// The options that were passed to the active dialog via options argument of BeginDialog.
        /// </summary>
        public const string Options = "this.options";

        [Obsolete]
        public const string OPTIONS = "this.options";
    }
}
