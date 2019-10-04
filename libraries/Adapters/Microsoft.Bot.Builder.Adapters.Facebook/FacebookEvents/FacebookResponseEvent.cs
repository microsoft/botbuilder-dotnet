// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;

namespace Microsoft.Bot.Builder.Adapters.Facebook.FacebookEvents
{
    public class FacebookResponseEvent
    {
        public string Object { get; set; }

        public List<FacebookEntry> Entry { get; set; }
    }
}
