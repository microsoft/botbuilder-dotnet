// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Builder.Dialogs.Choices
{
    /// <summary>
    /// Contains options to control how input is matched against a list of choices.
    /// </summary>
    public class FindChoicesOptions : FindValuesOptions
    {
        /// <summary>
        /// Gets or sets a value indicating whether the choices value will NOT be search over.
        /// The default is <c>false</c>. This is optional.
        /// </summary>
        /// <value>
        /// A <c>true</c> if the choices value will NOT be search over; otherwise <c>false</c>.
        /// </value>
        public bool NoValue { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the title of the choices action will NOT be searched over.
        /// The default is <c>false</c>. This is optional.
        /// </summary>
        /// <value>
        /// A <c>true</c> if the title of the choices action will NOT be searched over; otherwise <c>false</c>.
        /// </value>
        public bool NoAction { get; set; }
    }
}
