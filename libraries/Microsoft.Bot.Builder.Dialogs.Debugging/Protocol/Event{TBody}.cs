// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Builder.Dialogs.Debugging.Protocol
{
#pragma warning disable CA1716 // Identifiers should not match keywords (by design and we can't change it without breaking binary compat)
    internal class Event<TBody> : Event
#pragma warning restore CA1716 // Identifiers should not match keywords
    {
        public Event(int seq, string @event)
            : base(seq, @event)
        {
        }

        public TBody Body { get; set; }
    }
}
