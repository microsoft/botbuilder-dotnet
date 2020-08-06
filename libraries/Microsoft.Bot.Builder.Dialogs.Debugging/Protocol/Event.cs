// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Builder.Dialogs.Debugging.Protocol
{
#pragma warning disable CA1716 // Identifiers should not match keywords (by design and we can't change it without breaking binary compat)
    internal class Event : Message
#pragma warning restore CA1716 // Identifiers should not match keywords
    {
#pragma warning disable SA1300 // Should begin with an uppercase letter.
        public Event(int seq, string @event)
        {
            Seq = seq;
            Type = "event";
            this.@event = @event;
        }

        public string @event { get; set; }
#pragma warning restore SA1300 // Should begin with an uppercase letter.

        public static Event<TBody> From<TBody>(int seq, string @event, TBody body) => new Event<TBody>(seq, @event) { Body = body };
    }
}
