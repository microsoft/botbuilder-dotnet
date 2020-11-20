// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using Microsoft.Bot.Builder.Runtime.Builders.Middleware;

namespace Microsoft.Bot.Builder.Runtime.Settings
{
    /// <summary>
    /// Defines options to be supplied to <see cref="CoreBotAdapter"/>.
    /// </summary>
    public class CoreBotAdapterOptions
    {
        /// <summary>
        /// Gets the collection of <see cref="IMiddlewareBuilder"/> instances used to construct the
        /// middleware pipeline for the adapter.
        /// </summary>
        /// <value>
        /// The collection of <see cref="IMiddlewareBuilder"/> instances used to construct the
        /// middleware pipeline for the adapter.
        /// </value>
        public IList<IMiddlewareBuilder> Middleware { get; } = new List<IMiddlewareBuilder>();
    }
}
