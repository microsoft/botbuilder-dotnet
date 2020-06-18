// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Bot.Builder.Tests
{
    public class MemoryTranscriptTests : TranscriptBaseTests
    {
        public MemoryTranscriptTests()
            : base()
        {
            this.Store = new MemoryTranscriptStore();
            var folder = Path.Combine(Path.GetTempPath(), nameof(MemoryTranscriptTests));
            if (Directory.Exists(folder))
            {
                Directory.Delete(folder, true);
            }
        }

        [Fact]
        public async Task MemoryTranscript_BadArgs()
        {
            await BadArgs();
        }

        [Fact]
        public async Task MemoryTranscript_LogActivity()
        {
            await LogActivity();
        }

        [Fact]
        public async Task MemoryTranscript_LogMultipleActivities()
        {
            await LogMultipleActivities();
        }

        [Fact]
        public async Task MemoryTranscript_GetConversationActivities()
        {
            await GetTranscriptActivities();
        }

        [Fact]
        public async Task MemoryTranscript_GetConversationActivitiesStartDate()
        {
            await GetTranscriptActivitiesStartDate();
        }

        [Fact]
        public async Task MemoryTranscript_ListConversations()
        {
            await ListTranscripts();
        }

        [Fact]
        public async Task MemoryTranscript_DeleteConversation()
        {
            await DeleteTranscript();
        }
    }
}
