﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;

namespace Microsoft.Bot.Connector
{
    /// <summary>
    /// Ids of channels supported by the Bot Builder.
    /// </summary>
#pragma warning disable CA1052 // Static holder types should be Static or NotInheritable (we can't change this without breaking binary compat)
#pragma warning disable CA1724 // Type names should not match namespaces (we can't change this without breaking binary compat)
    public class Channels
#pragma warning restore CA1724 // Type names should not match namespaces
#pragma warning restore CA1052 // Static holder types should be Static or NotInheritable
    {
        /// <summary>
        /// Alexa channel.
        /// </summary>
        public const string Alexa = "alexa";

        /// <summary>
        /// Console channel.
        /// </summary>
        public const string Console = "console";

        /// <summary>
        /// Cortana channel.
        /// </summary>
        [Obsolete("This channel is deprecated.")]
        public const string Cortana = "cortana";

        /// <summary>
        /// Direct Line channel.
        /// </summary>
        public const string Directline = "directline";

        /// <summary>
        /// Direct Line Speech channel.
        /// </summary>
        public const string DirectlineSpeech = "directlinespeech";

        /// <summary>
        /// Email channel.
        /// </summary>
        public const string Email = "email";

        /// <summary>
        /// Emulator channel.
        /// </summary>
        public const string Emulator = "emulator";

        /// <summary>
        /// EnterpriseChannel channel.
        /// </summary>
        [Obsolete("This channel is deprecated.")]
        public const string EnterpriseChannel = "enterprisechannel";

        /// <summary>
        /// Facebook channel.
        /// </summary>
        public const string Facebook = "facebook";

        /// <summary>
        /// Group Me channel.
        /// </summary>
        public const string Groupme = "groupme";

        /// <summary>
        /// Kaizala channel.
        /// </summary>
        [Obsolete("This channel is deprecated.")]
        public const string Kaizala = "kaizala";

        /// <summary>
        /// Kik channel.
        /// </summary>
        [Obsolete("This channel is deprecated.")]
        public const string Kik = "kik";

        /// <summary>
        /// Line channel.
        /// </summary>
        public const string Line = "line";

        /// <summary>
        /// MS Teams channel.
        /// </summary>
        public const string Msteams = "msteams";

        /// <summary>
        /// Skype channel.
        /// </summary>
        public const string Skype = "skype";

        /// <summary>
        /// Skype for Business channel.
        /// </summary>
        [Obsolete("This channel is deprecated.")]
        public const string Skypeforbusiness = "skypeforbusiness";

        /// <summary>
        /// Slack channel.
        /// </summary>
        public const string Slack = "slack";

        /// <summary>
        /// SMS (Twilio) channel.
        /// </summary>
        public const string Sms = "sms";

        /// <summary>
        /// Telegram channel.
        /// </summary>
        public const string Telegram = "telegram";

        /// <summary>
        /// WebChat channel.
        /// </summary>
        public const string Webchat = "webchat";

        /// <summary>
        /// Test channel.
        /// </summary>
        public const string Test = "test";

        /// <summary>
        /// Twilio channel.
        /// </summary>
        [Obsolete("This channel is deprecated. Use Sms instead.")]
        public const string Twilio = "twilio-sms";

        /// <summary>
        /// Telephony channel.
        /// </summary>
        public const string Telephony = "telephony";

        /// <summary>
        /// Omni channel.
        /// </summary>
        public const string Omni = "omnichannel";

        /// <summary>
        /// Outlook channel.
        /// </summary>
        public const string Outlook = "outlook";

        /// <summary>
        /// M365 channel.
        /// </summary>
        public const string M365 = "m365extensions";
    }
}
