// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.AI.Luis
{
    /// <summary>
    /// Strongly typed LUIS builtin OrdinalV2.
    /// </summary>
    public class OrdinalV2
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OrdinalV2"/> class.
        /// </summary>
        /// <param name="offset">Offset from <see cref="RelativeTo"/>.</param>
        /// <param name="relativeTo">Position that anchors offset.</param>
        public OrdinalV2(string relativeTo, long offset)
        {
            RelativeTo = relativeTo;
            Offset = offset;
        }

        /// <summary>
        /// Gets or sets the anchor for the offset.
        /// </summary>
        /// <value>
        /// The base position in a sequence one of <see cref="Anchor"/>.
        /// </value>
        [JsonProperty("relativeTo")]
        public string RelativeTo { get; set; }

        /// <summary>
        /// Gets or sets the offset in the sequence with respect to <see cref="RelativeTo"/>.
        /// </summary>
        /// <value>
        /// Offset in sequence relative to <see cref="RelativeTo"/>.
        /// </value>
        [JsonProperty("offset")]
        public long Offset { get; set; }

        /// <inheritdoc/>
        public override string ToString() => $"OrdinalV2({RelativeTo}, {Offset})";

        /// <summary>
        /// Possible anchors for offsets.
        /// </summary>
#pragma warning disable CA1034 // Nested types should not be visible (we can't change this without breaking binary compat)
        public static class Anchor
#pragma warning restore CA1034 // Nested types should not be visible
        {
            /// <summary>
            /// Constant for Offset anchor type of Current.
            /// </summary>
            public const string Current = "current";

            /// <summary>
            /// Constant for Offset anchor type of End.
            /// </summary>
            public const string End = "end";

            /// <summary>
            /// Constant for Offset anchor type of Start.
            /// </summary>
            public const string Start = "start";
        }
    }
}
