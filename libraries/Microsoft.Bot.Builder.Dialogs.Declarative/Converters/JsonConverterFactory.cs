// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Builder.Dialogs.Debugging;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Declarative.Converters
{
    /// <summary>
    /// Abstraction that builds <see cref="JsonConverter"/> instances given the <see cref="SourceContext"/>.
    /// </summary>
    public abstract class JsonConverterFactory
    {
        /// <summary>
        /// Builds an instance of <see cref="JsonConverter"/> given the current <see cref="SourceContext"/>.
        /// </summary>
        /// <param name="resourceExplorer">Resource explorer for the factory.</param>
        /// <param name="context"><see cref="SourceContext"/> for the converter.</param>
        /// <returns><see cref="JsonConverter"/> for the current <see cref="SourceContext"/>.</returns>
        public abstract JsonConverter Build(ResourceExplorer resourceExplorer, SourceContext context);
    }
}
