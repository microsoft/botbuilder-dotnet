// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder
{
    /// <summary>
    ///  Middleware to patch mention Entities from Skype since they don't conform to expected values.
    /// </summary>
    public class SkypeMiddleware : IMiddleware
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SkypeMiddleware"/> class.
        /// </summary>
        public SkypeMiddleware()
        {
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
            if (turnContext.Activity.ChannelId == "skype" && turnContext.Activity.Type == ActivityTypes.Message)
            {
                foreach (var entity in turnContext.Activity.Entities)
                {
                    if (entity.Type == "mention")
                    {
                        // A Skype mention is of the format:
                        //    <at id=\"28:2bc5b54d-5d48-4ff1-bd25-03dcbb5ce918\">botname</at>
                        // But the Mention.Text doesn't contain those tags and RemoveMentionText can't remove
                        // the bot name from Activity.Text.
                        // This will remove the <at> nodes, leaving just the name.
                        string text = (string)entity.Properties["text"];
                        var mentionNameMatch = Regex.Match(text, @"(?<=<at.*>)(.*?)(?=<\/at>)", RegexOptions.IgnoreCase);
                        if (mentionNameMatch.Success)
                        {
                            entity.Properties["text"] = mentionNameMatch.Value;
                        }
                    }
                }
            }

            await next(cancellationToken).ConfigureAwait(false);
        }
    }
}
