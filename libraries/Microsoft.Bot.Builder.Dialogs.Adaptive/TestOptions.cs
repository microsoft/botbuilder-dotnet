using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive
{
    /// <summary>
    /// Test options.
    /// </summary>
    public class TestOptions
    {
        /// <summary>
        /// Kind.
        /// </summary>
        public const string Kind = "conversation.testOptions";

        /// <summary>
        /// Gets or sets random client.
        /// </summary>
        /// <value>
        /// Random seed.
        /// </value>
        [JsonProperty(PropertyName = "randomSeed")]
        public int RandomSeed { get; set; }
    }
}
