// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.LUIS
{
    /// <summary>
    /// Interface for Recognizers.
    /// This should be moved to the Core Bot Builder Library once it's stable enough
    /// </summary>
    public interface IRecognizer
    {
        /// <summary>
        /// Runs an utterance through a recognizer and returns the recognizer results
        /// </summary>
        /// <param name="utterance">utterance</param>
        /// <param name="ct">cancellation token</param>
        /// <returns>Recognizer Results</returns>
        Task<RecognizerResult> Recognize(string utterance, CancellationToken ct);
    }
}
