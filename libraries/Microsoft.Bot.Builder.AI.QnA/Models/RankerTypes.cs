// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Builder.AI.QnA
{
    /// <summary>
    /// Enumeration of types of ranking.
    /// </summary>
#pragma warning disable CA1052 // Static holder types should be Static or NotInheritable (we can't change this without breaking binary compat)
    public class RankerTypes
#pragma warning restore CA1052 // Static holder types should be Static or NotInheritable
    {
        /// <summary>
        /// Default Ranker Behaviour. i.e. Ranking based on Questions and Answer.
        /// </summary>
        public const string DefaultRankerType = "Default";

        /// <summary>
        /// Ranker based on question Only.
        /// </summary>
        public const string QuestionOnly = "QuestionOnly";

        /// <summary>
        /// Ranker based on Autosuggest for question field Only.
        /// </summary>
        public const string AutoSuggestQuestion = "AutoSuggestQuestion";
    }
}
