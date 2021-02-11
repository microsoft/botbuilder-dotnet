// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Builder.Dialogs.Memory.PathResolvers
{
    /// <summary>
    /// Maps %xxx => settings.xxx (aka activeDialog.Instance.xxx).
    /// </summary>
    public class PercentPathResolver : AliasPathResolver
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PercentPathResolver"/> class.
        /// </summary>
        public PercentPathResolver()
            : base(alias: "%", prefix: "class.")
        {
        }
    }
}
