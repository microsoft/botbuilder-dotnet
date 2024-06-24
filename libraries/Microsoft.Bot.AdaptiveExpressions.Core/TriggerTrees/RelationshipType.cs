// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.AdaptiveExpressions.Core.TriggerTrees
{
    /// <summary>
    /// Relationship between trigger expressions.
    /// </summary>
    public enum RelationshipType
    {
        /// <summary>
        /// First argument specializes the second, i.e. applies to a subset of the states the second argument covers.
        /// </summary>
        Specializes,

        /// <summary>
        /// First and second argument are the same expression.
        /// </summary>
        Equal,

        /// <summary>
        /// First argument generalizes the second, i.e. applies to a superset of the states the second argument covers.
        /// </summary>
        Generalizes,

        /// <summary>
        /// Cannot tell how the first and second arguments relate.
        /// </summary>
        Incomparable
    }
}
