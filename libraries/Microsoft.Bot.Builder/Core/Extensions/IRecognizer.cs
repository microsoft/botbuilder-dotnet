// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Core.Extensions
{
    /// <summary>
    /// Can convert from a generic recognizer result to a strongly typed one.
    /// </summary>
    public interface IRecognizerConvert
    {
        /// <summary>
        /// Convert recognizer result.
        /// </summary>
        /// <param name="result">Result to convert.</param>
        void Convert(dynamic result);
    }

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
        Task<RecognizerResult> Recognize(string utterance, CancellationToken ct);

        /// <summary>
        /// Runs an utterance through a recognizer and returns a strongly typed recognizer result.
        /// </summary>
        /// <param name="utterance">Utterance to analyze.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>Analysis of utterance.</returns>
        Task<T> Recognize<T>(string utterance, CancellationToken ct)
            where T : IRecognizerConvert, new();
    }
}
