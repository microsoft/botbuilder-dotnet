// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Microsoft.Bot.Builder.Dialogs.Debugging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Dialogs.Declarative.Observers
{
    /// <summary>
    /// Wrapper for legacy support of <see cref="IConverterObserver"/>. 
    /// </summary>
    [Obsolete("Deprecated in favor of IJsonLoadObserver.")]
    public class JsonLoadObserverWrapper : IJsonLoadObserver
    {
        private readonly IConverterObserver legacyObserver;

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonLoadObserverWrapper"/> class.
        /// </summary>
        /// <param name="observer">Legacy observer to be wrapped.</param>
        public JsonLoadObserverWrapper(IConverterObserver observer)
        {
            this.legacyObserver = observer ?? throw new ArgumentNullException(nameof(observer));
        }

        /// <inheritdoc cref="IJsonLoadObserver" />
        public bool OnAfterLoadToken<T>(SourceContext context, SourceRange range, JToken token, T obj, out T result) 
            where T : class
        {
            return this.legacyObserver.OnAfterLoadToken<T>(token, obj, out result);
        }

        /// <inheritdoc cref="IJsonLoadObserver" />
        public bool OnBeforeLoadToken<T>(SourceContext context, SourceRange range, JToken token, out T result) 
            where T : class
        {
            return this.legacyObserver.OnBeforeLoadToken<T>(token, out result);   
        }
    }
}
