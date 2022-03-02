// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using System;
using Newtonsoft.Json;

namespace Microsoft.Bot.Schema.Teams
{
    /// <summary>
    /// Specific details of a Teams meeting.
    /// </summary>
    public class MeetingDetailsBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MeetingDetailsBase"/> class.
        /// </summary>
        /// <param name="id">The meeting's Id, encoded as a BASE64 string.</param>
        /// <param name="joinUrl">The URL used to join the meeting.</param>
        /// <param name="title">The title of the meeting.</param>
        internal MeetingDetailsBase(
            string id,
            Uri joinUrl = null,
            string title = null)
        {
            Id = id;
            JoinUrl = joinUrl;
            Title = title;
        }

        /// <summary>
        /// Gets or sets the meeting's Id, encoded as a BASE64 string.
        /// </summary>
        /// <value>
        /// The meeting's Id, encoded as a BASE64 string.
        /// </value>
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the URL used to join the meeting.
        /// </summary>
        /// <value>
        /// The URL used to join the meeting.
        /// </value>
        [JsonProperty(PropertyName = "joinUrl")]
        public Uri JoinUrl { get; set; }

        /// <summary>
        /// Gets or sets the title of the meeting.
        /// </summary>
        /// <value>
        /// The title of the meeting.
        /// </value>
        [JsonProperty(PropertyName = "title")]
        public string Title { get; set; }
    }
}
