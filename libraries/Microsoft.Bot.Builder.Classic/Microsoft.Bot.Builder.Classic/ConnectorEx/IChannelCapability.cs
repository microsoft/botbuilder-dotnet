// 
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license.
// 
// Microsoft Bot Framework: http://botframework.com
// 
// Bot Builder SDK GitHub:
// https://github.com/Microsoft/BotBuilder
// 
// Copyright (c) Microsoft Corporation
// All rights reserved.
// 
// MIT License:
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED ""AS IS"", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using Microsoft.Bot.Builder.Classic.Internals.Fibers;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Classic.Dialogs
{
    public sealed class ChannelIds
    {
        public const string Facebook = "facebook";
        public const string Skype = "skype";
        public const string Msteams = "msteams";
        public const string Telegram = "telegram";
        public const string Kik = "kik";
        public const string Email = "email";
        public const string Slack = "slack";
        public const string Groupme = "groupme";
        public const string Sms = "sms";
        public const string Emulator = "emulator";
        public const string Directline = "directline";
        public const string Webchat = "webchat";
        public const string Console = "console";
        public const string Cortana = "cortana";
    }


    /// <summary>
    /// Capability for a specific channel
    /// </summary>
    public interface IChannelCapability
    {
        /// <summary>
        /// Indicates if channel supports keyboard.
        /// </summary>
        /// <param name="buttonCount"> number of buttons.</param>
        /// <returns>True if the channel support number of buttons; false otherwise.</returns>
        bool SupportsKeyboards(int buttonCount);

        /// <summary>
        /// Indicates if channel is TTS enabled.
        /// </summary>
        /// <returns>True if channel support TTS and the bot can set <see cref="Schema.Activity.Speak"/>; false otherwise.</returns>
        bool SupportsSpeak();

        /// <summary>
        /// Indicates if channel relies on <see cref="Schema.Activity.InputHint"/>.
        /// </summary>
        /// <returns>True if channel expect bot setting <see cref="Schema.Activity.InputHint"/>; false otherwise </returns>
        bool NeedsInputHint();
    }

    public sealed class ChannelCapability : IChannelCapability
    {
        private readonly IAddress address;

        public ChannelCapability(IAddress address)
        {
            SetField.NotNull(out this.address, nameof(address), address);
        }

        public bool NeedsInputHint()
        {
            return this.address.ChannelId == ChannelIds.Cortana;
        }

        public bool SupportsKeyboards(int buttonCount)
        {
            switch (this.address.ChannelId)
            {
                case ChannelIds.Facebook:
                    return buttonCount <= 10;
                case ChannelIds.Kik:
                    return buttonCount <= 20;
                case ChannelIds.Slack:
                case ChannelIds.Telegram:
                case ChannelIds.Cortana:
                    return buttonCount <= 100;
                default:
                    return false;
            }
        }

        public bool SupportsSpeak()
        {
            return this.address.ChannelId == ChannelIds.Cortana || this.address.ChannelId == ChannelIds.Webchat;
        }
    }

    public static class ChannelCapabilityEx
    {
        public static bool ShouldSetInputHint(this IChannelCapability channelCapability, IMessageActivity activity)
        {
            return channelCapability.NeedsInputHint()
                && activity.Type == ActivityTypes.Message
                && string.IsNullOrEmpty(activity.InputHint);
        }
    }
}
