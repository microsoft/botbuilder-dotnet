// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Conditions
{
    /// <summary>
    /// Actions triggered when an Intent of "QnAMatch" has been emitted by the QnAMatchRecognizer.
    /// </summary>
    /// <remarks>
    /// This trigger is run when the QnAMakerRecognizer has returned a QnAMatch intent. The entity @answer will have the QnAMaker answer.
    /// </remarks>
    public class OnQnAMatch : OnIntent
    {
        /// <summary>
        /// Class identifier.
        /// </summary>
        [JsonProperty("$kind")]
        public new const string Kind = "Microsoft.OnQnAMatch";

        // this is a duplicate of QnAMakerRecognizer.QnAMatchIntent, but copying this here removes need to have dependency between QnA and Adaptive assemblies.
        internal const string QnAMatchIntent = "QnAMatch";

        /// <summary>
        /// Initializes a new instance of the <see cref="OnQnAMatch"/> class.
        /// </summary>
        /// <param name="actions">Optional, actions to add to the plan when the rule constraints are met.</param>
        /// <param name="condition">Optional, condition which needs to be met for the actions to be executed.</param>
        /// <param name="callerPath">Optional, source file full path.</param>
        /// <param name="callerLine">Optional, line number in source file.</param>
        [JsonConstructor]
        public OnQnAMatch(List<Dialog> actions = null, string condition = null, [CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
            : base(QnAMatchIntent, actions: actions, condition: condition, callerPath: callerPath, callerLine: callerLine)
        {
        }
    }
}
