// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder
{
    /// <summary>
    /// Support the DirectLine speech and telephony channels to ensure the appropriate SSML tags are set 
    /// on the Activity Speak property.
    /// </summary>
    public class SetSpeakMiddleware : IMiddleware
    {
        private readonly string _voiceName;
        private readonly bool _fallbackToTextForSpeak;

        /// <summary>
        /// Initializes a new instance of the <see cref="SetSpeakMiddleware"/> class.
        /// </summary>
        /// <param name="voiceName">The SSML voice name attribute value.</param>
        /// <param name="fallbackToTextForSpeak">true if an empt Activity.Speak is populated with Activity.Text.</param>
        public SetSpeakMiddleware(string voiceName, bool fallbackToTextForSpeak)
        {
            _voiceName = voiceName;
            _fallbackToTextForSpeak = fallbackToTextForSpeak;
        }

        /// <summary>
        /// Processes an incoming activity.
        /// </summary>
        /// <param name="turnContext">The context object for this turn.</param>
        /// <param name="next">The delegate to call to continue the bot middleware pipeline.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <seealso cref="ITurnContext"/>
        /// <seealso cref="IActivity"/>
        public async Task OnTurnAsync(ITurnContext turnContext, NextDelegate next, CancellationToken cancellationToken = default)
        {
            turnContext.OnSendActivities(async (ctx, activities, nextSend) =>
            {
                foreach (var activity in activities)
                {
                    if (activity.Type == ActivityTypes.Message)
                    {
                        if (_fallbackToTextForSpeak && string.IsNullOrEmpty(activity.Speak))
                        {
                            activity.Speak = activity.Text;
                        }

                        if (!string.IsNullOrEmpty(activity.Speak)
                            && !string.IsNullOrEmpty(_voiceName)
                            && (string.Equals(turnContext.Activity.ChannelId, Channels.DirectlineSpeech, StringComparison.OrdinalIgnoreCase)
                                || string.Equals(turnContext.Activity.ChannelId, Channels.Emulator, StringComparison.OrdinalIgnoreCase)
                                || string.Equals(turnContext.Activity.ChannelId, Channels.Telephony, StringComparison.OrdinalIgnoreCase)))
                        {
                            if (!HasTag("speak", activity.Speak))
                            {
                                if (!HasTag("voice", activity.Speak))
                                {
                                    activity.Speak = $"<voice name='{_voiceName}'>{activity.Speak}</voice>";
                                }

                                activity.Speak = $"<speak version='1.0' xmlns='http://www.w3.org/2001/10/synthesis' xml:lang='{activity.Locale ?? "en-US"}'>{activity.Speak}</speak>";
                            }
                        }
                    }
                }

                return await nextSend().ConfigureAwait(false);
            });

            await next(cancellationToken).ConfigureAwait(false);
        }

        private bool HasTag(string tagName, string speakText)
        {
            try
            {
                var speakSsmlDoc = XDocument.Parse(speakText);

                if (speakSsmlDoc.Root != null && speakSsmlDoc.Root.AncestorsAndSelf().Any(x => x.Name.LocalName.ToLowerInvariant() == tagName))
                {
                    return true;
                }

                return false;
            }
            catch (XmlException)
            {
                return false;
            }
        }
    }
}
