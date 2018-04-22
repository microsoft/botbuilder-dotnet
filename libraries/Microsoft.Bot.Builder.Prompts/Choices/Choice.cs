// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Prompts.Choices
{
    public class Choice
    {
        ///<summary>
        /// Value to return when selected.
        ///</summary>
        public string Value { get; set; }

        ///<summary>
        /// (Optional) action to use when rendering the choice as a suggested action.
        ///</summary>
        public CardAction Action { get; set; }

        ///<summary>
        /// (Optional) list of synonyms to recognize in addition to the value.
        ///</summary>
        public List<string> Synonyms { get; set; }
    }
}
