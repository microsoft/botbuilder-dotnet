// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using Microsoft.Bot.Builder.Azure;
using Microsoft.Bot.Builder.Azure.Blobs;
using Microsoft.Bot.Builder.Dialogs.Debugging;
using Microsoft.Bot.Builder.Dialogs.Declarative;
using Microsoft.Bot.Builder.Dialogs.Declarative.Converters;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Runtime
{
    /// <summary>
    /// <see cref="ComponentRegistration"/> implementation for standard bot runtime components.
    /// </summary>
    internal class CoreBotComponentRegistration : ComponentRegistration, IComponentDeclarativeTypes
    {
        /// <summary>
        /// Gets standard bot runtime <see cref="DeclarativeType"/> resources.
        /// </summary>
        /// <param name="resourceExplorer"><see cref="ResourceExplorer"/> with expected path to get all schema resources.</param>
        /// <returns>Adaptive <see cref="DeclarativeType"/> resources.</returns>
        public IEnumerable<DeclarativeType> GetDeclarativeTypes(ResourceExplorer resourceExplorer)
        {
            // Middleware builders
            yield return new DeclarativeType<BlobsStorage>(
                BlobsStorage.Kind);
            yield return new DeclarativeType<CosmosDbPartitionedStorage>(
                CosmosDbPartitionedStorage.Kind);
        }

        /// <summary>
        /// Gets standard bot runtime <see cref="JsonConverter"/> resources.
        /// </summary>
        /// <param name="resourceExplorer"><see cref="ResourceExplorer"/> to use to resolve references.</param>
        /// <param name="sourceContext"><see cref="SourceContext"/> to build debugger source map.</param>
        /// <returns>Adaptive <see cref="JsonConverter"/> resources.</returns>
        public IEnumerable<JsonConverter> GetConverters(ResourceExplorer resourceExplorer, SourceContext sourceContext)
        {
            yield return new InterfaceConverter<IBotFrameworkHttpAdapter>(resourceExplorer, sourceContext);
            yield return new InterfaceConverter<IStorage>(resourceExplorer, sourceContext);
        }
    }
}
