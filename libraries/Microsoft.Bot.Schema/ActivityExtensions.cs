// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Linq;
using System.Text.RegularExpressions;

namespace Microsoft.Bot.Schema
{
    /// <summary>
    /// Helper functions for Message Activities.
    /// </summary>
    public static class ActivityExtensions
    {
        /// <summary>
        /// Is there a mention of Id in the Text Property.
        /// </summary>
        /// <param name="activity">activity.</param>
        /// <param name="id">ChannelAccount.Id.</param>
        /// <returns>true if this id is mentioned in the text.</returns>
        public static bool MentionsId(this IMessageActivity activity, string id)
        {
            return activity.GetMentions().Where(mention => mention.Mentioned.Id == id).Any();
        }

        /// <summary>
        /// Is there a mention of Recipient.Id in the Text Property.
        /// </summary>
        /// <param name="activity">activity.</param>
        /// <returns>true if this id is mentioned in the text.</returns>
        public static bool MentionsRecipient(this IMessageActivity activity)
        {
            return activity.GetMentions().Where(mention => mention.Mentioned.Id == activity.Recipient.Id).Any();
        }

        /// <summary>
        /// Remove recipient mention text from Text property.
        /// </summary>
        /// <param name="activity">.</param>
        /// <returns>new .Text property value.</returns>
        public static string RemoveRecipientMention(this IMessageActivity activity)
        {
            return activity.RemoveMentionText(activity.Recipient.Id);
        }

        /// <summary>
        /// Replace any mention text for given id from Text property. First checks for and replaces the name of the
        /// recipient with the matching id and then checks for the leftover <at></at> tags (this is done to handle
        /// the way Skype sends mention text.
        /// </summary>
        /// <param name="activity">activity.</param>
        /// <param name="id">id to match.</param>
        /// <returns>new .Text property value.</returns>
        public static string RemoveMentionText(this IMessageActivity activity, string id)
        {
            foreach (var mention in activity.GetMentions().Where(mention => mention.Mentioned.Id == id))
            {
                var mentionNameMatch = Regex.Match(mention.Text, @"(?<=<at.*>)(.*?)(?=<\/at>)", RegexOptions.IgnoreCase);
                if (mentionNameMatch.Success)
                {
                    activity.Text = activity.Text.Replace(mentionNameMatch.Value, string.Empty);
                    activity.Text = Regex.Replace(activity.Text, "<at></at>", string.Empty, RegexOptions.IgnoreCase);
                }
            }

            return activity.Text;
        }
    }
}
