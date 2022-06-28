// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Microsoft.Bot.Schema.Teams
{
    /// <summary>
    /// Container for <see cref="MeetingNotificationRecipientFailureInfo"/>, which is the result of a
    /// failure to notify recipients of a <see cref="MeetingNotification"/>.
    /// </summary>
    public partial class MeetingNotificationRecipientFailureInfos
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MeetingNotificationRecipientFailureInfos"/> class.
        /// </summary>
        public MeetingNotificationRecipientFailureInfos()
        {
            CustomInit();
        }

        /// <summary>
        /// Gets or sets the list of <see cref="MeetingNotificationRecipientFailureInfo"/>.
        /// </summary>
        /// <value>The list of recipients who did not receive a <see cref="MeetingNotification"/> including error information.</value>
        [JsonProperty(PropertyName = "recipientsFailureInfo")]
#pragma warning disable CA2227 // Collection properties should be read only (we can't change this without breaking binary compat)>
        public IList<MeetingNotificationRecipientFailureInfo> RecipientsFailureInfo { get; set; }
#pragma warning restore CA2227 // Collection properties should be read only

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults.
        /// </summary>
        partial void CustomInit();
    }
}
