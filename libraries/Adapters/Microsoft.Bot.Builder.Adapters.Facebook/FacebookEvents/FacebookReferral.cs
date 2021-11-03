// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Adapters.Facebook.FacebookEvents
{
    /// <summary>
    /// Represents the referral parameter in the messaging_referrals event.
    /// </summary>
    public class FacebookReferral
    {
        /// <summary>
        /// Gets or sets the ref parameter in the referral event.
        /// </summary>
        /// <value>The arbitrary data that was originally passed in the ref param added to the m.me link.</value>
        [JsonProperty(PropertyName = "ref")]
        public string Ref { get; set; }

        /// <summary>
        /// Gets or sets the source parameter in the referral event.
        /// </summary>
        /// <value>The source of this referral. For m.me links, the value of source is “SHORTLINK”. For referrals from Messenger Conversation Ads, the value of source is "ADS".</value>
        [JsonProperty(PropertyName = "source")]
        public string Source { get; set; }

        /// <summary>
        /// Gets or sets the type parameter in the referral event.
        /// </summary>
        /// <value>The identifier for the referral. For referrals coming from m.me links, it will always be "OPEN_THREAD".</value>
        public string Type { get; set; }
    }
}
