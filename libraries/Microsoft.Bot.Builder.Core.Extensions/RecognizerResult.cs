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
    public class RecognizerResult
    {
        /// <summary>
        /// Original text to recognizer.
        /// </summary>
        public string Text { set; get; }

        /// <summary>
        /// Text modified by recognizer for example by spell correction.
        /// </summary>
        public string AlteredText { set; get; }

        /// <summary>
        /// Object with the intent as key and the confidence as value.
        /// </summary>
        public JObject Intents { get; set; }

        /// <summary>
        /// Object with each top-level recognized entity as a key.
        /// </summary>
        public JObject Entities { get; set; }

        /// <summary>
        /// Any extra properties to include in the results.
        /// </summary>
        [JsonExtensionData(ReadData = true, WriteData = true)]
        public IDictionary<string, object> Properties { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// Default constructor.
        /// </summary>
        public RecognizerResult()
        { }

        /// <summary>
        /// Shallow copy constructor.
        /// </summary>
        /// <param name="other">Result to copy results from.</param>
        public RecognizerResult(RecognizerResult other)
        {
            Text = other.Text;
            AlteredText = other.AlteredText;
            Intents = other.Intents;
            Entities = other.Entities;
            Properties = other.Properties;
        }
    }
}
