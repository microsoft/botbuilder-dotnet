// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Microsoft.Bot.Builder.Dialogs.Debugging;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Declarative.Converters
{
    /// <summary>
    /// Abstraction that builds <see cref="JsonConverter"/> instances given the <see cref="SourceContext"/> from a simple builder lambda.
    /// </summary>
    public class LambdaJsonConverterFactory : JsonConverterFactory
    {
        private readonly Func<ResourceExplorer, SourceContext, JsonConverter> _converterBuilder;

        /// <summary>
        /// Initializes a new instance of the <see cref="LambdaJsonConverterFactory"/> class.
        /// </summary>
        /// <param name="converterBuilder">Function to build the <see cref="JsonConverter"/>.</param>
        public LambdaJsonConverterFactory(Func<ResourceExplorer, SourceContext, JsonConverter> converterBuilder)
        {
            _converterBuilder = converterBuilder ?? throw new ArgumentNullException(nameof(converterBuilder));
        }

        /// <inheritdoc/>
        public override JsonConverter Build(ResourceExplorer resourceExplorer, SourceContext context)
        {
            return _converterBuilder(resourceExplorer, context);
        }
    }
}
