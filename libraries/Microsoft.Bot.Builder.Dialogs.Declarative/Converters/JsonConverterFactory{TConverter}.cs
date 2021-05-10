// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Builder.Dialogs.Debugging;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Declarative.Converters
{
    /// <summary>
    /// Abstraction that builds <see cref="JsonConverter"/> instances for a JsonConverter for type <typeparamref name="TConverter"/> when <typeparamref name="TConverter"/>  is independent of the <see cref="SourceContext"/>.
    /// </summary>
    /// <typeparam name="TConverter">Type of class that the built <see cref="JsonConverter"/> will handle during Json deserialization.</typeparam>
    public class JsonConverterFactory<TConverter> : JsonConverterFactory
        where TConverter : JsonConverter, new()
    {
        /// <summary>
        /// Builds an instance of <see cref="JsonConverter"/> given the current <see cref="SourceContext"/>.
        /// </summary>
        /// <param name="resourceExplorer">Resource explorer for the factory.</param>
        /// <param name="context"><see cref="SourceContext"/> for the converter.</param>
        /// <returns><see cref="JsonConverter"/> of type <typeparamref name="TConverter"/> created using the default constructor.</returns>
        public override JsonConverter Build(ResourceExplorer resourceExplorer, SourceContext context)
        {
            return new TConverter();
        }
    }
}
