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
        private readonly string _languageAttribute = "lang";
        private readonly string _ssmlVersionAttribute = "version";
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
            if (IsVoiceTag(activity.Speak))
            {
                activity.Speak = CreateMultiVoiceTag(activity).ToString(SaveOptions.DisableFormatting);
            }

            if (!HasTag(_speakTag, activity.Speak))
            {
                if (!HasTag(_voiceTag, activity.Speak))
                {
                    activity.Speak = CreateXmlVoiceTag(activity).ToString(SaveOptions.DisableFormatting);
                }

                activity.Speak = CreateXmlSpeakTag(activity).ToString(SaveOptions.DisableFormatting);
            }
        }

        private void RemoveEmptyXmlnsAttribute(XElement xml)
        {
            foreach (var node in xml.Descendants())
            {
                node.Attributes("xmlns").Remove();
                node.Name = node.Parent.Name.Namespace + node.Name.LocalName;
            }
        }

        private XElement CreateMultiVoiceTag(Activity activity)
        {
            try
            {
                // Wrap voice tag under one virtual root before invoking the XDocument.Parse
                var speakTagTemplate = $"<speak version=\"1.0\" xml:lang=\"{activity.Locale ?? "en - US"}\" xmlns=\"{_nameSpaceUri}\">{activity.Speak}</speak>";
                var speakTag = XElement.Parse(speakTagTemplate);

                return speakTag;
            }
            catch (XmlException)
            {
                throw;
            }
        }

        private XElement CreateXmlSpeakTag(Activity activity)
        {
            try
            {
                XElement xml = new XElement(
                            _nameSpaceUri + _speakTag,
                            new XAttribute(_ssmlVersionAttribute, _ssmlVersionSupported),
                            new XAttribute(XNamespace.Xml + _languageAttribute, activity.Locale ?? "en - US"),
                            XElement.Parse(activity.Speak));

                // Attribute xmlns="" added due to child nodes containing empty namespace value.
                RemoveEmptyXmlnsAttribute(xml);

                return xml;
            }
            catch (XmlException)
            {
                throw;
            }
        }

        private XElement CreateXmlVoiceTag(Activity activity)
        {
            XElement voiceTag = new XElement(
            _voiceTag,
            new XAttribute(_nameAttribute, _voiceName),
            activity.Speak);

            return voiceTag;
        }

        private bool HasTag(string tagName, string speakText)
        {
            try
            {
                foreach (char c in speakText)
                {
                    if (c == '<')
                    {
                        break;
                    }
                    else if (char.IsWhiteSpace(c))
                    {
                        continue;
                    }
                    else
                    {
                        return false;
                    }
                }

                var speakSsmlDoc = XDocument.Parse(speakText);

                if (speakSsmlDoc.Root != null && speakSsmlDoc.Root.AncestorsAndSelf().Any(x => x.Name.LocalName.ToLowerInvariant() == tagName))
                {
                    return true;
                }

                return false;
            }
            catch (XmlException)
            {
                throw;
            }
        }

        private bool IsVoiceTag(string speakText)
        {
            return speakText.Contains("<voice");
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
    }
}
