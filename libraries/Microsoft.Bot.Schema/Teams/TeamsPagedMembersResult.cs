// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Schema.Teams
{
    public class TeamsPagedMembersResult
    {
        public TeamsPagedMembersResult()
        {
        }

        public TeamsPagedMembersResult(string continuationToken = default(string), IList<ChannelAccount> members = default(IList<ChannelAccount>))
        {
            ContinuationToken = continuationToken;
            var teamsChannelAccounts = members.Select(channelAccount => JObject.FromObject(channelAccount).ToObject<TeamsChannelAccount>());
            Members = teamsChannelAccounts.ToList();
        }

        [JsonProperty(PropertyName = "continuationToken")]
        public string ContinuationToken { get; set; }

        [JsonProperty(PropertyName = "members")]
        public IList<TeamsChannelAccount> Members { get; set; }
    }
}
