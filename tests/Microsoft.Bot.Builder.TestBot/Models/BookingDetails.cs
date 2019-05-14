// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Linq;
using Microsoft.Bot.Builder;

namespace Microsoft.BotBuilderSamples
{
    public class BookingDetails : IRecognizerConvert
    {
        public string Destination { get; set; }

        public string Origin { get; set; }

        public string TravelDate { get; set; }

        public void Convert(dynamic result)
        {
            var recognizerResult = (RecognizerResult)result;

            // We need to get the result from the LUIS JSON which at every level returns an array.
            Destination = recognizerResult.Entities["To"]?.FirstOrDefault()?["Airport"]?.FirstOrDefault()?.FirstOrDefault()?.ToString();
            Origin = recognizerResult.Entities["From"]?.FirstOrDefault()?["Airport"]?.FirstOrDefault()?.FirstOrDefault()?.ToString();

            // This value will be a TIMEX. And we are only interested in a Date so grab the first result and drop the Time part.
            // TIMEX is a format that represents DateTime expressions that include some ambiguity. e.g. missing a Year.
            TravelDate = recognizerResult.Entities["datetime"]?.FirstOrDefault()?["timex"]?.FirstOrDefault()?.ToString().Split('T')[0];
        }
    }
}
