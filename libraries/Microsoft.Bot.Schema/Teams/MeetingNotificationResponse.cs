// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using Newtonsoft.Json;

namespace Microsoft.Bot.Schema.Teams
{
    /// <summary>
    /// Specifies Bot meeting notification response.
    /// Contains list of <see cref="NotificationRecipientFailureInfo"/>.
    /// </summary>
    public partial class MeetingNotificationResponse
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MeetingNotificationResponse"/> class.
        /// </summary>
        public MeetingNotificationResponse()
        {
            CustomInit();
        }

        /// <summary>
        /// Gets or sets the list of <see cref="MeetingNotificationResponse"/>.
        /// </summary>
        /// <value>The list of recipients who did not receive a <see cref="BotMeetingNotificationBase"/> including error information.</value>
        [JsonProperty(PropertyName = "recipientsFailureInfo")]
#pragma warning disable CA2227 // Collection properties should be read only (we can't change this without breaking binary compat)>
        public IList<NotificationRecipientFailureInfo> RecipientsFailureInfo { get; set; }
#pragma warning restore CA2227 // Collection properties should be read only

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults.
        /// </summary>
        partial void CustomInit();
    }
}
