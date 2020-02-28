// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace AdaptiveExpressions.Properties
{
    public interface IExpressionProperty
    {
        /// <summary>
        /// Get value as object.
        /// </summary>
        /// <remarks>Helper methods which allows you to work with the expression property values as purely objects.</remarks>
        /// <param name="data">data to bind to.</param>
        /// <returns>value as object.</returns>
        object GetObject(object data);

        /// <summary>
        /// Try Get value as object.
        /// </summary>
        /// <remarks>Helper methods which allows you to work with the expression property values as purely objects.</remarks>
        /// <param name="data">data.</param>
        /// <returns>Value and error.</returns>
        (object Value, string Error) TryGetObject(object data);

        /// <summary>
        /// Set value as object.
        /// </summary>
        /// <param name="value">object.</param>
        void SetObject(object value);
    }
}
