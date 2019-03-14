// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Builder
{
    /// <summary>
    /// The Telemetry Logger Event names.
    /// </summary>
    public static class TelemetryLoggerConstants
    {
        // The name of the event when when new message is received from the user.
        public static readonly string BotMsgReceiveEvent = "BotMessageReceived";

        // The name of the event when logged when a message is sent from the bot to the user.
        public static readonly string BotMsgSendEvent = "BotMessageSend";

        // The name of the event when a message is updated by the bot.
        public static readonly string BotMsgUpdateEvent = "BotMessageUpdate";

        // The name of the event when a message is deleted by the bot.
        public static readonly string BotMsgDeleteEvent = "BotMessageDelete";
    }
}
