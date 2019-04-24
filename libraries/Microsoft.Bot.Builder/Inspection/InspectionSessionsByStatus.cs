// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder
{
    internal class InspectionSessionsByStatus
    {
        public IDictionary<string, ConversationReference> OpenedSessions { get; } = new Dictionary<string, ConversationReference>();

        public IDictionary<string, ConversationReference> AttachedSessions { get; } = new Dictionary<string, ConversationReference>();
    }
}
