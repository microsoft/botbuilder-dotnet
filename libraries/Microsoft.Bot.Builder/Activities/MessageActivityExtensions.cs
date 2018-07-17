// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Microsoft.Bot.Schema
{
    /// <summary>
    /// A set of extension methods for <see cref="MessageActivity"/>.
    /// </summary>
    public static class MessageActivityExtensions
    {
        /// <summary>
        /// Checks whether this message activity has content.
        /// </summary>
        /// <param name="messageActivity">The <see cref="MessageActivity"/> to check for content.</param>
        /// <returns>True, if this message activity has any content to send; otherwise, false.</returns>
        /// <remarks>This method is defined on the <see cref="Activity"/> class, but is only intended
        /// for use on an activity of <see cref="Activity.Type"/> <see cref="ActivityTypes.Message"/>.</remarks>
        public static bool HasContent(this MessageActivity messageActivity)
        {
            if (!string.IsNullOrWhiteSpace(messageActivity.Text))
            {
                return true;
            }

            if (!string.IsNullOrWhiteSpace(messageActivity.Summary))
            {
                return true;
            }

            if (messageActivity.Attachments?.Any() == true)
            {
                return true;
            }

            if (messageActivity.ChannelData != null)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Resolves the mentions from the entities of this (message) activity.
        /// </summary>
        /// <param name="messageActivity">The <see cref="MessageActivity"/> to get mentions for.</param>
        /// <returns>The array of mentions; or an empty array, if none are found.</returns>
        public static IEnumerable<Mention> GetMentions(this MessageActivity messageActivity)
        {
            if (messageActivity.Entities == null)
            {
                return Enumerable.Empty<Mention>();
            }

            return messageActivity.Entities
                .Where(e => e.Type.Equals("mention", StringComparison.OrdinalIgnoreCase))
                .Select(e => e.Properties.ToObject<Mention>());
        }

        /// <summary>
        /// Is there a mention of <paramref name="id"/> in the <see cref="MessageActivity.Text">Text</see> property.
        /// </summary>
        /// <param name="activity">The <see cref="MessageActivity"/> to check for a mention <paramref name="id"/>.</param>
        /// <param name="id">A <see cref="ChannelAccount.Id"/>.</param>
        /// <returns>True if this id is mentioned in the <see cref="MessageActivity.Text"/>.</returns>
        public static bool MentionsId(this MessageActivity activity, string id) =>
            activity.GetMentions()
                .Where(mention => mention.Mentioned.Id == id)
                .Any();

        /// <summary>
        /// Is there a mention of <see cref="Activity.Recipient.Id"/> in the <see cref="MessageActivity.Text">Text</see> property.
        /// </summary>
        /// <param name="messageActivity">The <see cref="MessageActivity"/> to check.</param>
        /// <returns>True if the <see cref="Activity.Recipient"/>'s Id is mentioned in the <see cref="MessageActivity.Text"/>.</returns>
        public static bool MentionsRecipient(this MessageActivity messageActivity) =>
            messageActivity.GetMentions()
                .Where(mention => mention.Mentioned.Id == messageActivity.Recipient.Id)
                .Any();

        /// <summary>
        /// Remove recipient mention text from <see cref="Message"/>Text property.
        /// </summary>
        /// <param name="messageActivity">The <see cref="MessageActivity"/> to remove the <see cref="Activity.Recipient"/> mentions from.</param>
        /// <returns>The updated value of the <see cref="MessageActivity.Text"/> property.</returns>
        public static string RemoveRecipientMention(this MessageActivity messageActivity) =>
            messageActivity.RemoveMentionText(messageActivity.Recipient.Id);

        /// <summary>
        /// Replace any mention text for given id from Text property.
        /// </summary>
        /// <param name="messageActivity">The <see cref="MessageActivity"/> to remove the mention text from.</param>
        /// <param name="id">The id of the mention to locate.</param>
        /// <returns>The updated value of the <see cref="MessageActivity.Text"/> property.</returns>
        public static string RemoveMentionText(this MessageActivity messageActivity, string id)
        {
            foreach (var mention in messageActivity.GetMentions().Where(mention => mention.Mentioned.Id == id))
            {
                messageActivity.Text = Regex.Replace(messageActivity.Text, mention.Text, string.Empty, RegexOptions.IgnoreCase);
            }

            return messageActivity.Text;
        }
    }
}
