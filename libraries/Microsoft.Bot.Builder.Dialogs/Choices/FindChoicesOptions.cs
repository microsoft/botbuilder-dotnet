// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Choices
{
    /// <summary>
    /// Contains options to control how input is matched against a list of choices.
    /// </summary>
    public class FindChoicesOptions : FindValuesOptions
    {
        /// <summary>
        /// Gets or sets a value indicating whether the choices value will NOT be search over.
        /// The default is <c>false</c>. This is optional.
        /// </summary>
        /// <value>
        /// A <c>true</c> if the choices value will NOT be search over; otherwise <c>false</c>.
        /// </value>
        [JsonProperty("noValue")]
        public bool NoValue { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the title of the choices action will NOT be searched over.
        /// The default is <c>false</c>. This is optional.
        /// </summary>
        /// <value>
        /// A <c>true</c> if the title of the choices action will NOT be searched over; otherwise <c>false</c>.
        /// </value>
        [JsonProperty("noAction")]
        public bool NoAction { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the recognizer should check for Numbers using the NumberRecognizer's
        /// NumberModel.
        /// </summary>
        /// <value>
        /// Default is <c>true</c>.  If <c>false</c>, the Number Model will not be used to check the utterance for numbers.
        /// </value>
        [JsonProperty("recognizeNumbers")]
        public bool RecognizeNumbers { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether the recognizer should check for Ordinal Numbers using the NumberRecognizer's
        /// OrdinalModel.
        /// </summary>
        /// <value>
        /// Default is <c>true</c>.  If <c>false</c>, the Ordinal Model will not be used to check the utterance for ordinal numbers.
        /// </value>
        [JsonProperty("recognizeOrdinals")]
        public bool RecognizeOrdinals { get; set; } = true;
    }
}
