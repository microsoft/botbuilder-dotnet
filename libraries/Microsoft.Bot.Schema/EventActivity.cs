// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Newtonsoft.Json;

namespace Microsoft.Bot.Schema
{
    /// <summary>
    /// Asynchronous external event
    /// </summary>
    public class EventActivity : ActivityWithValue
    {
        public EventActivity() : base(ActivityTypes.Event)
        {
        }

        /// <summary>
        /// Gets or sets the name of the event
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
