// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Microsoft.Bot.Builder.Dialogs.Adaptive.QnA.Recognizers;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Recognizers;
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
        [JsonProperty("$kind")]
        public new const string DeclarativeType = "Microsoft.OnQnAMatch";

        [JsonConstructor]
        public OnQnAMatch(List<Dialog> actions = null, string condition = null, [CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
            : base(QnAMakerRecognizer.QnAMatchIntent, actions: actions, condition: condition, callerPath: callerPath, callerLine: callerLine)
        {
        }
    }
}
