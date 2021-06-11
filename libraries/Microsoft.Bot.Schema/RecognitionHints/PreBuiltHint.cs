// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Schema
{
    /// <summary>
    /// Hint from a built-in definiion.
    /// </summary>
    public class PreBuiltHint : RecognitionHint
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PreBuiltHint"/> class.
        /// </summary>
        /// <param name="name">Name of LU file definition.</param>
        public PreBuiltHint(string name)
            : base("prebuilt", name)
        {
        }

        /// <inheritdoc/>
        public override RecognitionHint Clone()
            => new PreBuiltHint(Name) { Importance = Importance };
    }
}
