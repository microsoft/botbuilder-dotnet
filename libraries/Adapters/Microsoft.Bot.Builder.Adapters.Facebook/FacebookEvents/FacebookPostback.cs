// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Builder.Adapters.Facebook.FacebookEvents
{
    public class FacebookPostBack
    {
        public string Title { get; set; }

        public string Payload { get; set; }

        public string Referral { get; set; }
    }
}
