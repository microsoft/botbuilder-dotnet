// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Schema.Teams;

namespace Microsoft.Bot.Builder.Teams
{
    public interface ITeamsInfo
    {
        Task<TeamDetails> GetTeamDetailsAsync(ITurnContext turnContext, CancellationToken cancellationToken = default);

        Task<IList<ChannelInfo>> GetChannelsAsync(ITurnContext turnContext, CancellationToken cancellationToken = default);

        Task<IEnumerable<TeamsChannelAccount>> GetMembersAsync(ITurnContext turnContext, CancellationToken cancellationToken = default);
    }
}
