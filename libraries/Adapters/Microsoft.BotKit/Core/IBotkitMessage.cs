// Copyright(c) Microsoft Corporation.All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Schema;

namespace Microsoft.BotKit.Core
{
    public interface IBotkitMessage
    {
        string Type { get; set; }

        string Text { get; set; }

        string Value { get; set; }

        string User { get; set; }

        string Channel { get; set; }

        ConversationReference Reference { get; set; }

        Activity IncomingMessage { get; set; }
    }
}
