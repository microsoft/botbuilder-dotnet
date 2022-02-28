// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Newtonsoft.Json;

namespace Microsoft.Bot.Connector.Schema.Teams
{
    /// <summary>
    /// Describes a Teams Meeting.
    /// </summary>
    public partial class TeamsMeetingInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TeamsMeetingInfo"/> class.
        /// </summary>
        public TeamsMeetingInfo()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TeamsMeetingInfo"/> class.
        /// </summary>
        /// <param name="id">Unique identifier representing a teams meeting.</param>
        public TeamsMeetingInfo(string id = default)
        {
            Id = id;
            CustomInit();
        }

        /// <summary>
        /// Gets or sets unique identifier representing a Teams Meeting.
        /// </summary>
        /// <value>
        /// Unique identifier representing a Teams Meeting.
        /// </value>
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults.
        /// </summary>
        partial void CustomInit();
    }
}
