// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Runtime.Component
{
    /// <summary>
    /// Provides an interface for retrieving a collection of bot components from a given source.
    /// </summary>
    internal interface IBotComponentEnumerator
    {
        /// <summary>
        /// Get available bot components.
        /// </summary>
        /// <param name="componentName">Bot component identifier used to retrieve applicable components.</param>
        /// <returns>A collection of available bot components for the specified component name.</returns>
        IEnumerable<BotComponent> GetComponents(string componentName);
    }
}
