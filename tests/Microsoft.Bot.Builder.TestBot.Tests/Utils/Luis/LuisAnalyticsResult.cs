// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;

namespace Microsoft.BotBuilderSamples.Tests.Utils.Luis
{
    public class LuisAnalyticsResult
    {
        public LuisAnalyticsResult(List<string> incorrectPredictions, List<string> unclearPredictions, List<string> imbalancedIntents)
        {
            IncorrectPredictions = incorrectPredictions;
            UnclearPredictions = unclearPredictions;
            ImbalancedIntents = imbalancedIntents;
        }

        /// <summary>
        /// Gets a list of the intents that had the highest percentage of incorrect predictions.
        /// Consider revising the incorrectly predicted utterances in these intents.
        /// </summary>
        /// <value>
        /// TODO.
        /// </value>
        public List<string> IncorrectPredictions { get; }

        /// <summary>
        /// Gets a list of the intents that had many fewer utterances than other intents in your app, which can weigh predictions away from this intent.
        /// Consider adding more utterances to those intents.
        /// </summary>
        /// <value>
        /// TODO.
        /// </value>
        public List<string> ImbalancedIntents { get; }

        /// <summary>
        /// Gets a list of the intents that had the highest percentage of unclear predictions.
        /// Consider revising the unclear utterances in these intents.
        /// </summary>
        /// <value>
        /// TODO.
        /// </value>
        public List<string> UnclearPredictions { get; }
    }
}
