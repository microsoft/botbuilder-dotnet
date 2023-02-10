// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Schema.Teams
{
    using Newtonsoft.Json;

    /// <summary>
    /// Specifies Bot meeting notification including meeting notification value.
    /// </summary>
    /// <typeparam name="T">The first generic type parameter.</typeparam>.
    public class MeetingNotification<T> : MeetingNotificationBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MeetingNotification{T}"/> class.
        /// </summary>
        protected MeetingNotification()
        {
        }

        /// <summary>
        /// Gets or sets Teams Bot meeting notification value.
        /// </summary>
        /// <value>
        /// Teams Bot meeting notification value.
        /// </value>
        [JsonProperty(PropertyName = "value")]
        public T Value { get; set; }
    }
}
