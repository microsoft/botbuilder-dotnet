// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Newtonsoft.Json;

namespace Microsoft.Bot.Schema
{
    /// <summary>
    /// Synchronous request to invoke an operation
    /// </summary>
    public class InvokeActivity : ActivityWithValue
    {
        public InvokeActivity() : base(ActivityTypes.Invoke)
        {

        }

        /// <summary>
        /// Name of the operation to invoke
        /// </summary>
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets reference to another conversation or activity
        /// </summary>
        [JsonProperty(PropertyName = "relatesTo")]
        public ConversationReference RelatesTo { get; set; }
    }
}
