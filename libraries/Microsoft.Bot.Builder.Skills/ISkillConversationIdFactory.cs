// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;

namespace Microsoft.Bot.Builder.Skills
{
    public interface ISkillConversationIdFactory
    {
#pragma warning disable CA1054 // Uri parameters should not be strings (justification: using a string to match the type of the Activity property)
        string CreateSkillConversationId(string conversationId, string serviceUrl);
#pragma warning restore CA1054 // Uri parameters should not be strings

        (string, string) GetConversationInfo(string conversationId);
    }
}
