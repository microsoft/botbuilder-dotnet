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
        /// <param name="turnContext">Turn context.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Analysis of utterance.</returns>
        Task<RecognizerResult> RecognizeAsync(ITurnContext turnContext, CancellationToken cancellationToken);

        /// <summary>
        /// Runs an utterance through a recognizer and returns a strongly-typed recognizer result.
        /// </summary>
        /// <typeparam name="T">The recognition result type.</typeparam>
        /// <param name="turnContext">Turn context.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Analysis of utterance.</returns>
        Task<T> RecognizeAsync<T>(ITurnContext turnContext, CancellationToken cancellationToken)
            where T : IRecognizerConvert, new();
    }
}
