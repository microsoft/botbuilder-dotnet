// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace Microsoft.Bot.Builder.Core.Extensions
{
    /// <summary>
    /// Recognizer return value.
    /// </summary>
    public class RecognizerResult: IRecognizerConvert
    {
        /// <summary>
        /// Original text to recognizer.
        /// </summary>
        [JsonProperty("text")]
        public string Text { set; get; }

        /// <summary>
        /// Text modified by recognizer for example by spell correction.
        /// </summary>
        [JsonProperty("alteredText")]
        public string AlteredText { set; get; }

        /// <summary>
        /// Object with the intent as key and the confidence as value.
        /// </summary>
        [JsonProperty("intents")]
        public JObject Intents { get; set; }

        /// <summary>
        /// Object with each top-level recognized entity as a key.
        /// </summary>
        [JsonProperty("entities")]
        public JObject Entities { get; set; }

        /// <summary>
        /// Any extra properties to include in the results.
        /// </summary>
        [JsonExtensionData(ReadData = true, WriteData = true)]
        public IDictionary<string, object> Properties { get; set; } = new Dictionary<string, object>();

        /// <inheritdoc />
        public void Convert(dynamic result)
        {
            Text = result.Text;
            AlteredText = result.AlteredText;
            Intents = result.Intents;
            Entities = result.Entities;
            Properties = result.Properties;
        }
    }
}
