// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.AI.QnA
{
    /// <summary>
    /// Compound Filter for Metadata Join Expression.
    /// </summary>
    public enum StrictFiltersCompoundOperationType
    {
        /// <summary>
        /// Default Search Filter Operation Type, AND.
        /// </summary>
        AND,

        /// <summary>
        /// Search Filter Operation Type OR.
        /// </summary>
        OR
    }
}
