// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs.Debugging;
using Microsoft.Bot.Builder.Dialogs.Declarative;
using Microsoft.Bot.Builder.Dialogs.Declarative.Converters;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Microsoft.Bot.Runtime.Builders.Middleware;
using Microsoft.Bot.Runtime.Builders.Transcripts;
using Microsoft.Bot.Runtime.Providers.Adapter;
using Microsoft.Bot.Runtime.Providers.Channel;
using Microsoft.Bot.Runtime.Providers.Credentials;
using Microsoft.Bot.Runtime.Providers.Storage;
using Microsoft.Bot.Runtime.Providers.Telemetry;
using Newtonsoft.Json;

namespace Microsoft.Bot.Runtime
{
    public class CoreBotComponentRegistration : ComponentRegistration, IComponentDeclarativeTypes
    {
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

        public IEnumerable<JsonConverter> GetConverters(ResourceExplorer resourceExplorer, SourceContext sourceContext)
        {
            yield return new InterfaceConverter<IMiddlewareBuilder>(resourceExplorer, sourceContext);
            yield return new InterfaceConverter<ITranscriptLoggerBuilder>(resourceExplorer, sourceContext);
            yield return new InterfaceConverter<ITranscriptStoreBuilder>(resourceExplorer, sourceContext);
            yield return new InterfaceConverter<IAdapterProvider>(resourceExplorer, sourceContext);
            yield return new InterfaceConverter<IChannelProvider>(resourceExplorer, sourceContext);
            yield return new InterfaceConverter<ICredentialProvider>(resourceExplorer, sourceContext);
            yield return new InterfaceConverter<IStorageProvider>(resourceExplorer, sourceContext);
            yield return new InterfaceConverter<ITelemetryProvider>(resourceExplorer, sourceContext);
        }
    }
}
