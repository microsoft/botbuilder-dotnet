// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
namespace Microsoft.Bot.Builder.AI.Luis.Tests
{
    public class TelemetryConvertResult : IRecognizerConvert
    {
        private RecognizerResult _result;

        public TelemetryConvertResult()
        {
        }

        /// <summary>
        /// Convert recognizer result.
        /// </summary>
        /// <param name="result">Result to convert.</param>
        public void Convert(dynamic result)
        {
            _result = result as RecognizerResult;
        }
    }
}
