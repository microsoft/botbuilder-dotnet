// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Schema;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder
{
    /// <summary>
    ///  Middleware to normalize mention Entities from channels that apply &lt;at&gt; markup tags since they don't conform to expected values.
    ///  Bots that interact with Skype and/or teams should use this middleware if mentions are used.
    /// </summary>
    /// <description>
    ///  This will 
    ///  * remove mentions if they mention the recipient (aka the bot) as that text can cause confusion with intent processing.
    ///  * remove extra &lt;at&gt; markup tags on mentions and in the activity.text.
    /// </description>
    public class NormalizeMentionsMiddleware : IMiddleware
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NormalizeMentionsMiddleware"/> class.
        /// </summary>
        public NormalizeMentionsMiddleware()
        {
        }

        /// <summary>
        /// Gets or sets a value indicating whether the any recipient mentions should be removed.
        /// </summary>
        /// <value>If true, @mentions of the recipient will be completely stripped from the .text and .entities.</value>
        public bool RemoveRecipientMention { get; set; } = true;

        /// <summary>
        /// Middleware implementation which corrects Enity.Mention.Text to a value RemoveMentionText can work with.
        /// </summary>
        /// <param name="turnContext">turn context.</param>
        /// <param name="next">next middleware.</param>
        /// <param name="cancellationToken">cancellationToken.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task OnTurnAsync(ITurnContext turnContext, NextDelegate next, CancellationToken cancellationToken = default(CancellationToken))
        {
            NormalizeActivity(turnContext.Activity);
            await next(cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Normalize the activity.
        /// </summary>
        /// <param name="activity">activity.</param>
        private void NormalizeActivity(Activity activity)
        {
            if (activity.Type == ActivityTypes.Message)
            {
                if (this.RemoveRecipientMention)
                {
                    // strip recipient mention tags and text.
                    activity.RemoveRecipientMention();

                    if (activity.Entities != null)
                    {
                        // strip entity.mention records for recipient id.
                        activity.Entities = activity.Entities.Where(entity => entity.Type == "mention" &&
                           ((dynamic)entity.Properties["mentioned"]).id != activity.Recipient.Id).ToList();
                    }
                }

                // remove <at> </at> tags keeping the inner text.
                activity.Text = RemoveAt(activity.Text);

                if (activity.Entities != null)
                {
                    // remove <at> </at> tags from mention records keeping the inner text.
                    foreach (var entity in activity.Entities)
                    {
                        if (entity.Type == "mention")
                        {
                            string entityText = (string)entity.Properties["text"];
                            entity.Properties["text"] = RemoveAt(entityText)?.Trim();
                        }
                    }
                }
            }
        }

        private string RemoveAt(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return text;
            }

            bool foundTag;
            do
            {
                foundTag = false;
                int iAtStart = text.IndexOf("<at", StringComparison.InvariantCultureIgnoreCase);
                if (iAtStart >= 0)
                {
                    int iAtEnd = text.IndexOf(">", iAtStart, StringComparison.InvariantCultureIgnoreCase);
                    if (iAtEnd > 0)
                    {
                        int iAtClose = text.IndexOf("</at>", iAtEnd, StringComparison.InvariantCultureIgnoreCase);
                        if (iAtClose > 0)
                        {
                            // replace </at> 
                            var followingText = text.Substring(iAtClose + 5);

                            // if first char of followingText is not whitespace
                            if (!char.IsWhiteSpace(followingText.FirstOrDefault()))
                            {
                                // insert space because teams does => <at>Tom</at>is cool => Tomis cool
                                followingText = $" {followingText}";
                            }

                            text = text.Substring(0, iAtClose) + followingText;

                            // replace <at ...>
                            text = text.Substring(0, iAtStart) + text.Substring(iAtEnd + 1);

                            // we found one, try again, there may be more.
                            foundTag = true;
                        }
                    }
                }
            }
            while (foundTag);

            return text;
        }
    }
}
