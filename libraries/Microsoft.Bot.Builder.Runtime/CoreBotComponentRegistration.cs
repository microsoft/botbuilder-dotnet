// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using Microsoft.Bot.Builder.Dialogs.Debugging;
using Microsoft.Bot.Builder.Dialogs.Declarative;
using Microsoft.Bot.Builder.Dialogs.Declarative.Converters;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Microsoft.Bot.Builder.Runtime.Builders.Handlers;
using Microsoft.Bot.Builder.Runtime.Builders.Middleware;
using Microsoft.Bot.Builder.Runtime.Builders.Transcripts;
using Microsoft.Bot.Builder.Runtime.Providers.Adapter;
using Microsoft.Bot.Builder.Runtime.Providers.Channel;
using Microsoft.Bot.Builder.Runtime.Providers.Credentials;
using Microsoft.Bot.Builder.Runtime.Providers.Storage;
using Microsoft.Bot.Builder.Runtime.Providers.Telemetry;
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
            yield return new DeclarativeType<InspectionMiddlewareBuilder>(
                InspectionMiddlewareBuilder.Kind);
            yield return new DeclarativeType<ShowTypingMiddlewareBuilder>(
                ShowTypingMiddlewareBuilder.Kind);
            yield return new DeclarativeType<TelemetryMiddlewareBuilder>(
                TelemetryMiddlewareBuilder.Kind);
            yield return new DeclarativeType<TranscriptLoggerMiddlewareBuilder>(
                TranscriptLoggerMiddlewareBuilder.Kind);
            yield return new DeclarativeType<RemoveRecipientMentionMiddlewareBuilder>(
                RemoveRecipientMentionMiddlewareBuilder.Kind);

            // OnTurnError handler providers
            yield return new DeclarativeType<OnTurnErrorHandlerBuilder>(
                OnTurnErrorHandlerBuilder.Kind);

            // Transcript Logger builders
            yield return new DeclarativeType<FileTranscriptLoggerBuilder>(
                FileTranscriptLoggerBuilder.Kind);
            yield return new DeclarativeType<TraceTranscriptLoggerBuilder>(
                TraceTranscriptLoggerBuilder.Kind);

            // Transcript Store builders
            yield return new DeclarativeType<BlobsTranscriptStoreBuilder>(
                BlobsTranscriptStoreBuilder.Kind);
            yield return new DeclarativeType<MemoryTranscriptStoreBuilder>(
                MemoryTranscriptStoreBuilder.Kind);

            // Adapter providers
            yield return new DeclarativeType<BotCoreAdapterProvider>(
                BotCoreAdapterProvider.Kind);

            // Channel providers
            yield return new DeclarativeType<DeclarativeChannelProvider>(
                DeclarativeChannelProvider.Kind);

            // Credentials providers
            yield return new DeclarativeType<DeclarativeCredentialsProvider>(
                DeclarativeCredentialsProvider.Kind);

            // Storage providers
            yield return new DeclarativeType<BlobStorageProvider>(
                BlobStorageProvider.Kind);
            yield return new DeclarativeType<CosmosDbPartitionedStorageProvider>(
                CosmosDbPartitionedStorageProvider.Kind);
            yield return new DeclarativeType<MemoryStorageProvider>(
                MemoryStorageProvider.Kind);

            // Telemetry providers
            yield return new DeclarativeType<ApplicationInsightsTelemetryProvider>(
                ApplicationInsightsTelemetryProvider.Kind);
        }

        /// <summary>
        /// Gets standard bot runtime <see cref="JsonConverter"/> resources.
        /// </summary>
        /// <param name="resourceExplorer"><see cref="ResourceExplorer"/> to use to resolve references.</param>
        /// <param name="sourceContext"><see cref="SourceContext"/> to build debugger source map.</param>
        /// <returns>Adaptive <see cref="JsonConverter"/> resources.</returns>
        public IEnumerable<JsonConverter> GetConverters(ResourceExplorer resourceExplorer, SourceContext sourceContext)
        {
            yield return new InterfaceConverter<IMiddlewareBuilder>(resourceExplorer, sourceContext);
            yield return new InterfaceConverter<ITranscriptLoggerBuilder>(resourceExplorer, sourceContext);
            yield return new InterfaceConverter<IAdapterProvider>(resourceExplorer, sourceContext);
            yield return new InterfaceConverter<IChannelProvider>(resourceExplorer, sourceContext);
            yield return new InterfaceConverter<ICredentialProvider>(resourceExplorer, sourceContext);
            yield return new InterfaceConverter<IStorageProvider>(resourceExplorer, sourceContext);
            yield return new InterfaceConverter<ITelemetryProvider>(resourceExplorer, sourceContext);
        }
    }
}
