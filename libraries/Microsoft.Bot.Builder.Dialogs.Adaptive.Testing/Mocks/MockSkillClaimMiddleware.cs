// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Skills;
using Microsoft.Bot.Connector.Authentication;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Testing.Mocks
{
    /// <summary>
    /// Middleware for mocking skill claim.
    /// </summary>
    public class MockSkillClaimMiddleware : IMiddleware
    {
        private readonly ClaimsIdentity claimsIdentity;
        private readonly SkillConversationReference skillConversationReference;

        /// <summary>
        /// Initializes a new instance of the <see cref="MockSkillClaimMiddleware"/> class.
        /// </summary>
        /// <param name="mockCase">The mock case.</param>
        /// <param name="parentBotId">Parent bot Id. If null, new GUID.</param>
        /// <param name="skillBotId">Skill bot Id. If null, new GUID.</param>
        public MockSkillClaimMiddleware(MockCase mockCase, string parentBotId = null, string skillBotId = null)
        {
            if (string.IsNullOrEmpty(parentBotId))
            {
                parentBotId = Guid.NewGuid().ToString();
            }

            if (string.IsNullOrEmpty(skillBotId))
            {
                skillBotId = Guid.NewGuid().ToString();
            }

            claimsIdentity = new ClaimsIdentity();
            claimsIdentity.AddClaim(new Claim(AuthenticationConstants.VersionClaim, "2.0"));
            claimsIdentity.AddClaim(new Claim(AuthenticationConstants.AudienceClaim, skillBotId));
            claimsIdentity.AddClaim(new Claim(AuthenticationConstants.AuthorizedParty, parentBotId));

            if (mockCase == MockCase.RootBotConsumingSkill)
            {
                skillConversationReference = new SkillConversationReference { OAuthScope = AuthenticationConstants.ToChannelFromBotOAuthScope };
            }
            else if (mockCase == MockCase.MiddleSkill)
            {
                skillConversationReference = new SkillConversationReference { OAuthScope = parentBotId };
            }
            else
            {
                skillConversationReference = null;
            }
        }

        /// <summary>
        /// Enum to handle different mock cases.
        /// </summary>
        public enum MockCase
        {
            /// <summary>
            /// RunAsync is executing on a root bot handling replies from a skill.
            /// </summary>
            RootBotConsumingSkill,

            /// <summary>
            /// RunAsync is executing in a skill that is called from a root and calling another skill.
            /// </summary>
            MiddleSkill,

            /// <summary>
            /// RunAsync is executing in a skill that is called from a parent (a root or another skill) but doesn't call another skill.
            /// </summary>
            LeafSkill
        }

        /// <inheritdoc/>
        public async Task OnTurnAsync(ITurnContext turnContext, NextDelegate next, CancellationToken cancellationToken = default)
        {
            turnContext.TurnState.Add(BotAdapter.BotIdentityKey, claimsIdentity);
            if (skillConversationReference != null)
            {
                turnContext.TurnState.Add(SkillHandler.SkillConversationReferenceKey, skillConversationReference);
            }

            await next(cancellationToken).ConfigureAwait(false);
        }
    }
}
