// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Diagnostics.CodeAnalysis;
using Microsoft.Bot.AdaptiveExpressions.Core.Memory;

namespace Microsoft.Bot.AdaptiveExpressions.Core.Properties
{
    /// <summary>
    /// Interface which defines plain object access to the values of an ExpressionProperty.
    /// </summary>
    /// <remarks>    
    /// This interface definition allows reflection to work with ExpressionProperty{T} without having to know the generic parameter type.
    /// </remarks>
    public interface IExpressionProperty
    {
        /// <summary>
        /// Get value as object.
        /// </summary>
        /// <remarks>Helper methods which allows you to work with the expression property values as purely objects.</remarks>
        /// <param name="data">data to bind to.</param>
        /// <returns>value as object.</returns>
        object GetObject(IMemory data);

        /// <summary>
        /// Try Get value as object.
        /// </summary>
        /// <remarks>Helper methods which allows you to work with the expression property values as purely objects.</remarks>
        /// <param name="data">data.</param>
        /// <returns>Value and error.</returns>
        (object Value, string Error) TryGetObject(IMemory data);

        /// <summary>
        /// Set value as object.
        /// </summary>
        /// <param name="value">object.</param>
        void SetObject(object value);
    }
}
