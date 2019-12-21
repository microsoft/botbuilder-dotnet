// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Recognizers;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Conditions
{
    /// <summary>
    /// Actions triggered when an Intent of "ChooseIntent" has been emitted by a recognizer.
    /// </summary>
    /// <remarks>
    /// This trigger is run when the utterance has triggered ambiguity between intents from multiple recognizers in a CrossTrainedRecognizerSet.
    /// </remarks>
    public class OnChooseIntent : OnIntent
    {
        [JsonProperty("$kind")]
        public new const string DeclarativeType = "Microsoft.OnChooseIntent";

        [JsonConstructor]
        public OnChooseIntent(List<Dialog> actions = null, string constraint = null, [CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
            : base(CrossTrainedRecognizerSet.ChooseIntent, actions: actions, constraint: constraint, callerPath: callerPath, callerLine: callerLine)
        {
        }
    }
}
