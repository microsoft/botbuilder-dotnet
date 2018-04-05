// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.


using System.Threading;
using System.Threading.Tasks;
using Microsoft.Cognitive.LUIS.Models;

namespace Microsoft.Bot.Builder.LUIS
{
    /// <inheritdoc />
    /// <summary>
    /// A Luis specific interface that extends the generic Recognizer interface
    /// </summary>
    internal interface ILuisRecognizer : IRecognizer
    {
        Task<(RecognizerResult recognizerResult, LuisResult luisResult)> CallAndRecognize(string utterance, CancellationToken ct);
    }
}
