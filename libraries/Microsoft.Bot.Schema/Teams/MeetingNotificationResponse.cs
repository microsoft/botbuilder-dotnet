// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using Newtonsoft.Json;

namespace Microsoft.Bot.Schema.Teams
{
    /// <summary>
    /// Specifies Bot meeting notification response.
    /// Contains list of <see cref="MeetingNotificationRecipientFailureInfo"/>.
    /// </summary>
    public class MeetingNotificationResponse
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MeetingNotificationResponse"/> class.
        /// </summary>
        public MeetingNotificationResponse()
        {
        }

        /// <summary>
        /// Gets or sets the list of <see cref="MeetingNotificationRecipientFailureInfo"/>.
        /// </summary>
        /// <value>The list of <see cref="MeetingNotificationRecipientFailureInfo"/>.</value>
        [JsonProperty(PropertyName = "recipientsFailureInfo")]
#pragma warning disable CA2227 // Collection properties should be read only (we can't change this without breaking binary compat)>
        public IList<MeetingNotificationRecipientFailureInfo> RecipientsFailureInfo { get; set; }
#pragma warning restore CA2227 // Collection properties should be read only
    }
}
