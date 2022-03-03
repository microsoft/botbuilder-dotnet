﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Schema.Teams
{
    using Newtonsoft.Json;
    
    /// <summary>
    /// Describes a Teams Meeting.
    /// </summary>
    public class TeamsMeetingInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TeamsMeetingInfo"/> class.
        /// </summary>
        /// <param name="id">Unique identifier representing a teams meeting.</param>
        public TeamsMeetingInfo(string id = default)
        {
            Id = id;
        }

        /// <summary>
        /// Gets or sets unique identifier representing a Teams Meeting.
        /// </summary>
        /// <value>
        /// Unique identifier representing a Teams Meeting.
        /// </value>
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }
    }
}
