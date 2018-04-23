// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Builder.Prompts.Choices
{
    public class FindChoicesOptions: FindValuesOptions
    {
        /// <summary>
        /// (Optional) If `true`, the choices value will NOT be search over. The default is `false`.
        /// </summary>
        public bool NoValue { get; set; }

        /// <summary>
        /// (Optional) If `true`, the title of the choices action will NOT be searched over.The default is `false`.
        /// </summary>
        public bool NoAction { get; set; }
    }
}
