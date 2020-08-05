// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Builder.Dialogs.Debugging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Dialogs.Declarative.Observers
{
    /// <summary>
    /// Observer for <see cref="JsonConverter"/> instances. Handles notifications around the 
    /// object loading lifecycle, including the ability to intercept and provide alternate values
    /// to be considered by the converters.
    /// </summary>
    public interface IJsonLoadObserver
    {
        /// <summary>
        /// Notifies <see cref="IConverterObserver"/> instances before type-loading a <see cref="JToken"/>.
        /// </summary>
        /// <typeparam name="T">Type of the concrete object to be built.</typeparam>
        /// <param name="context">Source scope.</param>
        /// <param name="range">Source range.</param>
        /// <param name="token">Token to be used to build the object.</param>
        /// <param name="result">Output parameter for observer to provide its result to the converter.</param>
        /// <returns>True if the observer provides a result and False if not.</returns>
        bool OnBeforeLoadToken<T>(SourceContext context, SourceRange range, JToken token, out T result)
            where T : class;

        /// <summary>
        /// Notifies <see cref="IConverterObserver"/> instances after type-loading a <see cref="JToken"/> into the 
        /// provided instance of <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">Type of the concrete object that was built.</typeparam>
        /// <param name="context">Source scope.</param>
        /// <param name="range">Source Range.</param>
        /// <param name="token">Token used to build the object.</param>
        /// <param name="obj">Object that was built using the token.</param>
        /// <param name="result">Output parameter for observer to provide its result to the converter.</param>
        /// <returns>True if the observer provides a result and False if not.</returns>
        bool OnAfterLoadToken<T>(SourceContext context, SourceRange range, JToken token, T obj, out T result)
            where T : class;
    }
}
