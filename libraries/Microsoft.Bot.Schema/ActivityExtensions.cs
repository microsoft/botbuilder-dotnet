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
        /// Use with caution because this function is altering the text on the Activity.
        /// </summary>
        /// <param name="activity">.</param>
        /// <returns>new .Text property value.</returns>
        public static string RemoveRecipientMention(this IMessageActivity activity)
        {
            return activity.RemoveMentionText(activity.Recipient.Id);
        }

        /// <summary>
        /// Remove any mention text for given id from the Activity.Text property.  For example, given the message
        /// @echoBot Hi Bot, this will remove "@echoBot", leaving "Hi Bot".
        /// </summary>
        /// <description>
        /// Typically this would be used to remove the mention text for the target recipient (the bot usually), though
        /// it could be called for each member.  For example:
        ///    turnContext.Activity.RemoveMentionText(turnContext.Activity.Recipient.Id);
        /// The format of a mention Activity.Entity is dependent on the Channel.  But in all cases we
        /// expect the Mention.Text to contain the exact text for the user as it appears in
        /// Activity.Text.
        /// For example, Teams uses &lt;at&gt;username&lt;/at&gt;, whereas slack use @username. It
        /// is expected that text is in Activity.Text and this method will remove that value from
        /// Activity.Text.
        /// </description>
        /// <param name="activity">activity.  The Text property is modified by this method.</param>
        /// <param name="id">id to match.</param>
        /// <returns>new Activity.Text property value.</returns>
        public static string RemoveMentionText(this IMessageActivity activity, string id)
        {
            foreach (var mention in activity.GetMentions().Where(mention => mention.Mentioned.Id == id))
            {
                if (mention.Text == null)
                {
                    activity.Text = Regex.Replace(activity.Text, "<at>" + mention.Mentioned.Name + "</at>", string.Empty, RegexOptions.IgnoreCase).Trim();
                }
                else
                {
                   activity.Text = Regex.Replace(activity.Text, mention.Text, string.Empty, RegexOptions.IgnoreCase).Trim();
                }
            }

            return activity.Text;
        }
    }
}
