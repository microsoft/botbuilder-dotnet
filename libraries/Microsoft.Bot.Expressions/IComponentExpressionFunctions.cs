// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.
using System.Collections.Generic;

namespace Microsoft.Bot.Expressions
{
    /// <summary>
    /// Interface for adding custom functions to the expression engine.
    /// </summary>
    public interface IComponentExpressionFunctions
    {
        /// <summary>
        /// Return collection of ExpressionEvaluators.
        /// </summary>
        /// <returns>enumeration of custom ExpressionEvaluators.</returns>
        public IEnumerable<ExpressionEvaluator> GetExpressionEvaluators();
    }
}
