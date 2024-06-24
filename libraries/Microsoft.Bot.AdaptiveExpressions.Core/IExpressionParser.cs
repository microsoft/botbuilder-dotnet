// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.AdaptiveExpressions.Core
{
    /// <summary>
    /// Interface to parse a string into an <see cref="Expression"/>.
    /// </summary>
    public interface IExpressionParser
    {
        /// <summary>
        /// Parse a string into an <see cref="Expression"/>.
        /// </summary>
        /// <param name="expression">Expression to parse.</param>
        /// <returns>The resulting expression.</returns>
        Expression Parse(string expression);
    }
}
