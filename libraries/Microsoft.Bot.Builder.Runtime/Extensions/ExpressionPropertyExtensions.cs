// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using AdaptiveExpressions.Properties;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Bot.Builder.Runtime.Extensions
{
    /// <summary>
    /// Defines extension methods for <see cref="ExpressionProperty{T}"/>.
    /// </summary>
    public static class ExpressionPropertyExtensions
    {
        /// <summary>
        /// Computes a value by applying an <see cref="ExpressionProperty{T}"/> to <see cref="IConfiguration"/>.
        /// </summary>
        /// <typeparam name="T">The type of the value to be evaluated from the adaptive expression.</typeparam>
        /// <param name="property">The adaptive expression to be evaluated.</param>
        /// <param name="configuration">The application configuration.</param>
        /// <returns>A value of type <typeparamref name="T"/> as evaluated from the specified expression.</returns>
        public static T GetConfigurationValue<T>(this ExpressionProperty<T> property, IConfiguration configuration)
        {
            if (property == null)
            {
                throw new ArgumentNullException(nameof(property));
            }

            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            return property.GetValue(new ConfigurationMemory(configuration));
        }
    }
}
