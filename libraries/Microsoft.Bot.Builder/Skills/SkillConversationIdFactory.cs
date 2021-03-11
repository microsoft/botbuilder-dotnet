// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Skills
{
    /// <summary>
    /// A <see cref="SkillConversationIdFactory"/> that uses <see cref="IStorage"/> to store
    /// and retrieve <see cref="SkillConversationReference"/> instances.
    /// </summary>
    public class SkillConversationIdFactory : SkillConversationIdFactoryBase
    {
        private readonly IStorage _storage;

        /// <summary>
        /// Initializes a new instance of the <see cref="SkillConversationIdFactory"/> class.
        /// </summary>
        /// <param name="storage">
        /// <see cref="IStorage"/> instance to write and read <see cref="SkillConversationReference"/>s from and to.
        /// </param>
        public SkillConversationIdFactory(IStorage storage)
        {
            _storage = storage ?? throw new ArgumentNullException(nameof(storage));
        }

        /// <summary>
        /// Creates a new <see cref="SkillConversationReference"/>.
        /// </summary>
        /// <param name="options">Creation options to use when creating the <see cref="SkillConversationReference"/>.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>ID of the created <see cref="SkillConversationReference"/>.</returns>
        public override async Task<string> CreateSkillConversationIdAsync(
            SkillConversationIdFactoryOptions options,
            CancellationToken cancellationToken)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            // Create the storage key based on the SkillConversationIdFactoryOptions.
            var conversationReference = options.Activity.GetConversationReference();

            string skillConversationId = string.Join(
                "-",
                options.FromBotId,
                options.BotFrameworkSkill.AppId,
                conversationReference.Conversation.Id,
                conversationReference.ChannelId,
                "skillconvo");

            // Create the SkillConversationReference instance.
            var skillConversationReference = new SkillConversationReference
            {
                ConversationReference = conversationReference,
                OAuthScope = options.FromBotOAuthScope
            };

            // Store the SkillConversationReference using the skillConversationId as a key.
            var skillConversationInfo = new Dictionary<string, object>
            {
                {
                    skillConversationId, JObject.FromObject(skillConversationReference)
                }
            };

            await _storage.WriteAsync(skillConversationInfo, cancellationToken).ConfigureAwait(false);

            // Return the generated skillConversationId (that will be also used as the conversation ID to call the skill).
            return skillConversationId;
        }

        /// <summary>
        /// Retrieve a <see cref="SkillConversationReference"/> with the specified ID.
        /// </summary>
        /// <param name="skillConversationId">The ID of the <see cref="SkillConversationReference"/> to retrieve.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns><see cref="SkillConversationReference"/> for the specified ID; null if not found.</returns>
        public override async Task<SkillConversationReference> GetSkillConversationReferenceAsync(
            string skillConversationId,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(skillConversationId))
            {
                throw new ArgumentNullException(nameof(skillConversationId));
            }

            // Get the SkillConversationReference from storage for the given skillConversationId.
            var skillConversationInfo = await _storage
                .ReadAsync(new[] { skillConversationId }, cancellationToken)
                .ConfigureAwait(false);

            if (skillConversationInfo.Any())
            {
                var conversationInfo = ((JObject)skillConversationInfo[skillConversationId]).ToObject<SkillConversationReference>();
                return conversationInfo;
            }

            return null;
        }

        /// <summary>
        /// Deletes the <see cref="SkillConversationReference"/> with the specified ID.
        /// </summary>
        /// <param name="skillConversationId">The ID of the <see cref="SkillConversationReference"/> to be deleted.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Task to complete the deletion operation asynchronously.</returns>
        public override async Task DeleteConversationReferenceAsync(
            string skillConversationId,
            CancellationToken cancellationToken)
        {
            // Delete the SkillConversationReference from storage.
            await _storage.DeleteAsync(new[] { skillConversationId }, cancellationToken).ConfigureAwait(false);
        }
    }
}
