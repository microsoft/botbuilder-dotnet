// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Skills
{
    /// <summary>
    /// SkillConnector is the base class that handles communication with a skill.
    /// </summary>
    /// <remarks>
    /// Its responsibility is to forward a incoming request to the skill and handle
    /// the responses based on Skill Protocol.
    /// </remarks>
    public abstract class SkillConnector
    {
        /// <summary>
        /// Forward incoming request to the skill.
        /// </summary>
        /// <param name="turnContext">The <see cref="TurnContext"/> for the activity.</param>
        /// <param name="activity">Activity object to forward.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Response activity of the forwarded activity to the skill.</returns>
        public abstract Task<SkillTurnResult> ProcessActivityAsync(ITurnContext turnContext, Activity activity, CancellationToken cancellationToken);

        /// <summary>
        /// Forward incoming request to the skill.
        /// </summary>
        /// <param name="turnContext">The <see cref="TurnContext"/> for the activity.</param>
        /// <param name="activity">Activity object to forward.</param>
        /// <param name="activitiesHandler">A handler to process incoming activities.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Response activity of the forwarded activity to the skill.</returns>
        public abstract Task<SkillTurnResult> ProcessActivityAsync(ITurnContext turnContext, Activity activity, SendActivitiesHandler activitiesHandler, CancellationToken cancellationToken);
    }
}
