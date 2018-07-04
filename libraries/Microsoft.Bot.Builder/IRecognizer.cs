// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder
{
    /// <summary>
    /// Interface for Recognizers.
    /// </summary>
    public interface IRecognizer
    {
        /// <summary>
        /// Runs an utterance through a recognizer and returns a generic recognizer result.
        /// </summary>
        /// <param name="utterance">Utterance to analyze.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>Analysis of utterance.</returns>
        Task<RecognizerResult> RecognizeAsync(string utterance, CancellationToken ct);

        /// <summary>
        /// Runs an utterance through a recognizer and returns a strongly typed recognizer result.
        /// </summary>
        /// <param name="utterance">Utterance to analyze.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>Analysis of utterance.</returns>
        Task<T> RecognizeAsync<T>(string utterance, CancellationToken ct)
            where T : IRecognizerConvert, new();
    }
}
