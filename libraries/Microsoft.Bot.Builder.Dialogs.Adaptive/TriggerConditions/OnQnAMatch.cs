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
    /// Actions triggered when an Intent of "AmbigiousIntent" has been emitted by the recognizer.
    /// </summary>
    /// <remarks>
    /// This trigger is run when the utterance has triggered ambiguity between multiple recognizers in a RecognizerSet.
    /// </remarks>
    public class OnQnAMatch : OnIntent
    {
        [JsonProperty("$kind")]
        public new const string DeclarativeType = "Microsoft.OnQnAMatch";

        [JsonConstructor]
        public OnQnAMatch(List<Dialog> actions = null, string constraint = null, [CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
            : base(QnAMakerRecognizer.QnAMatchIntent, actions: actions, constraint: constraint, callerPath: callerPath, callerLine: callerLine)
        {
        }
    }
}
