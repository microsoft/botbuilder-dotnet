// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Builder
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
}
