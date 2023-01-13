﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Newtonsoft.Json;

namespace Microsoft.Bot.Schema.Teams
{
    /// <summary>
    /// Information regarding failure to notify a recipient of a <see cref="TeamsMeetingNotification"/>.
    /// </summary>
    public class TeamsMeetingNotificationRecipientFailureInfo
    {
        /// <summary>
        /// Gets or sets the mri for a recipient <see cref="TeamsMeetingNotification"/> failure.
        /// </summary>
        /// <value>The type of this notification container.</value>
        [JsonProperty(PropertyName = "recipientMri")]
        public string RecipientMri { get; set; }

        /// <summary>
        /// Gets or sets the error code for a <see cref="TeamsMeetingNotification"/>.
        /// </summary>
        /// <value>The error code for a <see cref="TeamsMeetingNotification"/>.</value>
        [JsonProperty(PropertyName = "errorcode")]
        public string ErrorCode { get; set; }

        /// <summary>
        /// Gets or sets the failure reason for a <see cref="TeamsMeetingNotification"/> failure.
        /// </summary>
        /// <value>The reason why a participant <see cref="TeamsMeetingNotification"/> failed.</value>
        [JsonProperty(PropertyName = "failureReason")]
        public string FailureReason { get; set; }
    }
}
