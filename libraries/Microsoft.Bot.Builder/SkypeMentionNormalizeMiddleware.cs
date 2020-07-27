// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder
{
    /// <summary>
    ///  Middleware to patch mention Entities from Skype since they don't conform to expected values.
    ///  Bots that interact with Skype should use this middleware if mentions are used.
    /// </summary>
    /// <description>
    ///  A Skype mention "text" field is of the format:
    ///    &lt;at id=\"28:2bc5b54d-5d48-4ff1-bd25-03dcbb5ce918\">botname&lt;/at&gt;
    ///  But Activity.Text doesn't contain those tags and RemoveMentionText can't remove
    ///  the entity from Activity.Text.
    ///  This will remove the &lt;at&gt; nodes, leaving just the name.
    /// </description>
    public class SkypeMentionNormalizeMiddleware : IMiddleware
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SkypeMentionNormalizeMiddleware"/> class.
        /// </summary>
        public SkypeMentionNormalizeMiddleware()
        {
        }

        /// <summary>
        /// Performs the normalization of Skype mention Entities.
        /// </summary>
        /// <param name="activity">The activity containing the mentions to normalize.</param>
        public static void NormalizeSkypMentionText(Activity activity)
        {
            if (activity.ChannelId == Channels.Skype && activity.Type == ActivityTypes.Message)
            {
                foreach (var entity in activity.Entities)
                {
                    if (entity.Type == "mention")
                    {
                        string text = (string)entity.Properties["text"];
                        var mentionNameMatch = Regex.Match(text, @"(?<=<at.*>)(.*?)(?=<\/at>)", RegexOptions.IgnoreCase);
                        if (mentionNameMatch.Success)
                        {
                            entity.Properties["text"] = mentionNameMatch.Value;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Middleware implementation which corrects Enity.Mention.Text to a value RemoveMentionText can work with.
        /// </summary>
        /// <param name="turnContext">turn context.</param>
        /// <param name="next">next middleware.</param>
        /// <param name="cancellationToken">cancellationToken.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task OnTurnAsync(ITurnContext turnContext, NextDelegate next, CancellationToken cancellationToken = default(CancellationToken))
        {
            NormalizeSkypMentionText(turnContext.Activity);
            await next(cancellationToken).ConfigureAwait(false);
        }
    }
}
