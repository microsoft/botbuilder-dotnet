// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Builder;

namespace Microsoft.Bot.Builder.Runtime.Builders.Transcripts
{
    public interface ITranscriptLoggerBuilder : ITranscriptLoggerBuilder<ITranscriptLogger>
    {
    }

    public interface ITranscriptLoggerBuilder<out TTranscriptLogger> : IBuilder<TTranscriptLogger>
        where TTranscriptLogger : ITranscriptLogger
    {
    }
}
