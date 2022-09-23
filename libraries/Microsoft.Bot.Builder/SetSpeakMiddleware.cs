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
        private readonly XNamespace _nameSpaceUri = @"http://www.w3.org/2001/10/synthesis";
        private readonly string _speakTag = "speak";
        private readonly string _voiceTag = "voice";
        private readonly string _nameAttribute = "name";
        private readonly string _ssmlVersionSupported = "1.0";

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

                        if (IsSpeakActivitySet(activity) && IsChannelActivitySet(turnContext))
                        {
                            SetXmlSpeakTag(activity);
                        }
                    }
                }

                return await nextSend().ConfigureAwait(false);
            });

            await next(cancellationToken).ConfigureAwait(false);
        }

        private void SetXmlSpeakTag(Activity activity)
        {
            var tags = TagsPresent(activity.Speak);
            if (!tags.SpeakTag)
            {
                if (!tags.VoiceTag)
                {
                    activity.Speak = CreateXmlVoiceTag(activity);
                }

                activity.Speak = CreateXmlSpeakTag(activity);
            }
        }

        private string CreateXmlSpeakTag(Activity activity)
        {
            try
            {
                var speakTagTemplate = $"<{_speakTag} version=\"{_ssmlVersionSupported}\" xml:lang=\"{activity.Locale ?? "en - US"}\" xmlns=\"{_nameSpaceUri}\">{activity.Speak}</{_speakTag}>";
                var xmlSpeakTag = XElement.Parse(speakTagTemplate);

                return xmlSpeakTag.ToString(SaveOptions.DisableFormatting);
            }
            catch (XmlException)
            {
            }

            return activity.Speak;
        }

        private string CreateXmlVoiceTag(Activity activity)
        {
            try
            {
                var voiceTag = new XElement(
                    _voiceTag,
                    new XAttribute(_nameAttribute, _voiceName),
                    activity.Speak);

                return voiceTag.ToString(SaveOptions.DisableFormatting);
            }
            catch (XmlException)
            {
            }

            return activity.Speak;
        }

        private Tags TagsPresent(string speakText)
        {
            try
            {
                if (IsVoiceTag(speakText))
                {
                    return new Tags() { VoiceTag = true };
                }

                var speakSsmlDoc = XDocument.Parse(speakText);
                if (speakSsmlDoc.Root != null)
                {
                    var hasSpeakTag = speakSsmlDoc.Root.AncestorsAndSelf().Any(x => x.Name.LocalName.ToLowerInvariant() == _speakTag);
                    var hasVoiceTag = speakSsmlDoc.Root.AncestorsAndSelf().Any(x => x.Name.LocalName.ToLowerInvariant() == _voiceTag);
                    return new Tags { SpeakTag = hasSpeakTag, VoiceTag = hasVoiceTag };
                }
            }
            catch (XmlException)
            {
                // tags not present
            }

            return new Tags();
        }

        private bool IsVoiceTag(string speakText)
        {
            return speakText.StartsWith("<voice", StringComparison.OrdinalIgnoreCase);
        }

        private bool IsSpeakActivitySet(Activity activity)
        {
            return !string.IsNullOrEmpty(activity.Speak)
                            && !string.IsNullOrEmpty(_voiceName);
        }

        private bool IsChannelActivitySet(ITurnContext turnContext)
        {
            return string.Equals(turnContext.Activity.ChannelId, Channels.DirectlineSpeech, StringComparison.OrdinalIgnoreCase)
                                || string.Equals(turnContext.Activity.ChannelId, Channels.Emulator, StringComparison.OrdinalIgnoreCase)
                                || string.Equals(turnContext.Activity.ChannelId, Channels.Telephony, StringComparison.OrdinalIgnoreCase);
        }

        private class Tags
        {
            public bool SpeakTag { get; set; }

            public bool VoiceTag { get; set; }
        }
    }
}
